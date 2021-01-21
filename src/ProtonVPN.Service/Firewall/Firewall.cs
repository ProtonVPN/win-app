/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Common.Logging;
using ProtonVPN.NetworkFilter;
using ProtonVPN.Service.Driver;
using Action = ProtonVPN.NetworkFilter.Action;

namespace ProtonVPN.Service.Firewall
{
    internal class Firewall : IFirewall
    {
        private readonly ILogger _logger;
        private readonly IDriver _calloutDriver;
        private readonly Common.Configuration.Config _config;
        private readonly IpLayer _ipLayer;
        private readonly IpFilter _ipFilter;
        private FirewallParams _lastParams = FirewallParams.Null;

        private readonly List<ValueTuple<string, List<Guid>>> _serverAddressFilters = new();
        private readonly List<FirewallItem> _firewallItems = new();

        private const int DnsUdpPort = 53;
        private const int DhcpUdpPort = 67;

        public Firewall(
            ILogger logger,
            IDriver calloutDriver,
            Common.Configuration.Config config,
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

        public void EnableLeakProtection(FirewallParams firewallParams)
        {
            if (LeakProtectionEnabled)
            {
                ApplyChange(firewallParams);
                return;
            }

            if (_ipFilter.Instance == null)
            {
                _ipFilter.StartSession(firewallParams.SessionType);
            }

            if (_ipFilter.Instance.Session.Type == SessionType.Dynamic && firewallParams.SessionType == SessionType.Permanent)
            {
                _ipFilter.StartSession(SessionType.Permanent);
                _ipFilter.ClosePreviousSession();
            }
            else if (_ipFilter.Instance.Session.Type == SessionType.Permanent && firewallParams.SessionType == SessionType.Dynamic)
            {
                _ipFilter.StartSession(SessionType.Dynamic);
                _ipFilter.ClosePreviousSession();
            }

            _calloutDriver.Start();
            PermitOpenVpnServerAddress(firewallParams);
            ApplyFilters(firewallParams);
            _lastParams = firewallParams;
        }

        private void ApplyFilters(FirewallParams firewallParams)
        {
            try
            {
                _logger.Info("Firewall: Blocking internet");

                EnableDnsLeakProtection(firewallParams);

                if (!firewallParams.DnsLeakOnly)
                {
                    EnableBaseLeakProtection(firewallParams);
                }

                LeakProtectionEnabled = true;

                _logger.Info("Firewall: Internet blocked");
            }
            catch (NetworkFilterException ex)
            {
                _logger.Error(ex);
            }
        }

        public void DisableLeakProtection()
        {
            try
            {
                _logger.Info("Firewall: Restoring internet");

                _ipFilter.CloseCurrentSession();
                _ipFilter.DeletePermanentFilters();
                _serverAddressFilters.Clear();
                _firewallItems.Clear();
                LeakProtectionEnabled = false;
                _calloutDriver.Stop();
                _lastParams = FirewallParams.Null;

                _logger.Info("Firewall: Internet restored");
            }
            catch (NetworkFilterException ex)
            {
                _logger.Error(ex);
            }
        }

        private void ApplyChange(FirewallParams firewallParams)
        {
            if (firewallParams.SessionType != _lastParams.SessionType)
            {
                _ipFilter.StartSession(firewallParams.SessionType);
                _serverAddressFilters.Clear();
                ApplyFilters(firewallParams);
                _ipFilter.ClosePreviousSession();
            }

            if (firewallParams.InterfaceIndex != _lastParams.InterfaceIndex)
            {
                List<Guid> previousGuids = GetFirewallGuidsByType(FirewallItemType.PermitInterfaceFilter);
                PermitFromNetworkInterface(4, firewallParams);
                RemoveItems(previousGuids);

                previousGuids = GetFirewallGuidsByType(FirewallItemType.DnsCalloutFilter);
                CreateDnsCalloutFilter(firewallParams);
                RemoveItems(previousGuids);
            }

            if (firewallParams.DnsLeakOnly != _lastParams.DnsLeakOnly)
            {
                if (firewallParams.DnsLeakOnly)
                {
                    RemoveItems(GetFirewallGuidsByType(FirewallItemType.BaseProtectionFilter));
                }
                else
                {
                    EnableBaseLeakProtection(firewallParams);
                }
            }

            PermitOpenVpnServerAddress(firewallParams);
            _lastParams = firewallParams;
        }

        private void RemoveItems(List<Guid> guids)
        {
            DeleteIpFilters(guids);
            List<FirewallItem> firewallItems = _firewallItems.Where(item => guids.Contains(item.Guid)).ToList();

            foreach (FirewallItem item in firewallItems)
            {
                _firewallItems.Remove(item);
            }
        }

        private List<Guid> GetFirewallGuidsByType(FirewallItemType type)
        {
            return _firewallItems
                .Where(item => item.ItemType == type)
                .Select(item => item.Guid)
                .ToList();
        }

        private void EnableDnsLeakProtection(FirewallParams firewallParams)
        {
            CreateDnsCalloutFilter(firewallParams);
            BlockDns(3, firewallParams);
        }

        private void EnableBaseLeakProtection(FirewallParams firewallParams)
        {
            PermitDhcp(4, firewallParams);
            PermitFromNetworkInterface(4, firewallParams);
            PermitFromProcesses(4, firewallParams.Persistent);

            PermitIpv4Loopback(2, firewallParams.Persistent);
            PermitIpv6Loopback(2, firewallParams.Persistent);
            PermitPrivateNetwork(2, firewallParams.Persistent);

            BlockAllIpv4Network(1, firewallParams.Persistent);
            BlockAllIpv6Network(1, firewallParams.Persistent);
        }

        private void BlockDns(uint weight, FirewallParams firewallParams)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _ipFilter.Sublayer.CreateRemoteUdpPortFilter(new DisplayData(
                        "ProtonVPN DNS filter", "Permit UDP 53 port so we can block it at network layer"),
                    Action.HardPermit,
                    layer,
                    weight,
                    DnsUdpPort,
                    firewallParams.Persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.DnsFilter, guid));
            });

            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _ipFilter.Sublayer.CreateRemoteTcpPortFilter(new DisplayData(
                        "ProtonVPN block DNS", "Block TCP 53 port"),
                    Action.HardBlock,
                    layer,
                    weight,
                    DnsUdpPort,
                    firewallParams.Persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.DnsFilter, guid));
            });

            _ipLayer.ApplyToIpv6(layer =>
            {
                Guid guid = _ipFilter.Sublayer.CreateRemoteTcpPortFilter(new DisplayData(
                        "ProtonVPN block DNS", "Block TCP 53 port"),
                    Action.HardBlock,
                    layer,
                    weight,
                    DnsUdpPort,
                    firewallParams.Persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.DnsFilter, guid));
            });

            _ipLayer.ApplyToIpv6(layer =>
            {
                Guid guid = _ipFilter.Sublayer.CreateRemoteUdpPortFilter(new DisplayData(
                        "ProtonVPN block DNS", "Block UDP 53 port"),
                    Action.HardBlock,
                    layer,
                    weight,
                    DnsUdpPort,
                    firewallParams.Persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.DnsFilter, guid));
            });
        }

        private void CreateDnsCalloutFilter(FirewallParams firewallParams)
        {
            Guid guid = _ipFilter.Sublayer.BlockOutsideDns(new DisplayData("ProtonVPN block DNS", "Block outside dns"),
                Layer.OutboundIPPacketV4,
                3,
                IpFilter.DnsCalloutGuid,
                firewallParams.InterfaceIndex,
                firewallParams.Persistent);
            _firewallItems.Add(new FirewallItem(FirewallItemType.DnsCalloutFilter, guid));
        }

        private void PermitDhcp(uint weight, FirewallParams firewallParams)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _ipFilter.Sublayer.CreateRemoteUdpPortFilter(
                    new DisplayData("ProtonVPN permit DHCP", "Permit 67 UDP port"),
                    Action.SoftPermit,
                    layer,
                    weight,
                    DhcpUdpPort,
                    firewallParams.Persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
            });
        }

        private void PermitFromNetworkInterface(uint weight, FirewallParams firewallParams)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _ipFilter.Sublayer.CreateNetInterfaceFilter(
                    new DisplayData("ProtonVPN permit VPN tunnel", "Permit TAP adapter traffic"),
                    Action.SoftPermit,
                    layer,
                    firewallParams.InterfaceIndex,
                    weight,
                    firewallParams.Persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.PermitInterfaceFilter, guid));
            });

            _ipLayer.ApplyToIpv6(layer =>
            {
                Guid guid = _ipFilter.Sublayer.CreateNetInterfaceFilter(
                    new DisplayData("ProtonVPN permit VPN tunnel", "Permit TAP adapter traffic"),
                    Action.SoftPermit,
                    layer,
                    firewallParams.InterfaceIndex,
                    weight,
                    firewallParams.Persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.PermitInterfaceFilter, guid));
            });
        }

        private void PermitOpenVpnServerAddress(FirewallParams firewallParams)
        {
            if (string.IsNullOrEmpty(firewallParams.ServerIp))
            {
                return;
            }

            (string ip, List<Guid> guids) = _serverAddressFilters.FirstOrDefault();
            if (ip != null && ip == firewallParams.ServerIp)
            {
                _serverAddressFilters.RemoveAt(0);
                _serverAddressFilters.Add((ip, guids));
                return;
            }

            var filterGuids = new List<Guid>();

            _ipLayer.ApplyToIpv4(layer =>
            {
                filterGuids.Add(_ipFilter.Sublayer.CreateRemoteIPv4Filter(
                    new DisplayData("ProtonVPN permit OpenVPN server", "Permit server ip"),
                    Action.HardPermit,
                    layer,
                    1,
                    firewallParams.ServerIp,
                    firewallParams.Persistent));
            });

            _serverAddressFilters.Add((firewallParams.ServerIp, filterGuids));

            DeletePreviousFilters();
        }

        private void DeletePreviousFilters()
        {
            if (_serverAddressFilters.Count >= 3)
            {
                (string oldAddress, var guids) = _serverAddressFilters.FirstOrDefault();
                if (guids == null)
                {
                    return;
                }

                DeleteIpFilters(guids);
                _serverAddressFilters.RemoveAll(tuple => tuple.Item1 == oldAddress);
            }
        }

        private void DeleteIpFilters(List<Guid> guids)
        {
            foreach (Guid guid in guids)
            {
                _ipFilter.Sublayer.DestroyFilter(guid);
            }
        }

        private void BlockAllIpv4Network(uint weight, bool persistent)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _ipFilter.Sublayer.CreateLayerFilter(
                    new DisplayData("ProtonVPN block IPv4", "Block all IPv4 traffic"),
                    Action.SoftBlock,
                    layer,
                    weight,
                    persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
            });
        }

        private void BlockAllIpv6Network(uint weight, bool persistent)
        {
            _ipLayer.ApplyToIpv6(layer =>
            {
                Guid guid = _ipFilter.Sublayer.CreateLayerFilter(
                    new DisplayData("ProtonVPN block IPv6", "Block all IPv6 traffic"),
                    Action.SoftBlock,
                    layer,
                    weight,
                    persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
            });
        }

        private void PermitIpv4Loopback(uint weight, bool persistent)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _ipFilter.Sublayer.CreateLoopbackFilter(
                    new DisplayData("ProtonVPN permit IPv4 loopback", "Permit IPv4 loopback traffic"),
                    Action.HardPermit,
                    layer,
                    weight,
                    persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
            });
        }

        private void PermitIpv6Loopback(uint weight, bool persistent)
        {
            _ipLayer.ApplyToIpv6(layer =>
            {
                Guid guid = _ipFilter.Sublayer.CreateLoopbackFilter(
                    new DisplayData("ProtonVPN permit IPv6 loopback", "Permit IPv6 loopback traffic"),
                    Action.HardPermit,
                    layer,
                    weight,
                    persistent);
                _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
            });
        }

        private void PermitPrivateNetwork(uint weight, bool persistent)
        {
            List<NetworkAddress> networkAddresses = new List<NetworkAddress>
            {
                new NetworkAddress("10.0.0.0", "255.0.0.0"),
                new NetworkAddress("172.16.0.0", "255.240.0.0"),
                new NetworkAddress("192.168.0.0", "255.255.0.0"),
                new NetworkAddress("224.0.0.0", "240.0.0.0"),
                new NetworkAddress("255.255.255.255", "255.255.255.255")
            };

            foreach (NetworkAddress networkAddress in networkAddresses)
            {
                _ipLayer.ApplyToIpv4(layer =>
                {
                    Guid guid = _ipFilter.Sublayer.CreateRemoteNetworkIPv4Filter(
                        new DisplayData("ProtonVPN permit private network", ""),
                        Action.HardPermit,
                        layer,
                        weight,
                        networkAddress,
                        persistent);
                    _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
                });
            }
        }

        private void PermitFromProcesses(uint weight, bool persistent)
        {
            List<string> processes = new List<string>
            {
                _config.AppExePath, _config.ServiceExePath, _config.UpdateServiceExePath,
            };

            foreach (string processPath in processes)
            {
                _ipLayer.ApplyToIpv4(layer =>
                {
                    Guid guid = _ipFilter.Sublayer.CreateAppFilter(
                        new DisplayData("ProtonVPN permit app", "Permit ProtonVPN app to bypass VPN tunnel"),
                        Action.HardPermit,
                        layer,
                        weight,
                        processPath,
                        persistent);
                    _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
                });
            }
        }
    }
}