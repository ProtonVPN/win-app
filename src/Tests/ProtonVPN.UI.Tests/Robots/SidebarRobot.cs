/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class SidebarRobot
{
    private const int ALL_COUNTRIES_TAB_INDEX = 0;
    private const int SECURE_CORE_COUNTRIES_TAB_INDEX = 1;
    private const int P2P_COUNTRIES_TAB_INDEX = 2;
    private const int TOR_COUNTRIES_TAB_INDEX = 3;

    protected Element SidebarComponent = Element.ByAutomationId("SidebarComponent");

    protected Element RecentsListItem = Element.ByName("Recents");
    protected Element CountriesListItem = Element.ByName("Countries");
    protected Element GatewaysListItem = Element.ByName("Gateways");
    protected Element ProfilesListItem = Element.ByName("Profiles");


    protected Element CountryTabs = Element.ByAutomationId("CountriesFeaturesList");

    protected Element CreateProfileButton = Element.ByAutomationId("CreateProfileButton");

    public SidebarRobot NavigateToRecents()
    {
        RecentsListItem.Click();
        return this;
    }

    public SidebarRobot NavigateToCountries()
    {
        CountriesListItem.Click();
        return this;
    }

    public SidebarRobot NavigateToAllCountriesTab()
    {
        return NavigateToCountriesTab(ALL_COUNTRIES_TAB_INDEX);
    }

    public SidebarRobot NavigateToSecureCoreCountriesTab()
    {
        return NavigateToCountriesTab(SECURE_CORE_COUNTRIES_TAB_INDEX);
    }

    public SidebarRobot NavigateToP2PCountriesTab()
    {
        return NavigateToCountriesTab(P2P_COUNTRIES_TAB_INDEX);
    }

    public SidebarRobot NavigateToTorCountriesTab()
    {
        return NavigateToCountriesTab(TOR_COUNTRIES_TAB_INDEX);
    }

    public SidebarRobot NavigateToGateways()
    {
        GatewaysListItem.Click();
        return this;
    }

    public SidebarRobot NavigateToProfiles()
    {
        ProfilesListItem.Click();
        return this;
    }

    public SidebarRobot ConnectToCountry(string countryName)
    {
        Element countryButton = Element.ByName(countryName);
        countryButton.Click();
        return this;
    }

    public SidebarRobot DisconnectFromCountry(string countryName)
    {
        Element countryButton = Element.ByName(countryName);
        countryButton.Click();
        return this;
    }

    public SidebarRobot CreateProfile()
    {
        CreateProfileButton.Click();
        return this;
    }

    private SidebarRobot NavigateToCountriesTab(int index)
    {
        NavigateToCountries();
        CountryTabs.ClickItem(index);
        return this;
    }

    public class Verifications : SidebarRobot
    {
        public Verifications IsSidebarAvailable()
        {
            SidebarComponent.WaitUntilDisplayed();
            return this;
        }

        public Verifications ConnectionItemExists(string connectionItemName)
        {
            Element.ByName(connectionItemName).WaitUntilDisplayed();
            return this;
        }
    }

    public Verifications Verify => new Verifications();
}