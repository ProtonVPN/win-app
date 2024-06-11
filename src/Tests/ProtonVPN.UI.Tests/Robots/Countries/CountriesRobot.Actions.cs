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

using FlaUI.Core.AutomationElements;

namespace ProtonVPN.UI.Tests.Robots.Countries;

public partial class CountriesRobot
{
    public CountriesRobot DoConnect(string item)
    {
        Button connectButton = GetConnectButton(item);
        connectButton.WaitUntilClickable();
        connectButton.Invoke();
        return this;
    }

    public CountriesRobot DoConnectSecureCore(string entryCountry, string exitCountry)
    {
        GetSecureCoreConnectButton(entryCountry, exitCountry).FocusAndClick();
        return this;
    }

    public CountriesRobot DoNavigateToCountry(string countryCode)
    {
        Button navigateToCountryButton = GetNavigateToCountryButton(countryCode);
        navigateToCountryButton.ScrollIntoView();
        navigateToCountryButton.Invoke();
        return this;
    }

    public CountriesRobot DoShowServers(string city)
    {
        GetShowServersButton(city).Invoke();
        return this;
    }

    public CountriesRobot SearchFor(string query)
    {
        SearchTextBox.Text = query;
        return this;
    }
}