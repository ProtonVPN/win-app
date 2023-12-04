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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Wrappers;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.Wrappers;

public class ConnectionRequestWrapper : ConnectionRequestWrapperBase, IConnectionRequestWrapper
{
    private const int MAX_PHYSICAL_SERVERS = 20;

    private readonly IAuthKeyManager _authKeyManager;
    private readonly IServersLoader _serversLoader;
    private readonly Random _random = new();

    public ConnectionRequestWrapper(
        ISettings settings,
        IEntityMapper entityMapper,
        IAuthKeyManager authKeyManager,
        IServersLoader serversLoader)
        : base(settings, entityMapper)
    {
        _authKeyManager = authKeyManager;
        _serversLoader = serversLoader;
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
        IEnumerable<Server> servers = _serversLoader.GetServers();
        ILocationIntent? locationIntent = connectionIntent.Location;
        IFeatureIntent? featureIntent = connectionIntent.Feature;

        if (locationIntent is not null)
        {
            servers = locationIntent.FilterServers(servers);
        }

        servers = featureIntent is null
            ? servers.Where(s =>
                !s.Features.IsSupported(ServerFeatures.SecureCore | ServerFeatures.B2B | ServerFeatures.Tor))
            : featureIntent.FilterServers(servers);

        IEnumerable<VpnHost> hosts = SortServers(servers)
            .SelectMany(s => s.Servers.OrderBy(_ => _random.Next()))
            .Where(s => s.Status != 0)
            .Select(s => new VpnHost(s.Domain, s.EntryIp, s.Label, GetServerPublicKey(s), s.Signature))
            .Distinct(s => (s.Ip, s.Label))
            .Take(MAX_PHYSICAL_SERVERS);

        return EntityMapper.Map<VpnHost, VpnServerIpcEntity>(hosts).ToArray();
    }

    public IEnumerable<Server> SortServers(IEnumerable<Server> source)
    {
        return Settings.IsPortForwardingEnabled
            ? source.OrderByDescending(s => s.Features.IsSupported(ServerFeatures.P2P)).ThenBy(s => s.Score)
            : source.OrderBy(s => s.Score);
    }

    private PublicKey? GetServerPublicKey(PhysicalServer server)
    {
        return string.IsNullOrEmpty(server.X25519PublicKey)
            ? null
            : new PublicKey(server.X25519PublicKey, KeyAlgorithm.X25519);
    }
}