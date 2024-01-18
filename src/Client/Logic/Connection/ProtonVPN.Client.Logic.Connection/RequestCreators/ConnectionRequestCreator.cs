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
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.RequestCreators;

public class ConnectionRequestCreator : ConnectionRequestCreatorBase, IConnectionRequestCreator
{
    protected readonly IIntentServerListGenerator IntentServerListGenerator;
    protected readonly ISmartSecureCoreServerListGenerator SmartSecureCoreServerListGenerator;
    protected readonly ISmartStandardServerListGenerator SmartStandardServerListGenerator;

    private readonly IAuthKeyManager _authKeyManager;

    public ConnectionRequestCreator(
        ISettings settings,
        IEntityMapper entityMapper,
        IAuthKeyManager authKeyManager,
        IIntentServerListGenerator intentServerListGenerator,
        ISmartSecureCoreServerListGenerator smartSecureCoreServerListGenerator,
        ISmartStandardServerListGenerator smartStandardServerListGenerator,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
        : base(settings, entityMapper, mainSettingsRequestCreator)
    {
        IntentServerListGenerator = intentServerListGenerator;
        SmartSecureCoreServerListGenerator = smartSecureCoreServerListGenerator;
        SmartStandardServerListGenerator = smartStandardServerListGenerator;

        _authKeyManager = authKeyManager;
    }

    public virtual ConnectionRequestIpcEntity Create(IConnectionIntent connectionIntent)
    {
        MainSettingsIpcEntity settings = GetSettings();

        ConnectionRequestIpcEntity request = new()
        {
            Config = GetVpnConfig(settings),
            Credentials = GetVpnCredentials(),
            Protocol = settings.VpnProtocol,
            Servers = PhysicalServersToVpnServerIpcEntities(GetPhysicalServers(connectionIntent)),
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

    protected IEnumerable<PhysicalServer> GetPhysicalServers(IConnectionIntent connectionIntent)
    {
        if (connectionIntent.Feature is B2BFeatureIntent || !Settings.IsSmartReconnectEnabled)
        {
            return IntentServerListGenerator.Generate(connectionIntent);
        }
        else if (connectionIntent.Feature is SecureCoreFeatureIntent secureCoreFeatureIntent)
        {
            CountryLocationIntent countryLocationIntent = connectionIntent.Location as CountryLocationIntent ?? new CountryLocationIntent();
            return SmartSecureCoreServerListGenerator.Generate(secureCoreFeatureIntent, countryLocationIntent);
        }
        else
        {
            return SmartStandardServerListGenerator.Generate(connectionIntent);
        }
    }

    protected VpnServerIpcEntity[] PhysicalServersToVpnServerIpcEntities(IEnumerable<PhysicalServer> physicalServers)
    {
        IEnumerable<VpnHost> hosts = physicalServers
            .Select(s => new VpnHost(s.Domain, s.EntryIp, s.Label, GetServerPublicKey(s), s.Signature));
        return EntityMapper.Map<VpnHost, VpnServerIpcEntity>(hosts).ToArray();
    }

    protected PublicKey? GetServerPublicKey(PhysicalServer server)
    {
        return string.IsNullOrEmpty(server.X25519PublicKey)
            ? null
            : new PublicKey(server.X25519PublicKey, KeyAlgorithm.X25519);
    }
}