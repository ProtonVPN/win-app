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

using System.Threading;
using FlaUI.Core.AutomationElements;

namespace ProtonVPN.UI.Tests.Robots.Countries;

public partial class CountriesRobot
{
    public CountriesRobot DoConnectTo(string entryCountryCode, string cityState = null, int? serverNumber = null)
    {
        if (!string.IsNullOrEmpty(entryCountryCode))
        {
            EntryCountryCodeTextBox.Text = entryCountryCode;
        }

        if (!string.IsNullOrEmpty(cityState))
        {
            CityStateTextBox.WaitUntilEnabled();
            CityStateTextBox.Text = cityState;
        }

        if (serverNumber != null)
        {
            ServerTextBox.WaitUntilEnabled();
            ServerTextBox.Text = serverNumber.ToString();
        }

        //Give some time for the app to process configured values.
        this.Wait(1000);
        CountriesConnectButton.Focus();
        CountriesConnectButton.Click();

        return this;
    }
}