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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Wrappers;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.Wrappers;

public class ConnectionRequestWrapper : ConnectionRequestWrapperBase, IConnectionRequestWrapper
{
    private readonly IAuthKeyManager _authKeyManager;

    public ConnectionRequestWrapper(
        ISettings settings,
        IEntityMapper entityMapper,
        IAuthKeyManager authKeyManager)
        : base(settings, entityMapper)
    {
        _authKeyManager = authKeyManager;
    }

    public ConnectionRequestIpcEntity Wrap(IConnectionIntent connectionIntent)
    {
        MainSettingsIpcEntity settings = GetSettings();

        ConnectionRequestIpcEntity request = new()
        {
            Config = GetVpnConfig(settings),
            Credentials = GetVpnCredentials(),
            Protocol = settings.VpnProtocol,
            Servers = GetVpnServers(connectionIntent),
            Settings = settings,
        };

        return request;
    }

    protected override VpnCredentialsIpcEntity GetVpnCredentials()
    {
        PublicKey publicKey = _authKeyManager.GetPublicKey();
        SecretKey secretKey = _authKeyManager.GetSecretKey();

        return new VpnCredentialsIpcEntity
        {
            ClientCertPem = Settings.AuthenticationCertificatePem,
            ClientKeyPair = new AsymmetricKeyPairIpcEntity
            {
                PublicKey = EntityMapper.Map<PublicKey, PublicKeyIpcEntity>(publicKey),
                SecretKey = EntityMapper.Map<SecretKey, SecretKeyIpcEntity>(secretKey)
            }
        };
    }

    private VpnServerIpcEntity[] GetVpnServers(IConnectionIntent connectionIntent)
    {
        // TODO consider intent when we implement Countries page and servers selection
        return new List<VpnServerIpcEntity>()
        {
            new VpnServerIpcEntity() // CH#5
            {
                Name = "node-ch-02.protonvpn.net",
                Ip = "185.159.157.6",
                Label = "0",
                X25519PublicKey = new ServerPublicKeyIpcEntity(new PublicKey("00WGV9C77fp+u1G2YrJ3VphcEKFCXcplgUU5THM+QgI=", KeyAlgorithm.X25519)),
                Signature = "7zE5YnKNw5q9pE4BWaPaFzJTTj5NLeHkfhUxMfZynopZDMJCcrubIZhd0F1bWx+q5nIUNqEss+3ORHlzZwwcCg=="
            }
        }.ToArray();
    }
}