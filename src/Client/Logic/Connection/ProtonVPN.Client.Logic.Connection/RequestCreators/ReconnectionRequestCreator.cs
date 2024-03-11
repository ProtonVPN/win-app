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
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
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
        IMainSettingsRequestCreator mainSettingsRequestCreator)
        : base(logger, settings, entityMapper, connectionKeyManager, connectionCertificateManager, intentServerListGenerator,
            smartSecureCoreServerListGenerator, smartStandardServerListGenerator, mainSettingsRequestCreator)
    {
    }

    public override async Task<ConnectionRequestIpcEntity> CreateAsync(IConnectionIntent connectionIntent)
    {
        MainSettingsIpcEntity settings = GetSettings();
        VpnConfigIpcEntity config = GetVpnConfig(settings);
        if (settings.VpnProtocol != VpnProtocolIpcEntity.Smart)
        {
            List<VpnProtocolIpcEntity> preferredProtocols = SmartPreferredProtocols.ToList();
            if (preferredProtocols.Remove(settings.VpnProtocol))
            {
                preferredProtocols.Insert(0, settings.VpnProtocol);
                config.PreferredProtocols = preferredProtocols;
            }
        }

        ConnectionRequestIpcEntity request = new()
        {
            Config = config,
            Credentials = await GetVpnCredentialsAsync(),
            Protocol = VpnProtocolIpcEntity.Smart,
            Servers = PhysicalServersToVpnServerIpcEntities(GetReconnectionPhysicalServers(connectionIntent)),
            Settings = settings,
        };

        return request;
    }

    private IEnumerable<PhysicalServer> GetReconnectionPhysicalServers(IConnectionIntent connectionIntent)
    {
        IEnumerable<PhysicalServer> intentServers = IntentServerListGenerator.Generate(connectionIntent);

        if (!Settings.IsSmartReconnectEnabled || connectionIntent.Feature is B2BFeatureIntent ||
            (connectionIntent.Location is FreeServerLocationIntent fsli && fsli.Type == FreeServerType.Random))
        {
            return intentServers;
        }

        List<PhysicalServer> smartList;

        if (connectionIntent.Feature is SecureCoreFeatureIntent secureCoreFeatureIntent)
        {
            CountryLocationIntent countryLocationIntent = connectionIntent.Location is CountryLocationIntent cli ? cli : new CountryLocationIntent();
            smartList = SmartSecureCoreServerListGenerator.Generate(secureCoreFeatureIntent, countryLocationIntent).ToList();
        }
        else
        {
            smartList = SmartStandardServerListGenerator.Generate(connectionIntent).ToList();
        }

        smartList.AddRange(intentServers.Where(ips => !smartList.Any(slps => slps.Id == ips.Id)));

        return smartList;
    }
}