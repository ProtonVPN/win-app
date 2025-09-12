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

using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.GatewayServers;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Servers;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Migrations;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Settings.Migrations;

public class ProfilesMigrator : IProfilesMigrator
{
    private const string FASTEST_PROFILE_NAME = "Fastest";
    private const string RANDOM_PROFILE_NAME = "Random";

    private readonly IServersLoader _serversLoader;
    private readonly IProfilesManager _profilesManager;
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly ISettings _settings;
    private readonly ILogger _logger;

    public ProfilesMigrator(
        IServersLoader serversLoader,
        IProfilesManager profilesManager,
        IRecentConnectionsManager recentConnectionsManager,
        ISettings settings,
        ILogger logger)
    {
        _serversLoader = serversLoader;
        _profilesManager = profilesManager;
        _recentConnectionsManager = recentConnectionsManager;
        _settings = settings;
        _logger = logger;
    }

    public void Migrate(List<LegacyProfile> legacyProfiles, string? quickConnectProfileId = null)
    {
        List<IConnectionProfile> connectionProfiles = MapProfilesToConnectionProfiles(legacyProfiles);
        _profilesManager.OverrideProfiles(connectionProfiles);

        switch (quickConnectProfileId)
        {
            case FASTEST_PROFILE_NAME:
                _settings.DefaultConnection = DefaultConnection.Fastest;
                break;
            case RANDOM_PROFILE_NAME:
                _settings.DefaultConnection = DefaultConnection.Random;
                break;
            default:
                if (MapQuickConnectProfileToConnectionProfile(legacyProfiles, quickConnectProfileId) is IConnectionProfile quickConnectProfile)
                {
                    _recentConnectionsManager.OverrideRecentConnections([], quickConnectProfile);
                }
                else
                {
                    _settings.DefaultConnection = DefaultSettings.DefaultConnection;
                }
                break;
        }
    }

    private List<IConnectionProfile> MapProfilesToConnectionProfiles(List<LegacyProfile> legacyProfiles)
    {
        List<IConnectionProfile> connectionProfiles = [];

        foreach (LegacyProfile legacyProfile in legacyProfiles)
        {
            IConnectionProfile connectionProfile = GetConnectionProfile(legacyProfile);
            _logger.Info<AppLog>($"Migrated profile '{connectionProfile}'.");
            connectionProfiles.Add(connectionProfile);
        }

        return connectionProfiles;
    }

    private IConnectionProfile GetConnectionProfile(LegacyProfile legacyProfile)
    {
        ILocationIntent? locationIntent;
        IFeatureIntent? featureIntent = null;

        Server? server = string.IsNullOrWhiteSpace(legacyProfile.ServerId)
            ? null
            : GetServerById(legacyProfile, legacyProfile.ServerId);

        if (server is null)
        {
            if (!string.IsNullOrEmpty(legacyProfile.GatewayName))
            {
                locationIntent = SingleGatewayLocationIntent.From(legacyProfile.GatewayName);
            }
            else if (!string.IsNullOrEmpty(legacyProfile.CountryCode))
            {
                locationIntent = SingleCountryLocationIntent.From(legacyProfile.CountryCode);
            }
            else
            {
                locationIntent = MultiCountryLocationIntent.From(GetLegacyProfileSelectionStrategy(legacyProfile));
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(server.GatewayName))
            {
                locationIntent = SingleGatewayServerLocationIntent.From(server.GatewayName, GatewayServerInfo.From(server.Id, server.Name, server.ExitCountry));
            }
            else if (legacyProfile.Features.IsSupported(ServerFeatures.SecureCore))
            {
                locationIntent = SingleCountryLocationIntent.From(server.ExitCountry);
            }
            else
            {
                locationIntent = SingleServerLocationIntent.From(server.ExitCountry, server.State, server.City, ServerInfo.From(server.Id, server.Name));
            }
        }

        if (legacyProfile.Features.IsB2B())
        {
            featureIntent = new B2BFeatureIntent();
        }
        else if (legacyProfile.Features.IsSupported(ServerFeatures.P2P))
        {
            featureIntent = new P2PFeatureIntent();
        }
        else if (legacyProfile.Features.IsSupported(ServerFeatures.Tor))
        {
            featureIntent = new TorFeatureIntent();
        }
        else if (legacyProfile.Features.IsSupported(ServerFeatures.SecureCore))
        {
            featureIntent = new SecureCoreFeatureIntent(server?.EntryCountry);
        }

        Guid profileId = Guid.TryParse(legacyProfile.Id, out Guid result) ? result : Guid.NewGuid();
        string profileName = legacyProfile.Name ?? string.Empty;
        IProfileIcon profileIcon = ProfileIcon.Default;
        profileIcon.Color = MigrateProfileColor(legacyProfile);
        IProfileSettings profileSettings = ProfileSettings.Default;
        profileSettings.VpnProtocol = GetLegacyProfileProtocol(legacyProfile);
        IProfileOptions profileOptions = ProfileOptions.Default;

        return new ConnectionProfile(profileId, DateTime.UtcNow, profileIcon, profileSettings, profileOptions, locationIntent, featureIntent, profileName);
    }

    private Server? GetServerById(LegacyProfile legacyProfile, string serverId)
    {
        Server? server = _serversLoader.GetById(serverId);

        if (server is null)
        {
            _logger.Warn<AppLog>($"No server was found with ID '{serverId}'. The profile '{legacyProfile.Name}' ({legacyProfile.CountryCode}) will be migrated without the server.");
        }

        return server;
    }

    private SelectionStrategy GetLegacyProfileSelectionStrategy(LegacyProfile legacyProfile)
    {
        // TODO: Can we parse Fastest or Random from legacy profile?
        return SelectionStrategy.Fastest;
    }

    private ProfileColor MigrateProfileColor(LegacyProfile legacyProfile)
    {
        return legacyProfile.ColorCode switch
        {
            "#F44236" => ProfileColor.Red, // Red
            "#E91D62" => ProfileColor.Red, // Magenta
            "#9C27B0" => ProfileColor.Purple, // Violet
            "#6739B6" => ProfileColor.Purple, // Purple
            "#3E50B4" => ProfileColor.Blue, // Navy
            "#2195F2" => ProfileColor.Blue, // Blue
            "#01BBD4" => ProfileColor.Blue, // Cyan
            "#029587" => ProfileColor.Green, // Teal
            "#8BC24A" => ProfileColor.Green, // Green
            "#CCDB38" => ProfileColor.Green, // Lime
            "#FFE93B" => ProfileColor.Yellow, // Yellow
            "#FF7044" => ProfileColor.Orange, // Orange
            "#FF9700" => ProfileColor.Yellow, // Gold
            "#607C8A" => ProfileColor.Purple, // Gray
            _ => ProfileIcon.DEFAULT_COLOR,
        };
    }

    private VpnProtocol GetLegacyProfileProtocol(LegacyProfile legacyProfile)
    {
        return legacyProfile?.VpnProtocol is null
            ? VpnProtocol.Smart
            : legacyProfile.VpnProtocol switch
        {
            (int)LegacyVpnProtocol.Smart => VpnProtocol.Smart,
            (int)LegacyVpnProtocol.OpenVpnTcp => VpnProtocol.OpenVpnTcp,
            (int)LegacyVpnProtocol.OpenVpnUdp => VpnProtocol.OpenVpnUdp,
            (int)LegacyVpnProtocol.WireGuardUdp => VpnProtocol.WireGuardUdp,
            (int)LegacyVpnProtocol.WireGuardTcp => VpnProtocol.WireGuardTcp,
            (int)LegacyVpnProtocol.WireGuardTls => VpnProtocol.WireGuardTls,
            _ => VpnProtocol.Smart,
        };
    }

    private IConnectionProfile? MapQuickConnectProfileToConnectionProfile(List<LegacyProfile> profiles, string? quickConnectProfileId)
    {
        if (!string.IsNullOrEmpty(quickConnectProfileId))
        {
            LegacyProfile? profile = profiles.FirstOrDefault(p => p.Id == quickConnectProfileId);
            if (profile is not null)
            {
                IConnectionProfile connectionProfile = GetConnectionProfile(profile);
                _logger.Info<AppLog>($"Migrated quick connect profile '{connectionProfile}'.");
                return connectionProfile;
            }
        }

        return null;
    }
}