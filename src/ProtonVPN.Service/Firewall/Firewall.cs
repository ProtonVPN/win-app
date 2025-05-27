/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.FirewallLogs;
using ProtonVPN.NetworkFilter;
using ProtonVPN.Service.Driver;
using Action = ProtonVPN.NetworkFilter.Action;

namespace ProtonVPN.Service.Firewall;

internal class Firewall : IFirewall, IStartable
{
    private const string PERMIT_APP_FILTER_NAME = "ProtonVPN permit app";
    private const int LOCAL_TRAFFIC_WEIGHT = 2;

    private readonly ILogger _logger;
    private readonly IDriver _calloutDriver;
    private readonly IStaticConfiguration _staticConfig;
    private readonly IpLayer _ipLayer;
    private readonly IpFilter _ipFilter;
    private FirewallParams _lastParams = FirewallParams.Empty;
    private bool _dnsCalloutFiltersAdded;

    private readonly List<ServerAddressFilterCollection> _serverAddressFilterCollection = new();
    private readonly List<FirewallItem> _firewallItems = new();

    private const int DNS_UDP_PORT = 53;
    private const int DHCP_UDP_PORT = 67;

    public Firewall(
        ILogger logger,
        IDriver calloutDriver,
        IStaticConfiguration staticConfig,
        IpLayer ipLayer,
        IpFilter ipFilter)
    {
        _logger = logger;
        _staticConfig = staticConfig;
        _ipLayer = ipLayer;
        _ipFilter = ipFilter;
        _calloutDriver = calloutDriver;
    }

    public bool LeakProtectionEnabled { get; private set; }

    public bool? IsLocalAreaNetworkAccessEnabled => _lastParams?.IsLocalAreaNetworkAccessEnabled;

    public void Start()
    {
        if (_ipFilter.PermanentSublayer.GetFilterCount() > 0)
        {
            _lastParams = new()
            {
                ServerIp = string.Empty,
                Persistent = true,
                PermanentStateAfterReboot = true,
            };
            LeakProtectionEnabled = true;

            _logger.Info<FirewallLog>("Detected permanent filters. Trying to recreate process permit filters.");

            //In case the app was launched after update,
            //we need to recreate permit from process filters since paths have changed due to version folder.
            _ipFilter.PermanentSublayer.DestroyFiltersByName(PERMIT_APP_FILTER_NAME);
            PermitFromProcesses(4, _lastParams);
        }
    }

    public void EnableLeakProtection(FirewallParams firewallParams)
    {
        if (LeakProtectionEnabled)
        {
            ApplyChange(firewallParams);
            return;
        }

        _calloutDriver.Start();
        PermitServerAddress(firewallParams);
        ApplyFilters(firewallParams);
        SetLastParams(firewallParams);
    }

    public void DisableLeakProtection()
    {
        try
        {
            _logger.Info<FirewallLog>("Restoring internet");

            _ipFilter.DynamicSublayer.DestroyAllFilters();
            _ipFilter.PermanentSublayer.DestroyAllFilters();
            _serverAddressFilterCollection.Clear();
            _firewallItems.Clear();
            LeakProtectionEnabled = false;
            _dnsCalloutFiltersAdded = false;
            _calloutDriver.Stop();
            _lastParams = FirewallParams.Empty;

            _logger.Info<FirewallLog>("Internet restored");
        }
        catch (NetworkFilterException ex)
        {
            _logger.Error<FirewallLog>("An error occurred when deleting the network filters.", ex);
        }
    }

    private void ApplyFilters(FirewallParams firewallParams)
    {
        try
        {
            _logger.Info<FirewallLog>("Blocking internet");

            EnableDnsLeakProtection(firewallParams);

            if (!firewallParams.DnsLeakOnly)
            {
                EnableBaseLeakProtection(firewallParams);
            }

            LeakProtectionEnabled = true;

            _logger.Info<FirewallLog>("Internet blocked");
        }
        catch (NetworkFilterException ex)
        {
            _logger.Error<FirewallLog>("An error occurred when applying the network filters.", ex);
        }
    }

    private void ApplyChange(FirewallParams firewallParams)
    {
        if (_lastParams.PermanentStateAfterReboot)
        {
            HandlePermanentStateAfterReboot(firewallParams);
            SetLastParams(firewallParams);
            return;
        }

        if (firewallParams.SessionType != _lastParams.SessionType)
        {
            List<Guid> previousFilters = GetFirewallGuidsByTypes(FirewallItemType.VariableFilter, FirewallItemType.LocalNetworkFilter);
            List<Guid> previousInterfaceFilters = GetFirewallGuidsByTypes(FirewallItemType.PermitInterfaceFilter);

            ApplyFilters(firewallParams);

            RemoveItems(previousFilters, _lastParams.SessionType);
            RemoveItems(previousInterfaceFilters, _lastParams.SessionType);
        }

        if (firewallParams.AddInterfaceFilters && firewallParams.InterfaceIndex != _lastParams.InterfaceIndex)
        {
            List<Guid> previousGuids = GetFirewallGuidsByTypes(FirewallItemType.PermitInterfaceFilter);
            PermitFromNetworkInterface(4, firewallParams);
            RemoveItems(previousGuids, _lastParams.SessionType);

            previousGuids = GetFirewallGuidsByTypes(FirewallItemType.DnsCalloutFilter);
            _dnsCalloutFiltersAdded = false;
            CreateDnsCalloutFilter(4, firewallParams);
            RemoveItems(previousGuids, _lastParams.SessionType);
        }

        if (firewallParams.DnsLeakOnly != _lastParams.DnsLeakOnly)
        {
            if (firewallParams.DnsLeakOnly)
            {
                RemoveItems(GetFirewallGuidsByTypes(FirewallItemType.VariableFilter, FirewallItemType.LocalNetworkFilter), _lastParams.SessionType);
                RemoveItems(GetFirewallGuidsByTypes(FirewallItemType.BlockOutsideOpenVpnFilter), _lastParams.SessionType);
            }
            else
            {
                EnableBaseLeakProtection(firewallParams);
            }
        }

        if (firewallParams.IsLocalAreaNetworkAccessEnabled != _lastParams.IsLocalAreaNetworkAccessEnabled)
        {
            if (firewallParams.IsLocalAreaNetworkAccessEnabled)
            {
                PermitPrivateNetwork(LOCAL_TRAFFIC_WEIGHT, firewallParams);
            }
            else
            {
                RemoveItems(GetFirewallGuidsByTypes(FirewallItemType.LocalNetworkFilter), _lastParams.SessionType);
            }
        }

        PermitServerAddress(firewallParams);
        BlockOutsideOpenVpnTraffic(firewallParams);
        SetLastParams(firewallParams);
    }

    private void SetLastParams(FirewallParams firewallParams)
    {
        //This is needed due to WireGuard, because we don't know the interface index in advance.
        uint interfaceIndex = 0;
        if (_lastParams.InterfaceIndex > 0 && firewallParams.InterfaceIndex == 0)
        {
            interfaceIndex = _lastParams.InterfaceIndex;
        }

        _lastParams = firewallParams;
        if (interfaceIndex > 0)
        {
            _lastParams.InterfaceIndex = interfaceIndex;
        }
    }

    private void HandlePermanentStateAfterReboot(FirewallParams firewallParams)
    {
        _calloutDriver.Start();
        CreateDnsCalloutFilter(4, firewallParams);
        PermitFromNetworkInterface(4, firewallParams);
        PermitServerAddress(firewallParams);
    }

    private void RemoveItems(List<Guid> guids, SessionType sessionType)
    {
        DeleteIpFilters(guids, sessionType);
        List<FirewallItem> firewallItems = _firewallItems.Where(item => guids.Contains(item.Guid)).ToList();

        foreach (FirewallItem item in firewallItems)
        {
            _firewallItems.Remove(item);
        }
    }

    private List<Guid> GetFirewallGuidsByTypes(params FirewallItemType[] firewallItemTypes)
    {
        return _firewallItems
            .Where(item => firewallItemTypes.Contains(item.ItemType))
            .Select(item => item.Guid)
            .ToList();
    }

    private void EnableDnsLeakProtection(FirewallParams firewallParams)
    {
        BlockDns(3, firewallParams);
        CreateDnsCalloutFilter(4, firewallParams);
    }

    private void EnableBaseLeakProtection(FirewallParams firewallParams)
    {
        PermitDhcp(4, firewallParams);
        PermitFromNetworkInterface(4, firewallParams);
        PermitFromProcesses(4, firewallParams);

        PermitIpv4Loopback(LOCAL_TRAFFIC_WEIGHT, firewallParams);
        PermitIpv6Loopback(LOCAL_TRAFFIC_WEIGHT, firewallParams);
        PermitPrivateNetwork(LOCAL_TRAFFIC_WEIGHT, firewallParams);

        BlockAllIpv4Network(1, firewallParams);
        BlockAllIpv6Network(1, firewallParams);
        BlockOutsideOpenVpnTraffic(firewallParams);
    }

    private void BlockOutsideOpenVpnTraffic(FirewallParams firewallParams)
    {
        if (string.IsNullOrEmpty(firewallParams.ServerIp) || firewallParams.DnsLeakOnly)
        {
            return;
        }

        List<Guid> filters = GetFirewallGuidsByTypes(FirewallItemType.BlockOutsideOpenVpnFilter);
        if (filters.Count > 0)
        {
            RemoveItems(filters, firewallParams.SessionType);
        }

        _ipLayer.ApplyToIpv4(layer =>
        {
            Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).BlockOutsideOpenVpn(
                new DisplayData("ProtonVPN block outside OpenVPN traffic",
                    "Blocks outgoing traffic to VPN server if when the process is not openvpn.exe"),
                layer,
                weight: 1,
                _staticConfig.OpenVpn.ExePath,
                firewallParams.ServerIp,
                firewallParams.Persistent);
            _firewallItems.Add(new FirewallItem(FirewallItemType.BlockOutsideOpenVpnFilter, guid));
        });
    }

    private void BlockDns(uint weight, FirewallParams firewallParams)
    {
        _ipLayer.ApplyToIpv4(layer =>
        {
            Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateRemoteUdpPortFilter(new DisplayData(
                    "ProtonVPN DNS filter", "Block UDP 53 port"),
                Action.HardBlock,
                layer,
                weight,
                DNS_UDP_PORT,
                firewallParams.Persistent);
            _firewallItems.Add(new FirewallItem(FirewallItemType.VariableFilter, guid));
        });

        _ipLayer.ApplyToIpv4(layer =>
        {
            Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateRemoteTcpPortFilter(new DisplayData(
                    "ProtonVPN block DNS", "Block TCP 53 port"),
                Action.HardBlock,
                layer,
                weight,
                DNS_UDP_PORT,
                firewallParams.Persistent);
            _firewallItems.Add(new FirewallItem(FirewallItemType.VariableFilter, guid));
        });

        _ipLayer.ApplyToIpv6(layer =>
        {
            Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateRemoteTcpPortFilter(new DisplayData(
                    "ProtonVPN block DNS", "Block TCP 53 port"),
                Action.HardBlock,
                layer,
                weight,
                DNS_UDP_PORT,
                firewallParams.Persistent);
            _firewallItems.Add(new FirewallItem(FirewallItemType.VariableFilter, guid));
        });

        _ipLayer.ApplyToIpv6(layer =>
        {
            Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateRemoteUdpPortFilter(new DisplayData(
                    "ProtonVPN block DNS", "Block UDP 53 port"),
                Action.HardBlock,
                layer,
                weight,
                DNS_UDP_PORT,
                firewallParams.Persistent);
            _firewallItems.Add(new FirewallItem(FirewallItemType.VariableFilter, guid));
        });
    }

    private void CreateDnsCalloutFilter(uint weight, FirewallParams firewallParams)
    {
        if (_dnsCalloutFiltersAdded || !firewallParams.AddInterfaceFilters)
        {
            return;
        }

        Guid guid = _ipFilter.DynamicSublayer.BlockOutsideDns(
            new DisplayData("ProtonVPN block DNS", "Block outside dns"),
            Layer.OutboundIPPacketV4,
            weight,
            IpFilter.DnsCalloutGuid,
            firewallParams.InterfaceIndex);
        _firewallItems.Add(new FirewallItem(FirewallItemType.DnsCalloutFilter, guid));

        _ipLayer.ApplyToIpv4(layer =>
        {
            guid = _ipFilter.DynamicSublayer.CreateRemoteUdpPortFilter(
                new DisplayData("ProtonVPN DNS filter", "Permit UDP 53 port so we can block it at network layer"),
                Action.HardPermit,
                layer,
                weight,
                DNS_UDP_PORT);
            _firewallItems.Add(new FirewallItem(FirewallItemType.DnsFilter, guid));
        });

        _dnsCalloutFiltersAdded = true;
    }

    private void PermitDhcp(uint weight, FirewallParams firewallParams)
    {
        _ipLayer.ApplyToIpv4(layer =>
        {
            Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateRemoteUdpPortFilter(
                new DisplayData("ProtonVPN permit DHCP", "Permit 67 UDP port"),
                Action.SoftPermit,
                layer,
                weight,
                DHCP_UDP_PORT,
                firewallParams.Persistent);
            _firewallItems.Add(new FirewallItem(FirewallItemType.VariableFilter, guid));
        });
    }

    private void PermitFromNetworkInterface(uint weight, FirewallParams firewallParams)
    {
        if (!firewallParams.AddInterfaceFilters)
        {
            return;
        }

        //Create the following filters dynamically on permanent or dynamic sublayer,
        //but prevent keeping them after reboot, as interface index might be changed.
        _ipLayer.ApplyToIpv4(layer =>
        {
            Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateNetInterfaceFilter(
                new DisplayData("ProtonVPN permit VPN tunnel", "Permit tunnel interface traffic"),
                Action.SoftPermit,
                layer,
                firewallParams.InterfaceIndex,
                weight,
                persistent: false);
            _firewallItems.Add(new FirewallItem(FirewallItemType.PermitInterfaceFilter, guid));
        });

        _ipLayer.ApplyToIpv6(layer =>
        {
            Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateNetInterfaceFilter(
                new DisplayData("ProtonVPN permit VPN tunnel", "Permit tunnel interface traffic"),
                Action.SoftPermit,
                layer,
                firewallParams.InterfaceIndex,
                weight,
                persistent: false);
            _firewallItems.Add(new FirewallItem(FirewallItemType.PermitInterfaceFilter, guid));
        });
    }

    private void PermitServerAddress(FirewallParams firewallParams)
    {
        if (string.IsNullOrEmpty(firewallParams.ServerIp))
        {
            return;
        }

        ReorderServerPermitFilters(firewallParams.ServerIp);

        List<Guid> filterGuids = new();

        _ipLayer.ApplyToIpv4(layer =>
        {
            filterGuids.Add(_ipFilter.GetSublayer(firewallParams.SessionType).CreateRemoteIPv4Filter(
                new DisplayData("ProtonVPN permit OpenVPN server", "Permit server ip"),
                Action.HardPermit,
                layer,
                1,
                firewallParams.ServerIp,
                persistent: false));
        });

        _serverAddressFilterCollection.Add(new ServerAddressFilterCollection
        {
            ServerIp = firewallParams.ServerIp,
            SessionType = firewallParams.SessionType,
            Filters = filterGuids,
        });

        DeleteServerPermitFilters(firewallParams);
    }

    private void ReorderServerPermitFilters(string serverIp)
    {
        if (_serverAddressFilterCollection.Count == 0)
        {
            return;
        }

        int index = 0;
        ServerAddressFilterCollection item = null;

        foreach (ServerAddressFilterCollection collection in _serverAddressFilterCollection)
        {
            if (collection.ServerIp == serverIp)
            {
                item = collection;
                break;
            }

            index++;
        }

        if (item != null)
        {
            _serverAddressFilterCollection.RemoveAt(index);
            _serverAddressFilterCollection.Add(item);
        }
    }

    private void DeleteServerPermitFilters(FirewallParams firewallParams)
    {
        if (_serverAddressFilterCollection.Count >= 3)
        {
            ServerAddressFilterCollection serverAddressFilterCollection = _serverAddressFilterCollection.FirstOrDefault();
            if (serverAddressFilterCollection == null || serverAddressFilterCollection.Filters?.Count == 0)
            {
                return;
            }

            //Use permanent session here to be able to remove filters created
            //on both dynamic and permanent sublayers.
            DeleteIpFilters(serverAddressFilterCollection.Filters, SessionType.Permanent);
            _serverAddressFilterCollection.Remove(serverAddressFilterCollection);
        }

        //If session type changes, we need to remove previous permit filters from dynamic/persistent sublayer.
        if (_lastParams.SessionType != firewallParams.SessionType)
        {
            foreach (ServerAddressFilterCollection serverAddressFilters in _serverAddressFilterCollection.ToList())
            {
                if (serverAddressFilters.SessionType == _lastParams.SessionType)
                {
                    DeleteIpFilters(serverAddressFilters.Filters, _lastParams.SessionType);
                    _serverAddressFilterCollection.Remove(serverAddressFilters);
                }
            }
        }
    }

    private void DeleteIpFilters(List<Guid> guids, SessionType sessionType)
    {
        foreach (Guid guid in guids)
        {
            _ipFilter.GetSublayer(sessionType).DestroyFilter(guid);
        }
    }

    private void BlockAllIpv4Network(uint weight, FirewallParams firewallParams)
    {
        _ipLayer.ApplyToIpv4(layer =>
        {
            Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateLayerFilter(
                new DisplayData("ProtonVPN block IPv4", "Block all IPv4 traffic"),
                Action.SoftBlock,
                layer,
                weight,
                firewallParams.Persistent);
            _firewallItems.Add(new FirewallItem(FirewallItemType.VariableFilter, guid));
        });
    }

    private void BlockAllIpv6Network(uint weight, FirewallParams firewallParams)
    {
        _ipLayer.ApplyToIpv6(layer =>
        {
            Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateLayerFilter(
                new DisplayData("ProtonVPN block IPv6", "Block all IPv6 traffic"),
                Action.SoftBlock,
                layer,
                weight,
                firewallParams.Persistent);
            _firewallItems.Add(new FirewallItem(FirewallItemType.VariableFilter, guid));
        });
    }

    private void PermitIpv4Loopback(uint weight, FirewallParams firewallParams)
    {
        _ipLayer.ApplyToIpv4(layer =>
        {
            Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateLoopbackFilter(
                new DisplayData("ProtonVPN permit IPv4 loopback", "Permit IPv4 loopback traffic"),
                Action.HardPermit,
                layer,
                weight,
                firewallParams.Persistent);
            _firewallItems.Add(new FirewallItem(FirewallItemType.VariableFilter, guid));
        });
    }

    private void PermitIpv6Loopback(uint weight, FirewallParams firewallParams)
    {
        _ipLayer.ApplyToIpv6(layer =>
        {
            Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateLoopbackFilter(
                new DisplayData("ProtonVPN permit IPv6 loopback", "Permit IPv6 loopback traffic"),
                Action.HardPermit,
                layer,
                weight,
                firewallParams.Persistent);
            _firewallItems.Add(new FirewallItem(FirewallItemType.VariableFilter, guid));
        });
    }

    private void PermitPrivateNetwork(uint weight, FirewallParams firewallParams)
    {
        if (!firewallParams.IsLocalAreaNetworkAccessEnabled)
        {
            return;
        }

        List<NetworkAddress> networkAddresses = new()
        {
            new("10.0.0.0", "255.0.0.0"),
            new("169.254.0.0", "255.255.0.0"),
            new("172.16.0.0", "255.240.0.0"),
            new("192.168.0.0", "255.255.0.0"),
            new("224.0.0.0", "240.0.0.0"),
            new("255.255.255.255", "255.255.255.255")
        };

        foreach (NetworkAddress networkAddress in networkAddresses)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateRemoteNetworkIPv4Filter(
                    new DisplayData("ProtonVPN permit private network", ""),
                    Action.HardPermit,
                    layer,
                    weight,
                    networkAddress,
                    firewallParams.Persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.LocalNetworkFilter, guid));
            });
        }
    }

    private void PermitFromProcesses(uint weight, FirewallParams firewallParams)
    {
        List<string> processes = new()
        {
            _staticConfig.ClientExePath,
            _staticConfig.ServiceExePath,
            _staticConfig.WireGuard.ServicePath,
        };

        foreach (string processPath in processes)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateAppFilter(
                    new DisplayData(PERMIT_APP_FILTER_NAME, "Permit ProtonVPN app to bypass VPN tunnel"),
                    Action.HardPermit,
                    layer,
                    weight,
                    processPath,
                    firewallParams.Persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.VariableFilter, guid));
            });
        }
    }
}