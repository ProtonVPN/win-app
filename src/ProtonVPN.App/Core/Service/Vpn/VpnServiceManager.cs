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
using System.Threading.Tasks;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Service.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Service.Contract.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnServiceManager : IVpnServiceManager
    {
        private readonly VpnService _vpnService;
        private readonly SettingsContractProvider _settingsContractProvider;
        private readonly ILogger _logger;

        public VpnServiceManager(
            VpnService vpnService,
            SettingsContractProvider settingsContractProvider,
            ILogger logger)
        {
            _logger = logger;
            _settingsContractProvider = settingsContractProvider;
            _vpnService = vpnService;
        }

        public async Task Connect(VpnConnectionRequest request)
        {
            Ensure.NotNull(request, nameof(request));

            VpnConnectionRequestContract contract = Map(request);

            await _vpnService.Connect(contract);
        }

        public async Task UpdateServers(IReadOnlyList<VpnHost> servers, VpnConfig config)
        {
            Ensure.NotNull(servers, nameof(servers));
            Ensure.NotNull(config, nameof(config));

            VpnHostContract[] endpointIpsContract = Map(servers);
            VpnConfigContract configContract = Map(config);

            await _vpnService.UpdateServers(endpointIpsContract, configContract);
        }

        public async Task<InOutBytes> Total()
        {
            return Map(await _vpnService.Total());
        }

        public async Task RepeatState()
        {
            await _vpnService.RepeatState();
        }

        public Task Disconnect(VpnError vpnError = VpnError.None)
        {
            _logger.Info("Disconnect requested");
            return _vpnService.Disconnect(_settingsContractProvider.GetSettingsContract(), Map(vpnError));
        }

        public void RegisterCallback(Action<VpnStateChangedEventArgs> callback)
            => _vpnService.VpnStateChanged += (s, e) => callback(Map(e));

        private VpnConnectionRequestContract Map(VpnConnectionRequest request)
        {
            return new()
            {
                Servers = Map(request.Servers),
                Protocol = Map(request.Protocol),
                VpnConfig = Map(request.Config),
                Credentials = Map(request.Credentials),
                Settings = _settingsContractProvider.GetSettingsContract()
            };
        }

        private static VpnHostContract[] Map(IEnumerable<VpnHost> servers)
        {
            return servers.Select(Map).ToArray();
        }

        private static VpnHostContract Map(VpnHost host)
        {
            return new VpnHostContract
            {
                Name = host.Name, 
                Ip = host.Ip,
                Label = host.Label,
            };
        }

        private static VpnProtocolContract Map(VpnProtocol protocol)
        {
            return (VpnProtocolContract) protocol;
        }

        private static VpnCredentialsContract Map(VpnCredentials credentials)
        {
            return new VpnCredentialsContract
            {
                Username = credentials.Username,
                Password = credentials.Password
            };
        }

        private static VpnConfigContract Map(VpnConfig config)
        {
            var portConfig = config.Ports.ToDictionary(p => Map(p.Key), p => p.Value.ToArray());
            return new VpnConfigContract
            {
                Ports = portConfig,
                CustomDns = config.CustomDns.ToList(),
                SplitTunnelMode = config.SplitTunnelMode,
                SplitTunnelIPs = config.SplitTunnelIPs.ToList()
            };
        }

        private static VpnStateChangedEventArgs Map(VpnStateContract contract)
        {
            VpnStatus status = Map(contract.Status);
            VpnError error = Map(contract.Error);
            VpnProtocol protocol = Map(contract.Protocol);

            return new VpnStateChangedEventArgs(status, error, contract.EndpointIp, contract.NetworkBlocked, protocol);
        }

        private static InOutBytes Map(InOutBytesContract bytes)
        {
            return new InOutBytes(bytes.BytesIn, bytes.BytesOut);
        }

        private static VpnProtocol Map(VpnProtocolContract protocol)
        {
            switch (protocol)
            {
                case VpnProtocolContract.Smart:
                    return VpnProtocol.Auto;
                case VpnProtocolContract.OpenVpnTcp:
                    return VpnProtocol.OpenVpnTcp;
                case VpnProtocolContract.OpenVpnUdp:
                    return VpnProtocol.OpenVpnUdp;
            }

            return VpnProtocol.Auto;
        }

        private static VpnStatus Map(VpnStatusContract status)
        {
            return (VpnStatus) status;
        }

        private static VpnErrorTypeContract Map(VpnError vpnError)
        {
            return (VpnErrorTypeContract)vpnError;
        }

        private static VpnError Map(VpnErrorTypeContract vpnError)
        {
            return (VpnError)vpnError;
        }
    }
}