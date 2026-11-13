/*
 * Copyright (c) 2026 Proton AG
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

using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.Enums.Locations;

namespace ProtonVPN.UI.Tests.TestsHelper.UiFlows;

public class RecentsFlow : BaseTest
{
    public static void PopulateRecentsListWithFastestConnection()
    {
        HomeRobot
            .ConnectViaConnectionCard();
        CommonUiFlows.VerifyIsConnectedThenDisconnect();
    }

    public static void PopulateRecentsListWithCountry(Country country)
    {
        SidebarRobot
            .NavigateToAllCountriesTab()
            .ConnectToCountry(country);
        CommonUiFlows.VerifyIsConnectedThenDisconnect();
    }

    public static void PopulateRecentsListWithCity(Country country, City city)
    {
        SidebarRobot
            .NavigateToAllCountriesTab()
            .ExpandCities(country)
            .ConnectToCity(city);
        CommonUiFlows.VerifyIsConnectedThenDisconnect();
    }

    public static void PopulateRecentsListWithServer(Country country)
    {
        SidebarRobot
            .NavigateToAllCountriesTab()
            .ExpandCities(country)
            .ExpandSpecificServerList()
            .ConnectToServer();
        CommonUiFlows.VerifyIsConnectedThenDisconnect();
    }

    public static void PopulateRecentsListWithSecureCore(Country secureCoreCountry, Country viaCountry)
    {
        SidebarRobot
            .NavigateToSecureCoreCountriesTab()
            .ExpandCities(secureCoreCountry)
            .ConnectViaSecureCore(secureCoreCountry, viaCountry);
        CommonUiFlows.VerifyIsConnectedThenDisconnect();
    }

    public static void PopulateRecentsListWithProfile(string profileName)
    {
        SidebarRobot
            .NavigateToProfiles()
            .ConnectToProfile(profileName);
        CommonUiFlows.VerifyIsConnectedThenDisconnect();
    }
}