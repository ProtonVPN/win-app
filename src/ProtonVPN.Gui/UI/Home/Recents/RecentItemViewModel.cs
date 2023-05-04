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

using Windows.ApplicationModel.Resources;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Recents.Contracts;

namespace ProtonVPN.Gui.UI.Home.Recents;

public class RecentItemViewModel
{
    private IRecentConnection _recentConnection;

    public RecentItemViewModel(IRecentConnection recentConnection)
    {
        _recentConnection = recentConnection;

        EntryCountry = _recentConnection.EntryCountryCode;
        ExitCountry = _recentConnection.ExitCountryCode;
        IsPinned = _recentConnection.IsPinned;

        if (!_recentConnection.EntryCountryCode.IsNullOrEmpty() && !_recentConnection.ExitCountryCode.IsNullOrEmpty())
        {
            Title = GetCountryName(_recentConnection.ExitCountryCode);
            Subtitle = " via " + GetCountryName(_recentConnection.EntryCountryCode);
            IsSecureCore = true;
        }
        else if (!_recentConnection.ExitCountryCode.IsNullOrEmpty() && !_recentConnection.City.IsNullOrEmpty())
        {
            Title = GetCountryName(_recentConnection.ExitCountryCode);
            Subtitle = _recentConnection.City;
        }
        else if (!_recentConnection.ExitCountryCode.IsNullOrEmpty() && !_recentConnection.Server.IsNullOrEmpty())
        {
            Title = GetCountryName(_recentConnection.ExitCountryCode);
            Subtitle = _recentConnection.Server;
        }
        else if (!_recentConnection.ExitCountryCode.IsNullOrEmpty())
        {
            Title = GetCountryName(_recentConnection.ExitCountryCode);
        }
    }

    private string GetCountryName(string countryCode)
    {
        return ResourceLoader.GetForViewIndependentUse().GetString($"Country_val_{countryCode}");
    }

    public string Title { get; set; }

    public string Subtitle { get; set; }

    public string EntryCountry { get; set; }

    public string ExitCountry { get; set; }

    public bool IsSecureCore { get; set; }

    public bool IsPinned { get; set; }
    public bool IsRecent { get; set; }

    public bool IsActiveConnection { get; set; }

    public bool HasNoSubtitle => Subtitle.IsNullOrEmpty();
}