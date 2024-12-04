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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Profiles;

public class DefaultProfilesProvider : IDefaultProfilesProvider
{
    private readonly ILocalizationProvider _localizer;

    private readonly List<IConnectionProfile> _defaultProfiles;

    public DefaultProfilesProvider(
        ILocalizationProvider localizer)
    {
        _localizer = localizer;

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

    private IConnectionProfile GetDefaultStreamingProfile()
    {
        ILocationIntent locationIntent = new CountryLocationIntent("US");

        IConnectionProfile profile = new ConnectionProfile(locationIntent)
        {
            Name = _localizer.Get("Profiles_Default_Streaming"),
            Category = ProfileCategory.Streaming,
            Color = ProfileColor.Purple
        };

        profile.Settings.Protocol = VpnProtocol.WireGuardUdp;

        return profile;
    }

    private IConnectionProfile GetDefaultWorkProfile()
    {
        ILocationIntent locationIntent = CountryLocationIntent.Fastest;

        IConnectionProfile profile = new ConnectionProfile(locationIntent)
        {
            Name = _localizer.Get("Profiles_Default_WorkSchool"),
            Category = ProfileCategory.Business,
            Color = ProfileColor.Purple
        };

        profile.Settings.Protocol = VpnProtocol.WireGuardTls;

        return profile;
    }

    private IConnectionProfile GetDefaultAntiCensorshipProfile()
    {
        ILocationIntent locationIntent = CountryLocationIntent.FastestExcludingMyCountry;

        IConnectionProfile profile = new ConnectionProfile(locationIntent)
        {
            Name = _localizer.Get("Profiles_Default_AntiCensorship"),
            Category = ProfileCategory.Anonymous,
            Color = ProfileColor.Purple
        };

        profile.Settings.Protocol = VpnProtocol.WireGuardTls;

        return profile;
    }
}