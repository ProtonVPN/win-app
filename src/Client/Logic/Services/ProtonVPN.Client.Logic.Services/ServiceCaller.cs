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

using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Communication;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Services;

public class ServiceCaller : ServiceCallerBase, IServiceCaller
{
    private readonly ISettings _settings;
    private readonly IAuthKeyManager _authKeyManager;

    public ServiceCaller(ILogger logger, IAppGrpcClient grpcClient, ISettings settings, IAuthKeyManager authKeyManager, IServiceManager serviceManager)
        : base(logger, grpcClient, serviceManager)
    {
        _settings = settings;
        _authKeyManager = authKeyManager;
    }

    public Task RegisterClientAsync(int appServerPort, CancellationToken cancellationToken)
    {
        return InvokeAsync(c => c.RegisterStateConsumer(new StateConsumerIpcEntity { ServerPort = appServerPort }).Wrap());
    }

    public Task ConnectAsync(ConnectionRequestIpcEntity connectionRequest)
    {
        return InvokeAsync(c => c.Connect(connectionRequest).Wrap());
    }

#warning THIS METHOD SHOULD BE COMPLETED OR DELETED
    public Task ConnectAsync()
    {
        PublicKey publicKey = _authKeyManager.GetPublicKey();
        SecretKey secretKey = _authKeyManager.GetSecretKey();

        return InvokeAsync(c => c.Connect(new ConnectionRequestIpcEntity
        {
            Servers = new VpnServerIpcEntity[]
            {
                new VpnServerIpcEntity() // CH#5
                {
                    Name = "node-ch-02.protonvpn.net",
                    Ip = "185.159.157.6",
                    Label = "0",
                    X25519PublicKey = new ServerPublicKeyIpcEntity(new PublicKey("00WGV9C77fp+u1G2YrJ3VphcEKFCXcplgUU5THM+QgI=", KeyAlgorithm.X25519)),
                    Signature = "7zE5YnKNw5q9pE4BWaPaFzJTTj5NLeHkfhUxMfZynopZDMJCcrubIZhd0F1bWx+q5nIUNqEss+3ORHlzZwwcCg=="
                }
            },
            Protocol = VpnProtocolIpcEntity.Smart,
            Config = new VpnConfigIpcEntity
            {
                Ports = new Dictionary<VpnProtocolIpcEntity, int[]>
                {
                    { VpnProtocolIpcEntity.WireGuard, new int[] { 443, 88, 1224, 51820, 500, 4500 } },
                    { VpnProtocolIpcEntity.OpenVpnTcp, new int[] { 443, 1194, 4569, 5060, 80 } },
                    { VpnProtocolIpcEntity.OpenVpnUdp, new int[] { 443, 3389, 8080, 8443 } },
                },
                SplitTunnelMode = SplitTunnelModeIpcEntity.Disabled,
                NetShieldMode = 0,
                VpnProtocol = VpnProtocolIpcEntity.Smart,
                PreferredProtocols = new List<VpnProtocolIpcEntity> { VpnProtocolIpcEntity.Smart, VpnProtocolIpcEntity.WireGuard },
                ModerateNat = false,
                SplitTcp = true,
                AllowNonStandardPorts = true,
                PortForwarding = false,
            },
            Credentials = new VpnCredentialsIpcEntity
            {
                ClientCertPem = _settings.AuthenticationCertificatePem,
                ClientKeyPair = new AsymmetricKeyPairIpcEntity
                {
                    PublicKey = new PublicKeyIpcEntity
                    {
                        Algorithm = KeyAlgorithmIpcEntity.Ed25519, //TODO: use mapper to IpcEntity
                        Base64 = publicKey.Base64,
                        Pem = publicKey.Pem
                    },
                    SecretKey = new SecretKeyIpcEntity
                    {
                        Algorithm = KeyAlgorithmIpcEntity.Ed25519,  //TODO: use mapper to IpcEntity
                        Base64 = secretKey.Base64,
                        Pem = secretKey.Pem
                    }
                }
            },
            Settings = new MainSettingsIpcEntity
            {
                KillSwitchMode = KillSwitchModeIpcEntity.Off,
                SplitTunnel = new SplitTunnelSettingsIpcEntity { Mode = SplitTunnelModeIpcEntity.Disabled },
                NetShieldMode = 0,
                ModerateNat = false,
                SplitTcp = true,
                AllowNonStandardPorts = true,
                Ipv6LeakProtection = true,
                VpnProtocol = VpnProtocolIpcEntity.Smart,
                OpenVpnAdapter = OpenVpnAdapterIpcEntity.Tun,
                PortForwarding = false
            }
        }).Wrap());
    }

#warning THIS METHOD SHOULD BE COMPLETED OR DELETED
    public Task DisconnectAsync()
    {
        return InvokeAsync(c => c.Disconnect(new DisconnectionRequestIpcEntity
        {
            ErrorType = VpnErrorTypeIpcEntity.None,
            Settings = new MainSettingsIpcEntity
            {
                KillSwitchMode = KillSwitchModeIpcEntity.Off,
                SplitTunnel = new SplitTunnelSettingsIpcEntity { Mode = SplitTunnelModeIpcEntity.Disabled },
                NetShieldMode = 0,
                ModerateNat = false,
                SplitTcp = true,
                AllowNonStandardPorts = true,
                Ipv6LeakProtection = true,
                VpnProtocol = VpnProtocolIpcEntity.Smart,
                OpenVpnAdapter = OpenVpnAdapterIpcEntity.Tun,
                PortForwarding = false
            }
        }).Wrap());
    }
}