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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.Migrations;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Settings.Migrations;

public class ProfilesMigrator : IProfilesMigrator
{
    private const int RANDOM_PROFILE_TYPE = 2;
    private const string FASTEST_PROFILE_NAME = "Fastest";
    private const string RANDOM_PROFILE_NAME = "Random";
    private const ProfileCategory DEFAULT_MIGRATED_PROFILE_CATEGORY = ProfileCategory.Terminal;

    private readonly IServersLoader _serversLoader;
    private readonly IProfilesManager _profilesManager;

    public ProfilesMigrator(
        IServersLoader serversLoader,
        IProfilesManager profilesManager)
    {
        _serversLoader = serversLoader;
        _profilesManager = profilesManager;
    }

    public void Migrate(List<LegacyProfile> legacyProfiles, string? quickConnectProfileId = null)
    {
        if (legacyProfiles is null)
        {
            return;
        }

        List<IConnectionProfile> connectionProfiles = MapProfilesToConnectionProfiles(legacyProfiles);
        IConnectionProfile? quickConnectionProfile = MapQuickConnectProfileToConnectionProfile(legacyProfiles, quickConnectProfileId);

        _profilesManager.OverrideProfiles(connectionProfiles, quickConnectionProfile);
    }

    private List<IConnectionProfile> MapProfilesToConnectionProfiles(List<LegacyProfile> legacyProfiles)
    {
        List<IConnectionProfile> connectionProfiles = [];

        foreach (LegacyProfile legacyProfile in legacyProfiles)
        {
            connectionProfiles.Add(GetConnectionProfile(legacyProfile));
        }

        return connectionProfiles;
    }

    private IConnectionProfile GetConnectionProfile(LegacyProfile legacyProfile)
    {
        ILocationIntent? locationIntent;
        IFeatureIntent? featureIntent = null;

        Server? server = string.IsNullOrEmpty(legacyProfile.ServerId)
            ? null
            : _serversLoader.GetById(legacyProfile.ServerId);

        if (server is null)
        {
            if (!string.IsNullOrEmpty(legacyProfile.GatewayName))
            {
                locationIntent = new GatewayLocationIntent(legacyProfile.GatewayName);
            }
            else if (!string.IsNullOrEmpty(legacyProfile.CountryCode))
            {
                locationIntent = new CountryLocationIntent(legacyProfile.CountryCode,
                    GetLegacyProfileConnectionIntentKind(legacyProfile));
            }
            else
            {
                locationIntent = new CountryLocationIntent(
                    GetLegacyProfileConnectionIntentKind(legacyProfile));
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(server.GatewayName))
            {
                locationIntent = new GatewayServerLocationIntent(server.Id, server.Name, server.ExitCountry, server.GatewayName);
            }
            else if (legacyProfile.Features.IsSupported(ServerFeatures.SecureCore))
            {
                locationIntent = new CountryLocationIntent(server.ExitCountry);
            }
            else
            {
                locationIntent = new ServerLocationIntent(server.Id, server.Name, server.ExitCountry, server.State, server.City);
            }
        }

        if (legacyProfile.Features.IsSupported(ServerFeatures.B2B))
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

    private ConnectionIntentKind GetLegacyProfileConnectionIntentKind(LegacyProfile legacyProfile)
    {
        return ConnectionIntentKind.Fastest;
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
        if (!string.IsNullOrEmpty(quickConnectProfileId) &&
            quickConnectProfileId != FASTEST_PROFILE_NAME &&
            quickConnectProfileId != RANDOM_PROFILE_NAME)
        {
            LegacyProfile? profile = profiles.FirstOrDefault(p => p.Id == quickConnectProfileId);
            if (profile is not null)
            {
                return GetConnectionProfile(profile);
            }
        }

        return null;
    }
}