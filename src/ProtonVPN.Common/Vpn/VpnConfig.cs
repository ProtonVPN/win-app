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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Networking;

namespace ProtonVPN.Common.Vpn
{
    public class VpnConfig
    {
        public IReadOnlyDictionary<VpnProtocol, IReadOnlyCollection<int>> Ports { get; }
        public IReadOnlyCollection<string> CustomDns { get; }
        public SplitTunnelMode SplitTunnelMode { get; }
        public IReadOnlyCollection<string> SplitTunnelIPs { get; }
        public OpenVpnAdapter OpenVpnAdapter { get; set; }
        public VpnProtocol VpnProtocol { get; }
        public IList<VpnProtocol> PreferredProtocols { get; }
        public int NetShieldMode { get; }
        public bool SplitTcp { get; }
        public bool PortForwarding { get; }

        public bool ModerateNat { get; }

        public bool? AllowNonStandardPorts { get; }

        public VpnConfig(VpnConfigParameters parameters)
        {
            AssertPortsValid(parameters.Ports);
            AssertCustomDnsValid(parameters.CustomDns);

            Ports = parameters.Ports;
            CustomDns = parameters.CustomDns ?? new List<string>();
            SplitTunnelMode = parameters.SplitTunnelMode;
            SplitTunnelIPs = parameters.SplitTunnelIPs ?? new List<string>();
            OpenVpnAdapter = parameters.OpenVpnAdapter;
            VpnProtocol = parameters.VpnProtocol;
            PreferredProtocols = parameters.PreferredProtocols;
            NetShieldMode = parameters.NetShieldMode;
            SplitTcp = parameters.SplitTcp;
            ModerateNat = parameters.ModerateNat;
            AllowNonStandardPorts = parameters.AllowNonStandardPorts;
            PortForwarding = parameters.PortForwarding;
        }

        private void AssertPortsValid(IReadOnlyDictionary<VpnProtocol, IReadOnlyCollection<int>> ports)
        {
            foreach (KeyValuePair<VpnProtocol, IReadOnlyCollection<int>> item in ports)
            {
                foreach (int port in item.Value)
                {
                    if (port < 1 || port > 65535)
                    {
                        throw new ArgumentException($"Invalid OpenVPN port: {port}");
                    }
                }
            }
        }

        private void AssertCustomDnsValid(IReadOnlyCollection<string> customDns)
        {
            if (customDns == null)
            {
                return;
            }

            foreach (string dns in customDns)
            {
                if (!dns.IsValidIpAddress())
                {
                    throw new ArgumentException($"Invalid DNS address: {dns}");
                }
            }
        }
    }
}