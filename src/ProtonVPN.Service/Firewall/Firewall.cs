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
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.NetworkFilter;
using Action = ProtonVPN.NetworkFilter.Action;

namespace ProtonVPN.Service.Firewall
{
    internal class Firewall : IFirewall
    {
        private readonly ILogger _logger;
        private readonly Common.Configuration.Config _config;
        private readonly INetworkInterfaces _networkInterfaces;
        private readonly IpLayer _ipLayer;
        private readonly Sublayer _sublayer;

        private readonly List<ValueTuple<string, List<Guid>>> _serverAddressFilters = new List<ValueTuple<string, List<Guid>>>();

        public Firewall(
            ILogger logger,
            Common.Configuration.Config config,
            INetworkInterfaces networkInterfaces,
            IpLayer ipLayer,
            Sublayer sublayer)
        {
            _logger = logger;
            _config = config;
            _networkInterfaces = networkInterfaces;
            _ipLayer = ipLayer;
            _sublayer = sublayer;
        }

        public bool LeakProtectionEnabled { get; private set; }

        public void EnableLeakProtection(string serverIp)
        {
            PermitOpenVpnServerAddress(serverIp);

            if (LeakProtectionEnabled)
                return;

            try
            {
                _logger.Info("Firewall: Blocking internet");

                var tapInterface = _networkInterfaces.Interface(_config.OpenVpn.TapAdapterDescription);

                PermitDhcp(4);
                PermitFromNetworkInterface(tapInterface.Id, 4);
                PermitFromApp(4);
                PermitFromService(4);
                PermitFromUpdateService(4);

                BlockDns(3);

                PermitIpv4Loopback(2);
                PermitIpv6Loopback(2);
                PermitPrivateNetwork(2);

                BlockAllIpv4Network(1);
                BlockAllIpv6Network(1);

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
            if (!LeakProtectionEnabled)
                return;

            try
            {
                _logger.Info("Firewall: Restoring internet");

                _sublayer.DestroyAllFilters();
                _serverAddressFilters.Clear();
                LeakProtectionEnabled = false;

                _logger.Info("Firewall: Internet restored");
            }
            catch (NetworkFilterException ex)
            {
                _logger.Error(ex);
            }
        }

        private void BlockDns(uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                _sublayer.CreateRemoteTcpPortFilter(new DisplayData(
                        "ProtonVPN block DNS", "Block TCP 53 port"),
                    Action.HardBlock,
                    layer,
                    weight,
                    53);
            });

            _ipLayer.ApplyToIpv4(layer =>
            {
                _sublayer.CreateRemoteUdpPortFilter(new DisplayData(
                        "ProtonVPN block DNS",
                        "Block UDP 53 port"),
                    Action.HardBlock,
                    layer,
                    weight,
                    53);
            });
        }

        private void PermitDhcp(uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                _sublayer.CreateRemoteUdpPortFilter(
                    new DisplayData("ProtonVPN permit DHCP", "Permit 67 UDP port"),
                    Action.SoftPermit,
                    layer,
                    weight,
                    67);
            });
        }

        private void PermitFromNetworkInterface(string id, uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                _sublayer.CreateNetInterfaceFilter(
                    new DisplayData("ProtonVPN permit VPN tunnel", "Permit TAP adapter traffic"),
                    Action.SoftPermit,
                    layer,
                    weight,
                    id);
            });

            _ipLayer.ApplyToIpv6(layer =>
            {
                _sublayer.CreateNetInterfaceFilter(
                    new DisplayData("ProtonVPN permit VPN tunnel", "Permit TAP adapter traffic"),
                    Action.SoftPermit,
                    layer,
                    weight,
                    id);
            });
        }

        private void PermitOpenVpnServerAddress(string address)
        {
            var (ip, guids) = _serverAddressFilters.FirstOrDefault();
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
                var (oldAddress, guids) = _serverAddressFilters.FirstOrDefault();
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
            foreach (var guid in guids)
            {
                _sublayer.DestroyFilter(guid);
            }
        }

        private void BlockAllIpv4Network(uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                _sublayer.CreateLayerFilter(
                    new DisplayData("ProtonVPN block IPv4", "Block all IPv4 traffic"),
                    Action.SoftBlock,
                    layer,
                    weight);
            });
        }

        private void BlockAllIpv6Network(uint weight)
        {
            _ipLayer.ApplyToIpv6(layer =>
            {
                _sublayer.CreateLayerFilter(
                    new DisplayData("ProtonVPN block IPv6", "Block all IPv6 traffic"),
                    Action.SoftBlock,
                    layer,
                    weight);
            });
        }

        private void PermitIpv4Loopback(uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                _sublayer.CreateLoopbackFilter(
                    new DisplayData("ProtonVPN permit IPv4 loopback", "Permit IPv4 loopback traffic"),
                    Action.HardPermit,
                    layer,
                    weight);
            });
        }

        private void PermitIpv6Loopback(uint weight)
        {
            _ipLayer.ApplyToIpv6(layer =>
            {
                _sublayer.CreateLoopbackFilter(
                    new DisplayData("ProtonVPN permit IPv6 loopback", "Permit IPv6 loopback traffic"),
                    Action.HardPermit,
                    layer,
                    weight);
            });
        }

        private void PermitPrivateNetwork(uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                _sublayer.CreateRemoteNetworkIPv4Filter(new DisplayData(
                        "ProtonVPN permit private network",
                        ""),
                    Action.HardPermit,
                    layer,
                    weight,
                    new NetworkAddress("10.0.0.0", "255.0.0.0"));
            });

            _ipLayer.ApplyToIpv4(layer =>
            {
                _sublayer.CreateRemoteNetworkIPv4Filter(new DisplayData(
                        "ProtonVPN permit private network",
                        ""),
                    Action.HardPermit,
                    layer,
                    weight,
                    new NetworkAddress("172.16.0.0", "255.240.0.0"));
            });

            _ipLayer.ApplyToIpv4(layer =>
            {
                _sublayer.CreateRemoteNetworkIPv4Filter(
                    new DisplayData(
                        "ProtonVPN permit private network",
                        ""),
                    Action.HardPermit,
                    layer,
                    weight,
                    new NetworkAddress("192.168.0.0", "255.255.0.0"));
            });
        }

        private void PermitFromApp(uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                _sublayer.CreateAppFilter(
                    new DisplayData("ProtonVPN permit app", "Permit ProtonVPN app to bypass VPN tunnel"),
                    Action.HardPermit,
                    layer,
                    weight,
                    _config.AppExePath);
            });
        }

        private void PermitFromService(uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                _sublayer.CreateAppFilter(
                    new DisplayData("ProtonVPN permit service", "Permit ProtonVPN Service to bypass VPN tunnel"),
                    Action.HardPermit,
                    layer,
                    weight,
                    _config.ServiceExePath);
            });
        }

        private void PermitFromUpdateService(uint weight)
        {
            _ipLayer.ApplyToIpv4(layer =>
            {
                _sublayer.CreateAppFilter(
                    new DisplayData("ProtonVPN permit update service", "Permit ProtonVPN Update Service to bypass VPN tunnel"),
                    Action.HardPermit,
                    layer,
                    weight,
                    _config.UpdateServiceExePath);
            });
        }
    }
}
