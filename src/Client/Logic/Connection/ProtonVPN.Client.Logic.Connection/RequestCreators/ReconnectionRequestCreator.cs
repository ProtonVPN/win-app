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
        IServerListGenerator serverListGenerator,
        ISmartServerListGenerator smartServerListGenerator,
        IFeatureFlagsObserver featureFlagsObserver,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
        : base(logger,
               settings,
               entityMapper,
               connectionKeyManager,
               connectionCertificateManager,
               serverListGenerator,
               smartServerListGenerator,
               featureFlagsObserver,
               mainSettingsRequestCreator)
    { }

    public override async Task<ConnectionRequestIpcEntity> CreateAsync(IConnectionIntent connectionIntent)
    {
        MainSettingsIpcEntity settings = GetSettings(connectionIntent);
        VpnConfigIpcEntity config = GetVpnConfig(settings, connectionIntent);

        // If the protocol in the settings is a specific one (not Smart), put it at the top of the smart protocol list
        if (settings.VpnProtocol != VpnProtocolIpcEntity.Smart)
        {
            IList<VpnProtocolIpcEntity> smartProtocols = GetPreferredSmartProtocols();
            smartProtocols.Remove(settings.VpnProtocol);
            // Insert even if the protocol doesn't exist in the smart protocol list (ex.: Countries with only Stealth)
            smartProtocols.Insert(0, settings.VpnProtocol);
            config.PreferredProtocols = smartProtocols;
        }

        List<VpnProtocol> preferredProtocols = EntityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(config.PreferredProtocols);
        IEnumerable<PhysicalServer> physicalServers = GetReconnectionPhysicalServers(connectionIntent, preferredProtocols);

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
        IEnumerable<PhysicalServer> intentServers = ServerListGenerator.Generate(connectionIntent, preferredProtocols);

        if (IsToBypassSmartServerListGenerator(connectionIntent))
        {
            return intentServers;
        }

        List<PhysicalServer> smartList = SmartServerListGenerator.Generate(connectionIntent, preferredProtocols).ToList();

        smartList.AddRange(intentServers.Where(ips => !smartList.Any(slps => slps.Id == ips.Id)));

        return smartList;
    }
}