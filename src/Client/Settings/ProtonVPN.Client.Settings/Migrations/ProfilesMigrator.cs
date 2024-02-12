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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Migrations;

namespace ProtonVPN.Client.Settings.Migrations;

public class ProfilesMigrator : IProfilesMigrator
{
    private const int RANDOM_PROFILE_TYPE = 2;
    private const string FASTEST_PROFILE_NAME = "Fastest";
    private const string RANDOM_PROFILE_NAME = "Random";

    private readonly ISettings _settings;
    private readonly IServersLoader _serversLoader;
    private readonly IRecentConnectionsProvider _recentConnectionsProvider;

    public ProfilesMigrator(ISettings settings,
        IServersLoader serversLoader,
        IRecentConnectionsProvider recentConnectionsProvider)
    {
        _settings = settings;
        _serversLoader = serversLoader;
        _recentConnectionsProvider = recentConnectionsProvider;
    }

    public void Migrate(List<LegacyProfile> profiles, string? quickConnectProfileId = null)
    {
        if (profiles is null)
        {
            return;
        }

        List<IConnectionIntent> connectionIntents = MapProfilesToConnectionIntents(profiles);
        IConnectionIntent? firstConnectionIntent = MapQuickConnectProfileToConnectionIntent(profiles, quickConnectProfileId);

        _recentConnectionsProvider.SaveRecentConnections(connectionIntents, firstConnectionIntent);
    }

    private List<IConnectionIntent> MapProfilesToConnectionIntents(List<LegacyProfile> profiles)
    {
        List<IConnectionIntent> connectionIntents = [];

        foreach (LegacyProfile profile in profiles)
        {
            if (profile.ProfileType == RANDOM_PROFILE_TYPE)
            {
                continue;
            }

            IConnectionIntent connectionIntent = GetConnectionIntent(profile);
            connectionIntents.Add(connectionIntent);
        }

        return connectionIntents;
    }

    private IConnectionIntent GetConnectionIntent(LegacyProfile profile)
    {
        ILocationIntent? locationIntent = null;
        IFeatureIntent? featureIntent = null;

        Server? server = string.IsNullOrEmpty(profile.ServerId)
            ? null
            : _serversLoader.GetById(profile.ServerId);

        if (server is null)
        {
            if (!string.IsNullOrEmpty(profile.GatewayName))
            {
                locationIntent = new GatewayLocationIntent(profile.GatewayName);
            }
            else if (!string.IsNullOrEmpty(profile.CountryCode))
            {
                locationIntent = new CountryLocationIntent(profile.CountryCode);
            }
            else
            {
                locationIntent = new CountryLocationIntent();
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(server.GatewayName))
            {
                locationIntent = new GatewayServerLocationIntent(server.Id, server.Name, server.ExitCountry, server.GatewayName);
            }
            else if (profile.Features.IsSupported(ServerFeatures.SecureCore))
            {
                locationIntent = new CountryLocationIntent(server.ExitCountry);
            }
            else
            {
                locationIntent = new ServerLocationIntent(server.Id, server.Name, server.ExitCountry, server.City);
            }
        }

        if (profile.Features.IsSupported(ServerFeatures.B2B))
        {
            featureIntent = new B2BFeatureIntent();
        }
        else if (profile.Features.IsSupported(ServerFeatures.P2P))
        {
            featureIntent = new P2PFeatureIntent();
        }
        else if (profile.Features.IsSupported(ServerFeatures.Tor))
        {
            featureIntent = new TorFeatureIntent();
        }
        else if (profile.Features.IsSupported(ServerFeatures.SecureCore))
        {
            featureIntent = new SecureCoreFeatureIntent(server?.EntryCountry);
        }

        return new ConnectionIntent(locationIntent, featureIntent);
    }

    private IConnectionIntent? MapQuickConnectProfileToConnectionIntent(List<LegacyProfile> profiles, string? quickConnectProfileId)
    {
        if (!string.IsNullOrEmpty(quickConnectProfileId) &&
            quickConnectProfileId != FASTEST_PROFILE_NAME &&
            quickConnectProfileId != RANDOM_PROFILE_NAME)
        {
            LegacyProfile? profile = profiles.FirstOrDefault(p => p.Id == quickConnectProfileId);
            if (profile is not null)
            {
                return GetConnectionIntent(profile);
            }
        }

        return null;
    }
}