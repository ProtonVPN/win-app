/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.RequestCreators;

public class ReconnectionRequestCreator : ConnectionRequestCreator, IReconnectionRequestCreator
{
    public ReconnectionRequestCreator(
        ISettings settings,
        ILogger logger,
        IEntityMapper entityMapper,
        IConnectionKeyManager connectionKeyManager,
        IConnectionCertificateManager connectionCertificateManager,
        IIntentServerListGenerator intentServerListGenerator,
        ISmartSecureCoreServerListGenerator smartSecureCoreServerListGenerator,
        ISmartStandardServerListGenerator smartStandardServerListGenerator,
        IFeatureFlagsObserver featureFlagsObserver,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
        : base(logger, settings, entityMapper, connectionKeyManager, connectionCertificateManager, intentServerListGenerator,
            smartSecureCoreServerListGenerator, smartStandardServerListGenerator, featureFlagsObserver, mainSettingsRequestCreator)
    {
    }

    public override async Task<ConnectionRequestIpcEntity> CreateAsync(IConnectionIntent connectionIntent)
    {
        MainSettingsIpcEntity settings = GetSettings(connectionIntent);
        VpnConfigIpcEntity config = GetVpnConfig(settings, connectionIntent);

        // If the protocol in the settings is a specific one (not Smart), put it at the top of the smart protocol list
        if (settings.VpnProtocol != VpnProtocolIpcEntity.Smart)
        {
            IList<VpnProtocolIpcEntity> preferredProtocols = GetPreferredSmartProtocols();
            preferredProtocols.Remove(settings.VpnProtocol);
            // Insert even if the protocol doesn't exist in the smart protocol list (ex.: Countries with only Stealth)
            preferredProtocols.Insert(0, settings.VpnProtocol);
            config.PreferredProtocols = preferredProtocols;
        }

        IEnumerable<PhysicalServer> physicalServers = GetReconnectionPhysicalServers(connectionIntent,
            EntityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(config.PreferredProtocols));

        ConnectionRequestIpcEntity request = new()
        {
            RetryId = Guid.NewGuid(),
            Config = config,
            Credentials = await GetVpnCredentialsAsync(),
            Protocol = VpnProtocolIpcEntity.Smart,
            Servers = PhysicalServersToVpnServerIpcEntities(physicalServers),
            Settings = settings,
        };

        return request;
    }

    private IEnumerable<PhysicalServer> GetReconnectionPhysicalServers(IConnectionIntent connectionIntent, IList<VpnProtocol> preferredProtocols)
    {
        IEnumerable<PhysicalServer> intentServers = IntentServerListGenerator.Generate(connectionIntent, preferredProtocols);

        if (IsToBypassSmartServerListGenerator(connectionIntent))
        {
            return intentServers;
        }

        List<PhysicalServer> smartList;

        if (connectionIntent.Feature is SecureCoreFeatureIntent secureCoreFeatureIntent)
        {
            CountryLocationIntent countryLocationIntent = connectionIntent.Location is CountryLocationIntent cli ? cli : new CountryLocationIntent();
            smartList = SmartSecureCoreServerListGenerator.Generate(secureCoreFeatureIntent, countryLocationIntent, preferredProtocols).ToList();
        }
        else
        {
            smartList = SmartStandardServerListGenerator.Generate(connectionIntent, preferredProtocols).ToList();
        }

        smartList.AddRange(intentServers.Where(ips => !smartList.Any(slps => slps.Id == ips.Id)));

        return smartList;
    }
}