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

using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.UI.Settings.Pages.About.Models;
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.About;

public class ReleaseViewModelFactory
{
    private readonly ILocalizationProvider _localizer;

    public ReleaseViewModelFactory(ILocalizationProvider localizer)
    {
        _localizer = localizer;
    }

    public IReadOnlyList<Release> GetReleases(IReadOnlyList<ReleaseContract> releases)
    {
        return releases.Select(r => new Release
        {
            Version = r.Version,
            NewVersionLabel = r.IsNew ? _localizer.Get("Common_Tags_New") : string.Empty,
            BetaVersionLabel = r.IsEarlyAccess ? _localizer.Get("Common_Tags_Beta") : string.Empty,
            ReleaseDate = r.ReleaseDate,
            ChangeLog = r.ChangeLog,
        }).ToList();
    }
}