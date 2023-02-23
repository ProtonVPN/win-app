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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.DisconnectLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.PortForwarding;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Service.Settings;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Service.Contract.Crypto;
using ProtonVPN.Service.Contract.PortForwarding;
using ProtonVPN.Service.Contract.Settings;
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

        public async Task UpdateAuthCertificate(string certificate)
        {
            await _vpnService.UpdateAuthCertificate(certificate);
        }

        public async Task<InOutBytes> Total()
        {
            return Map(await _vpnService.Total());
        }

        public async Task RepeatState()
        {
            await _vpnService.RepeatState();
        }

        public Task Disconnect(VpnError vpnError = VpnError.None,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _logger.Info<DisconnectTriggerLog>($"Disconnect requested (Error: {vpnError})",
                sourceFilePath: sourceFilePath, sourceMemberName: sourceMemberName, sourceLineNumber: sourceLineNumber);
            return _vpnService.Disconnect(_settingsContractProvider.GetSettingsContract(), Map(vpnError));
        }

        public void RegisterVpnStateCallback(Action<VpnStateChangedEventArgs> callback)
            => _vpnService.VpnStateChanged += (s, e) => callback(Map(e));

        public void RegisterServiceSettingsStateCallback(Action<ServiceSettingsStateChangedEventArgs> callback)
            => _vpnService.ServiceSettingsStateChanged += (s, e) => callback(Map(e));

        public void RegisterPortForwardingStateCallback(Action<PortForwardingState> callback)
            => _vpnService.PortForwardingStateChanged += (s, e) => callback(Map(e));

        public void RegisterConnectionDetailsChangeCallback(Action<ConnectionDetails> callback)
            => _vpnService.ConnectionDetailsChanged += (s, e) => callback(Map(e));

        private static ConnectionDetails Map(ConnectionDetailsContract contract)
        {
            return new ConnectionDetails
            {
                ClientIpAddress = contract.ClientIpAddress,
                ClientCountryIsoCode = contract.ClientCountryIsoCode,
                ServerIpAddress = contract.ServerIpAddress,
            };
        }

        private static PortForwardingState Map(PortForwardingStateContract contract)
        {
            return new()
            {
                MappedPort = CreateTemporaryMappedPort(contract.MappedPort),
                Status = (PortMappingStatus)contract.Status,
                TimestampUtc = contract.TimestampUtc
            };
        }

        private static TemporaryMappedPort CreateTemporaryMappedPort(TemporaryMappedPortContract contract)
        {
            if (contract is null)
            {
                return null;
            }
            return new()
            {
                MappedPort = new(internalPort: contract.InternalPort, externalPort: contract.ExternalPort),
                Lifetime = contract.Lifetime,
                ExpirationDateUtc = contract.ExpirationDateUtc
            };
        }

        private static ServiceSettingsStateChangedEventArgs Map(ServiceSettingsStateContract contract)
        {
            return new(contract.IsNetworkBlocked, Map(contract.CurrentState));
        }

        private VpnConnectionRequestContract Map(VpnConnectionRequest request)
        {
            return new()
            {
                Servers = Map(request.Servers),
                Protocol = Map(request.VpnProtocol),
                VpnConfig = Map(request.Config),
                Credentials = Map(request.Credentials),
                Settings = _settingsContractProvider.GetSettingsContract(request.Config.OpenVpnAdapter)
            };
        }

        private static VpnHostContract[] Map(IEnumerable<VpnHost> servers)
        {
            return servers.Select(Map).ToArray();
        }

        private static VpnHostContract Map(VpnHost host)
        {
            return new()
            {
                Name = host.Name,
                Ip = host.Ip,
                Label = host.Label,
                X25519PublicKey = host.X25519PublicKey != null ? new ServerPublicKeyContract(host.X25519PublicKey) : null,
                Signature = host.Signature,
            };
        }

        private static VpnCredentialsContract Map(VpnCredentials credentials)
        {
            return new()
            {
                Username = credentials.Username,
                Password = credentials.Password,
                ClientCertPem = credentials.ClientCertPem,
                ClientKeyPair = credentials.ClientKeyPair == null ? null : new AsymmetricKeyPairContract(credentials.ClientKeyPair)
            };
        }

        private static VpnConfigContract Map(VpnConfig config)
        {
            Dictionary<VpnProtocolContract, int[]> portConfig =
                config.Ports.ToDictionary(p => Map(p.Key), p => p.Value.ToArray());
            return new VpnConfigContract
            {
                Ports = portConfig,
                CustomDns = config.CustomDns.ToList(),
                AllowNonStandardPorts = config.AllowNonStandardPorts,
                SplitTunnelMode = config.SplitTunnelMode,
                SplitTunnelIPs = config.SplitTunnelIPs.ToList(),
                NetShieldMode = config.NetShieldMode,
                VpnProtocol = Map(config.VpnProtocol),
                ModerateNat = config.ModerateNat,
                PreferredProtocols = Map(config.PreferredProtocols),
                SplitTcp = config.SplitTcp,
                PortForwarding = config.PortForwarding,
            };
        }

        private static IList<VpnProtocolContract> Map(IList<VpnProtocol> protocols)
        {
            return protocols.Select(Map).ToList();
        }

        private static VpnStateChangedEventArgs Map(VpnStateContract contract)
        {
            VpnStatus status = Map(contract.Status);
            VpnError error = Map(contract.Error);
            VpnProtocol protocol = Map(contract.VpnProtocol);

            return new(status, error, contract.EndpointIp, contract.NetworkBlocked, protocol, contract.OpenVpnAdapterType, contract.Label);
        }

        private static InOutBytes Map(InOutBytesContract bytes)
        {
            return new(bytes.BytesIn, bytes.BytesOut);
        }

        private static VpnProtocolContract Map(VpnProtocol protocol)
        {
            return protocol switch
            {
                VpnProtocol.OpenVpnUdp => VpnProtocolContract.OpenVpnUdp,
                VpnProtocol.OpenVpnTcp => VpnProtocolContract.OpenVpnTcp,
                VpnProtocol.WireGuard => VpnProtocolContract.WireGuard,
                VpnProtocol.Smart => VpnProtocolContract.Smart,
                _ => throw new NotImplementedException("VpnProtocol has an unknown value.")
            };
        }


        private static VpnProtocol Map(VpnProtocolContract protocol)
        {
            return protocol switch
            {
                VpnProtocolContract.OpenVpnUdp => VpnProtocol.OpenVpnUdp,
                VpnProtocolContract.OpenVpnTcp => VpnProtocol.OpenVpnTcp,
                VpnProtocolContract.WireGuard => VpnProtocol.WireGuard,
                VpnProtocolContract.Smart => VpnProtocol.Smart,
                _ => throw new NotImplementedException("VpnProtocol has an unknown value.")
            };
        }

        private static VpnStatus Map(VpnStatusContract status)
        {
            return (VpnStatus)status;
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