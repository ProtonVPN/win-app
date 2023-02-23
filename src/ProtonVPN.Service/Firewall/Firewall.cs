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
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.FirewallLogs;
using ProtonVPN.NetworkFilter;
using ProtonVPN.Service.Driver;
using Action = ProtonVPN.NetworkFilter.Action;

namespace ProtonVPN.Service.Firewall
{
    internal class Firewall : IFirewall, IStartable
    {
        private readonly ILogger _logger;
        private readonly IDriver _calloutDriver;
        private readonly IConfiguration _config;
        private readonly IpLayer _ipLayer;
        private readonly IpFilter _ipFilter;
        private FirewallParams _lastParams = FirewallParams.Empty;
        private bool _dnsCalloutFiltersAdded;

        private readonly List<ValueTuple<string, List<Guid>>> _serverAddressFilters = new();
        private readonly List<FirewallItem> _firewallItems = new();

        private const int DnsUdpPort = 53;
        private const int DhcpUdpPort = 67;

        public Firewall(
            ILogger logger,
            IDriver calloutDriver,
            IConfiguration config,
            IpLayer ipLayer,
            IpFilter ipFilter)
        {
            _logger = logger;
            _config = config;
            _ipLayer = ipLayer;
            _ipFilter = ipFilter;
            _calloutDriver = calloutDriver;
        }

        public bool LeakProtectionEnabled { get; private set; }

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
            PermitOpenVpnServerAddress(firewallParams);
            ApplyFilters(firewallParams);
            _lastParams = firewallParams;
        }

        public void DisableLeakProtection()
        {
            try
            {
                _logger.Info<FirewallLog>("Restoring internet");

                _ipFilter.DynamicSublayer.DestroyAllFilters();
                _ipFilter.PermanentSublayer.DestroyAllFilters();
                _serverAddressFilters.Clear();
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
                _lastParams = firewallParams;
                return;
            }

            if (firewallParams.SessionType != _lastParams.SessionType)
            {
                List<Guid> previousVariableFilters = GetFirewallGuidsByType(FirewallItemType.VariableFilter);
                List<Guid> previousInterfaceFilters = GetFirewallGuidsByType(FirewallItemType.PermitInterfaceFilter);

                ApplyFilters(firewallParams);

                RemoveItems(previousVariableFilters, _lastParams.SessionType);
                RemoveItems(previousInterfaceFilters, _lastParams.SessionType);
            }

            if (firewallParams.AddInterfaceFilters && firewallParams.InterfaceIndex != _lastParams.InterfaceIndex)
            {
                List<Guid> previousGuids = GetFirewallGuidsByType(FirewallItemType.PermitInterfaceFilter);
                PermitFromNetworkInterface(4, firewallParams);
                RemoveItems(previousGuids, _lastParams.SessionType);

                previousGuids = GetFirewallGuidsByType(FirewallItemType.DnsCalloutFilter);
                _dnsCalloutFiltersAdded = false;
                CreateDnsCalloutFilter(4, firewallParams);
                RemoveItems(previousGuids, _lastParams.SessionType);
            }

            if (firewallParams.DnsLeakOnly != _lastParams.DnsLeakOnly)
            {
                if (firewallParams.DnsLeakOnly)
                {
                    RemoveItems(GetFirewallGuidsByType(FirewallItemType.VariableFilter), _lastParams.SessionType);
                }
                else
                {
                    EnableBaseLeakProtection(firewallParams);
                }
            }

            PermitOpenVpnServerAddress(firewallParams);
            _lastParams = firewallParams;
        }

        private void HandlePermanentStateAfterReboot(FirewallParams firewallParams)
        {
            _calloutDriver.Start();
            CreateDnsCalloutFilter(4, firewallParams);
            PermitFromNetworkInterface(4, firewallParams);
            PermitOpenVpnServerAddress(firewallParams);
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

        private List<Guid> GetFirewallGuidsByType(FirewallItemType type)
        {
            return _firewallItems
                .Where(item => type == item.ItemType)
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

            PermitIpv4Loopback(2, firewallParams);
            PermitIpv6Loopback(2, firewallParams);
            PermitPrivateNetwork(2, firewallParams);

            BlockAllIpv4Network(1, firewallParams);
            BlockAllIpv6Network(1, firewallParams);
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
                    DnsUdpPort,
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
                    DnsUdpPort,
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
                    DnsUdpPort,
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
                    DnsUdpPort,
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
                    DnsUdpPort);
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
                    DhcpUdpPort,
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
                    new DisplayData("ProtonVPN permit VPN tunnel", "Permit TAP adapter traffic"),
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
                    new DisplayData("ProtonVPN permit VPN tunnel", "Permit TAP adapter traffic"),
                    Action.SoftPermit,
                    layer,
                    firewallParams.InterfaceIndex,
                    weight,
                    persistent: false);
                _firewallItems.Add(new FirewallItem(FirewallItemType.PermitInterfaceFilter, guid));
            });
        }

        private void PermitOpenVpnServerAddress(FirewallParams firewallParams)
        {
            if (string.IsNullOrEmpty(firewallParams.ServerIp))
            {
                return;
            }

            ReorderServerPermitFilters(firewallParams.ServerIp);

            var filterGuids = new List<Guid>();

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

            _serverAddressFilters.Add((firewallParams.ServerIp, filterGuids));

            DeletePreviousServerPermitFilters();
        }

        private void ReorderServerPermitFilters(string serverIp)
        {
            if (_serverAddressFilters.Count == 0)
            {
                return;
            }

            int index = 0;
            (string, List<Guid>)? item = null;

            foreach ((string, List<Guid>) filter in _serverAddressFilters)
            {
                if (filter.Item1 == serverIp)
                {
                    item = filter;
                    break;
                }

                index++;
            }

            if (item != null)
            {
                _serverAddressFilters.RemoveAt(index);
                _serverAddressFilters.Add(item.Value);
            }
        }

        private void DeletePreviousServerPermitFilters()
        {
            if (_serverAddressFilters.Count >= 3)
            {
                (string oldAddress, List<Guid> guids) = _serverAddressFilters.FirstOrDefault();
                if (guids == null)
                {
                    return;
                }

                //Use permanent session here to be able to remove filters created
                //on both dynamic and permanent sublayers.
                DeleteIpFilters(guids, SessionType.Permanent);
                _serverAddressFilters.RemoveAll(tuple => tuple.Item1 == oldAddress);
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
                    _firewallItems.Add(new FirewallItem(FirewallItemType.VariableFilter, guid));
                });
            }
        }

        private void PermitFromProcesses(uint weight, FirewallParams firewallParams)
        {
            List<string> processes = new List<string>
            {
                _config.AppExePath,
                _config.ServiceExePath,
                _config.WireGuard.ServicePath,
            };

            foreach (string processPath in processes)
            {
                _ipLayer.ApplyToIpv4(layer =>
                {
                    Guid guid = _ipFilter.GetSublayer(firewallParams.SessionType).CreateAppFilter(
                        new DisplayData("ProtonVPN permit app", "Permit ProtonVPN app to bypass VPN tunnel"),
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
}