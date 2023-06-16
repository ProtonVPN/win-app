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

using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Crypto;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Communication;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Services
{
    public class ServiceCaller : ServiceCallerBase, IServiceCaller
    {
        public ServiceCaller(ILogger logger, IAppGrpcClient grpcClient)
            : base(logger, grpcClient)
        {
        }

        public Task RegisterClientAsync(int appServerPort, CancellationToken cancellationToken)
        {
            return InvokeAsync(c => c.RegisterStateConsumer(new StateConsumerIpcEntity { ServerPort = appServerPort }).Wrap());
        }

#warning THIS METHOD SHOULD BE COMPLETED OR DELETED
        public Task ConnectAsync()
        {
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
                Config = new VpnConfigIpcEntity()
                {
                    Ports = new Dictionary<VpnProtocolIpcEntity, int[]>() { { VpnProtocolIpcEntity.WireGuard, new int[] { 443, 88, 1224, 51820, 500, 4500 } } },
                    SplitTunnelMode = SplitTunnelModeIpcEntity.Disabled,
                    NetShieldMode = 0,
                    VpnProtocol = VpnProtocolIpcEntity.Smart,
                    PreferredProtocols = new List<VpnProtocolIpcEntity>() { VpnProtocolIpcEntity.Smart, VpnProtocolIpcEntity.WireGuard },
                    ModerateNat = false,
                    SplitTcp = true,
                    AllowNonStandardPorts = true,
                    PortForwarding = false
                },
                Credentials = new VpnCredentialsIpcEntity()
                {
                    ClientCertPem = "-----BEGIN CERTIFICATE-----\nMIIB9zCCAamgAwIBAgIEfOpnzzAFBgMrZXAwMTEvMC0GA1UEAwwmUHJvdG9uVlBO\nIENsaWVudCBDZXJ0aWZpY2F0ZSBBdXRob3JpdHkwHhcNMjMwNjE1MTIyODQxWhcN\nMjMwNjE2MTIyODQyWjAVMRMwEQYDVQQDDAoyMDk1NzM2NzgzMCowBQYDK2VwAyEA\nZIu6plU9z17AV4XwTysPDbtsKPvcBL1EYQel5nr0r5Ojgf4wgfswHQYDVR0OBBYE\nFN+X5VKFE0W7TY7ogkVyFWrX/AgdMBMGDCsGAQQBg7tpAQAAAAQDAgEBMBwGDCsG\nAQQBg7tpAQAAAgQMMAoECHZwbi1wYWlkMBkGDCsGAQQBg7tpAQAAAwQJBAdXaW5k\nb3dzMA4GA1UdDwEB/wQEAwIHgDAMBgNVHRMBAf8EAjAAMBMGA1UdJQQMMAoGCCsG\nAQUFBwMCMFkGA1UdIwRSMFCAFLPhzBCWovgipcvcz0QGRizrks4FoTWkMzAxMS8w\nLQYDVQQDDCZQcm90b25WUE4gQ2xpZW50IENlcnRpZmljYXRlIEF1dGhvcml0eYIB\nATAFBgMrZXADQQDvHhNtufyElb/cNXD3DIluy3DxGYxIg5J8JVK0t4MzOgR61Nfw\n0o0aureldkXSvi4wyhxdXMFKmBy4QDALTWoI\n-----END CERTIFICATE-----\n",
                    ClientKeyPair = new AsymmetricKeyPairIpcEntity()
                    {
                        PublicKey = new PublicKeyIpcEntity()
                        {
                            Algorithm = KeyAlgorithmIpcEntity.Ed25519,
                            Base64 = "MCowBQYDK2VwAyEAZIu6plU9z17AV4XwTysPDbtsKPvcBL1EYQel5nr0r5M=",
                            Pem = "-----BEGIN PUBLIC KEY-----\r\nMCowBQYDK2VwAyEAZIu6plU9z17AV4XwTysPDbtsKPvcBL1EYQel5nr0r5M=\r\n-----END PUBLIC KEY-----"
                        },
                        SecretKey = new SecretKeyIpcEntity()
                        {
                            Algorithm = KeyAlgorithmIpcEntity.Ed25519,
                            Base64 = "MC4CAQAwBQYDK2VwBCIEIGTjg75N6QHsnx5fNv/420E2TERSf0gaaQdyPzuMsTdx",
                            Pem = "-----BEGIN PRIVATE KEY-----\r\nMC4CAQAwBQYDK2VwBCIEIGTjg75N6QHsnx5fNv/420E2TERSf0gaaQdyPzuMsTdx\r\n-----END PRIVATE KEY-----"
                        }
                    }
                },
                Settings = new MainSettingsIpcEntity()
                {
                    KillSwitchMode = KillSwitchModeIpcEntity.Off,
                    SplitTunnel = new SplitTunnelSettingsIpcEntity() { Mode = SplitTunnelModeIpcEntity.Disabled },
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
                Settings = new MainSettingsIpcEntity()
                {
                    KillSwitchMode = KillSwitchModeIpcEntity.Off,
                    SplitTunnel = new SplitTunnelSettingsIpcEntity() { Mode = SplitTunnelModeIpcEntity.Disabled },
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
}