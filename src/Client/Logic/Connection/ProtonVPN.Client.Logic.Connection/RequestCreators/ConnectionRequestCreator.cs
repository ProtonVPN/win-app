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
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.UserCertificateLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.RequestCreators;

public class ConnectionRequestCreator : ConnectionRequestCreatorBase, IConnectionRequestCreator
{
    protected readonly IIntentServerListGenerator IntentServerListGenerator;
    protected readonly ISmartSecureCoreServerListGenerator SmartSecureCoreServerListGenerator;
    protected readonly ISmartStandardServerListGenerator SmartStandardServerListGenerator;

    private readonly IConnectionKeyManager _connectionKeyManager;
    private readonly IConnectionCertificateManager _connectionCertificateManager;

    public ConnectionRequestCreator(
        ILogger logger,
        ISettings settings,
        IEntityMapper entityMapper,
        IConnectionKeyManager connectionKeyManager,
        IConnectionCertificateManager connectionCertificateManager,
        IIntentServerListGenerator intentServerListGenerator,
        ISmartSecureCoreServerListGenerator smartSecureCoreServerListGenerator,
        ISmartStandardServerListGenerator smartStandardServerListGenerator,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
        : base(logger, settings, entityMapper, mainSettingsRequestCreator)
    {
        IntentServerListGenerator = intentServerListGenerator;
        SmartSecureCoreServerListGenerator = smartSecureCoreServerListGenerator;
        SmartStandardServerListGenerator = smartStandardServerListGenerator;

        _connectionKeyManager = connectionKeyManager;
        _connectionCertificateManager = connectionCertificateManager;
    }

    public virtual async Task<ConnectionRequestIpcEntity> CreateAsync(IConnectionIntent connectionIntent)
    {
        MainSettingsIpcEntity settings = GetSettings(connectionIntent);
        VpnConfigIpcEntity config = GetVpnConfig(settings, connectionIntent);
        List<VpnProtocol> preferredProtocols = EntityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(config.PreferredProtocols);

        ConnectionRequestIpcEntity request = new()
        {
            RetryId = Guid.NewGuid(),
            Config = config,
            Credentials = await GetVpnCredentialsAsync(),
            Protocol = settings.VpnProtocol,
            Servers = PhysicalServersToVpnServerIpcEntities(GetPhysicalServers(connectionIntent, preferredProtocols)),
            Settings = settings,
        };

        return request;
    }

    protected override async Task<VpnCredentialsIpcEntity> GetVpnCredentialsAsync()
    {
        await RequestCertificateIfNecessaryAsync();

        ConnectionCertificate? connectionCertificate = Settings.ConnectionCertificate;
        AsymmetricKeyPair? keyPair = _connectionKeyManager.GetKeyPairOrNull();

        return new VpnCredentialsIpcEntity
        {
            Certificate = connectionCertificate.HasValue ? CreateCertificate(connectionCertificate.Value) : null,
            ClientKeyPair = keyPair is null ? null : new AsymmetricKeyPairIpcEntity
            {
                PublicKey = EntityMapper.Map<PublicKey, PublicKeyIpcEntity>(keyPair.PublicKey),
                SecretKey = EntityMapper.Map<SecretKey, SecretKeyIpcEntity>(keyPair.SecretKey)
            }
        };
    }

    private async Task RequestCertificateIfNecessaryAsync()
    {
        if (_connectionKeyManager.GetKeyPairOrNull() is null)
        {
            Logger.Info<UserCertificateLog>("Connection keys are missing, forcing new keys and certificate.");
            await _connectionCertificateManager.ForceRequestNewKeyPairAndCertificateAsync();
        }
        else
        {
            ConnectionCertificate? connectionCertificate = Settings.ConnectionCertificate;

            if (connectionCertificate is null)
            {
                Logger.Info<UserCertificateLog>("Connection certificate is missing, requesting a new certificate.");
                await _connectionCertificateManager.RequestNewCertificateAsync();
            }
            else if (connectionCertificate.Value.ExpirationUtcDate <= DateTimeOffset.UtcNow)
            {
                Logger.Info<UserCertificateLog>("Connection certificate is expired, requesting a new certificate.");
                await _connectionCertificateManager.RequestNewCertificateAsync();
            }
        }
    }

    private ConnectionCertificateIpcEntity CreateCertificate(ConnectionCertificate connectionCertificate)
    {
        return new()
        {
            Pem = connectionCertificate.Pem,
            ExpirationDateUtc = connectionCertificate.ExpirationUtcDate.UtcDateTime,
        };
    }

    protected IEnumerable<PhysicalServer> GetPhysicalServers(IConnectionIntent connectionIntent, IList<VpnProtocol> preferredProtocols)
    {
        if (IsToBypassSmartServerListGenerator(connectionIntent))
        {
            return IntentServerListGenerator.Generate(connectionIntent, preferredProtocols);
        }
        else if (connectionIntent.Feature is SecureCoreFeatureIntent secureCoreFeatureIntent)
        {
            CountryLocationIntent countryLocationIntent = connectionIntent.Location as CountryLocationIntent ?? CountryLocationIntent.Fastest;
            return SmartSecureCoreServerListGenerator.Generate(secureCoreFeatureIntent, countryLocationIntent, preferredProtocols);
        }
        else
        {
            return SmartStandardServerListGenerator.Generate(connectionIntent, preferredProtocols);
        }
    }

    protected VpnServerIpcEntity[] PhysicalServersToVpnServerIpcEntities(IEnumerable<PhysicalServer> physicalServers)
    {
        IEnumerable<VpnHost> hosts = physicalServers
            .Select(s => new VpnHost(s.Domain, s.EntryIp, s.Label, GetServerPublicKey(s), s.Signature, s.RelayIpByProtocol));
        return EntityMapper.Map<VpnHost, VpnServerIpcEntity>(hosts).ToArray();
    }

    protected PublicKey? GetServerPublicKey(PhysicalServer server)
    {
        return string.IsNullOrEmpty(server.X25519PublicKey)
            ? null
            : new PublicKey(server.X25519PublicKey, KeyAlgorithm.X25519);
    }
}