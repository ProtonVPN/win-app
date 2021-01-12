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
        private readonly Sublayer _sublayer;
        private FirewallParams _lastParams = FirewallParams.Null;
        private Callout _dnsCallout;

        private readonly List<ValueTuple<string, List<Guid>>> _serverAddressFilters =
            new List<ValueTuple<string, List<Guid>>>();

        private readonly Guid _networkLayerCalloutGuid = Guid.Parse("{10636af3-50d6-4f53-acb7-d5af33217fcb}");
        private readonly List<FirewallItem> _firewallItems = new List<FirewallItem>();

        public Firewall(
            ILogger logger,
            IDriver calloutDriver,
            Common.Configuration.Config config,
            IpLayer ipLayer,
            Sublayer sublayer)
        {
            _logger = logger;
            _config = config;
            _ipLayer = ipLayer;
            _sublayer = sublayer;
            _calloutDriver = calloutDriver;
        }

        public bool LeakProtectionEnabled { get; private set; }

        public void EnableLeakProtection(FirewallParams firewallParams)
        {
            PermitOpenVpnServerAddress(firewallParams.ServerIp);

            if (LeakProtectionEnabled)
            {
                ApplyChange(firewallParams);
                return;
            }

            _calloutDriver.Start();

            try
            {
                _logger.Info("Firewall: Blocking internet");

                EnableDnsLeakProtection(firewallParams.InterfaceIndex);

                if (!firewallParams.DnsLeakOnly)
                {
                    EnableBaseLeakProtection(firewallParams.InterfaceIndex);
                }

                LeakProtectionEnabled = true;

                _logger.Info("Firewall: Internet blocked");
            }
            catch (NetworkFilterException ex)
            {
                _logger.Error(ex);
            }

            _lastParams = firewallParams;
        }

        public void DisableLeakProtection()
        {
            if (!LeakProtectionEnabled)
                return;

            try
            {
                _logger.Info("Firewall: Restoring internet");

                _sublayer.DestroyAllFilters();
                _sublayer.DestroyAllCallouts();
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
            if (firewallParams.InterfaceIndex != _lastParams.InterfaceIndex)
            {
                List<Guid> previousGuids = GetFirewallGuidsByType(FirewallItemType.PermitInterfaceFilter);
                PermitFromNetworkInterface(firewallParams.InterfaceIndex);
                RemoveItems(previousGuids);

                previousGuids = GetFirewallGuidsByType(FirewallItemType.DnsCalloutFilter);
                CreateDnsCalloutFilter(firewallParams.InterfaceIndex);
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
                    EnableBaseLeakProtection(firewallParams.InterfaceIndex);
                }
            }

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

        private void EnableDnsLeakProtection(uint interfaceIndex)
        {
            CreateDnsCallout();
            CreateDnsCalloutFilter(interfaceIndex);
            BlockDns(3);
        }

        private void EnableBaseLeakProtection(uint tapInterfaceIndex)
        {
            PermitDhcp(4);
            PermitFromNetworkInterface(tapInterfaceIndex);
            PermitFromProcesses(4);

            PermitIpv4Loopback(2);
            PermitIpv6Loopback(2);
            PermitPrivateNetwork(2);

            BlockAllIpv4Network(1);
            BlockAllIpv6Network(1);
        }

        private void BlockDns(uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _sublayer.CreateRemoteUdpPortFilter(new DisplayData(
                        "ProtonVPN DNS filter", "Permit UDP 53 port so we can block it at network layer"),
                    Action.HardPermit,
                    layer,
                    weight,
                    53);
                _firewallItems.Add(new FirewallItem(FirewallItemType.DnsFilter, guid));
            });

            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _sublayer.CreateRemoteTcpPortFilter(new DisplayData(
                        "ProtonVPN block DNS", "Block TCP 53 port"),
                    Action.HardBlock,
                    layer,
                    weight,
                    53);
                _firewallItems.Add(new FirewallItem(FirewallItemType.DnsFilter, guid));
            });

            _ipLayer.ApplyToIpv6(layer =>
            {
                Guid guid = _sublayer.CreateRemoteTcpPortFilter(new DisplayData(
                        "ProtonVPN block DNS", "Block TCP 53 port"),
                    Action.HardBlock,
                    layer,
                    weight,
                    53);
                _firewallItems.Add(new FirewallItem(FirewallItemType.DnsFilter, guid));
            });

            _ipLayer.ApplyToIpv6(layer =>
            {
                Guid guid = _sublayer.CreateRemoteUdpPortFilter(new DisplayData(
                        "ProtonVPN block DNS",
                        "Block UDP 53 port"),
                    Action.HardBlock,
                    layer,
                    weight,
                    53);
                _firewallItems.Add(new FirewallItem(FirewallItemType.DnsFilter, guid));
            });
        }

        private void CreateDnsCallout()
        {
            _dnsCallout = _sublayer.CreateCallout(
                new DisplayData
                {
                    Name = "ProtonVPN block dns callout",
                    Description = "Sends server failure packet response for non TAP DNS queries.",
                },
                _networkLayerCalloutGuid,
                Layer.OutboundIPPacketV4);
            _firewallItems.Add(new FirewallItem(FirewallItemType.DnsCallout, _dnsCallout.Id));
        }

        private void CreateDnsCalloutFilter(uint interfaceIndex)
        {
            Guid guid = _sublayer.BlockOutsideDns(new DisplayData("ProtonVPN block DNS", "Block outside dns"),
                Layer.OutboundIPPacketV4,
                3,
                _dnsCallout,
                interfaceIndex);
            _firewallItems.Add(new FirewallItem(FirewallItemType.DnsCalloutFilter, guid));
        }

        private void PermitDhcp(uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _sublayer.CreateRemoteUdpPortFilter(
                    new DisplayData("ProtonVPN permit DHCP", "Permit 67 UDP port"),
                    Action.SoftPermit,
                    layer,
                    weight,
                    67);
                _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
            });
        }

        private void PermitFromNetworkInterface(uint tapInterfaceIndex)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _sublayer.CreateNetInterfaceFilter(
                    new DisplayData("ProtonVPN permit VPN tunnel", "Permit TAP adapter traffic"),
                    Action.SoftPermit,
                    layer,
                    4,
                    tapInterfaceIndex);
                _firewallItems.Add(new FirewallItem(FirewallItemType.PermitInterfaceFilter, guid));
            });

            _ipLayer.ApplyToIpv6(layer =>
            {
                Guid guid = _sublayer.CreateNetInterfaceFilter(
                    new DisplayData("ProtonVPN permit VPN tunnel", "Permit TAP adapter traffic"),
                    Action.SoftPermit,
                    layer,
                    4,
                    tapInterfaceIndex);
                _firewallItems.Add(new FirewallItem(FirewallItemType.PermitInterfaceFilter, guid));
            });
        }

        private void PermitOpenVpnServerAddress(string address)
        {
            (string ip, var guids) = _serverAddressFilters.FirstOrDefault();
            if (ip != null && ip == address)
            {
                _serverAddressFilters.RemoveAt(0);
                _serverAddressFilters.Add((ip, guids));
                return;
            }

            var filterGuids = new List<Guid>();

            _ipLayer.ApplyToIpv4(layer =>
            {
                filterGuids.Add(_sublayer.CreateRemoteIPv4Filter(
                    new DisplayData("ProtonVPN permit OpenVPN server", "Permit server ip"),
                    Action.HardPermit,
                    layer,
                    1,
                    address));
            });

            _serverAddressFilters.Add((address, filterGuids));

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
                _sublayer.DestroyFilter(guid);
            }
        }

        private void BlockAllIpv4Network(uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _sublayer.CreateLayerFilter(
                    new DisplayData("ProtonVPN block IPv4", "Block all IPv4 traffic"),
                    Action.SoftBlock,
                    layer,
                    weight);
                _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
            });
        }

        private void BlockAllIpv6Network(uint weight)
        {
            _ipLayer.ApplyToIpv6(layer =>
            {
                Guid guid = _sublayer.CreateLayerFilter(
                    new DisplayData("ProtonVPN block IPv6", "Block all IPv6 traffic"),
                    Action.SoftBlock,
                    layer,
                    weight);
                _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
            });
        }

        private void PermitIpv4Loopback(uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _sublayer.CreateLoopbackFilter(
                    new DisplayData("ProtonVPN permit IPv4 loopback", "Permit IPv4 loopback traffic"),
                    Action.HardPermit,
                    layer,
                    weight);
                _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
            });
        }

        private void PermitIpv6Loopback(uint weight)
        {
            _ipLayer.ApplyToIpv6(layer =>
            {
                Guid guid = _sublayer.CreateLoopbackFilter(
                    new DisplayData("ProtonVPN permit IPv6 loopback", "Permit IPv6 loopback traffic"),
                    Action.HardPermit,
                    layer,
                    weight);
                _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
            });
        }

        private void PermitPrivateNetwork(uint weight)
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
                    Guid guid = _sublayer.CreateRemoteNetworkIPv4Filter(
                        new DisplayData("ProtonVPN permit private network", ""),
                        Action.HardPermit,
                        layer,
                        weight,
                        networkAddress);
                    _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
                });
            }
        }

        private void PermitFromProcesses(uint weight)
        {
            List<string> processes = new List<string>
            {
                _config.AppExePath, _config.ServiceExePath, _config.UpdateServiceExePath,
            };

            foreach (string processPath in processes)
            {
                _ipLayer.ApplyToIpv4(layer =>
                {
                    Guid guid = _sublayer.CreateAppFilter(
                        new DisplayData("ProtonVPN permit app", "Permit ProtonVPN app to bypass VPN tunnel"),
                        Action.HardPermit,
                        layer,
                        weight,
                        processPath);
                    _firewallItems.Add(new FirewallItem(FirewallItemType.BaseProtectionFilter, guid));
                });
            }
        }
    }
}