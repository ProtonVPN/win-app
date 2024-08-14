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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Profiles;

public class DefaultProfilesProvider : IDefaultProfilesProvider
{
    private List<IConnectionProfile> _defaultProfiles;

    public DefaultProfilesProvider()
    {
        ResetDefaultProfiles();
    }

    private void ResetDefaultProfiles()
    {
        _defaultProfiles =
        [
            GetDefaultStreamingProfile(),
            GetDefaultWorkProfile(),
            GetDefaultAntiCensorshipProfile()
        ];
    }

    public List<IConnectionProfile> GetDefaultProfiles()
    {
        return _defaultProfiles.ToList();
    }

    private static IConnectionProfile GetDefaultStreamingProfile()
    {
        ILocationIntent locationIntent = new CountryLocationIntent("US");

        IConnectionProfile profile = new ConnectionProfile(locationIntent)
        {
            // TODO: Should profile name be localized?
            Name = "Streaming US",
            Category = ProfileCategory.Streaming,
            Color = ProfileColor.Purple
        };

        profile.Settings.Protocol = VpnProtocol.WireGuardUdp;

        return profile;
    }

    private static IConnectionProfile GetDefaultWorkProfile()
    {
        ILocationIntent locationIntent = new CountryLocationIntent();

        IConnectionProfile profile = new ConnectionProfile(locationIntent)
        {
            Name = "Work/School",
            Category = ProfileCategory.Business,
            Color = ProfileColor.Purple
        };

        // TODO: Stealth protocol is under feature flag. Check that it is available. Replace with Smart otherwise.
        profile.Settings.Protocol = VpnProtocol.WireGuardTls;

        return profile;
    }

    private static IConnectionProfile GetDefaultAntiCensorshipProfile()
    {
        // TODO: Replace with fastest excluding my country
        ILocationIntent locationIntent = new CountryLocationIntent();

        IConnectionProfile profile = new ConnectionProfile(locationIntent)
        {
            Name = "Anti-censorship",
            Category = ProfileCategory.Anonymous,
            Color = ProfileColor.Purple
        };

        // TODO: Stealth protocol is under feature flag. Check that it is available. Replace with Smart otherwise.
        profile.Settings.Protocol = VpnProtocol.WireGuardTls;

        return profile;
    }
}