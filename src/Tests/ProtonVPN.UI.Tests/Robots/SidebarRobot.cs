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

using System.Threading;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class SidebarRobot
{
    private const int ALL_COUNTRIES_TAB_INDEX = 0;
    private const int SECURE_CORE_COUNTRIES_TAB_INDEX = 1;
    private const int P2P_COUNTRIES_TAB_INDEX = 2;
    private const int TOR_COUNTRIES_TAB_INDEX = 3;
    private const string FASTEST_PROFILE = "Fastest";

    protected Element SidebarComponent = Element.ByAutomationId("SidebarComponent");

    protected Element RecentsListItem = Element.ByName("Recents");
    protected Element CountriesListItem = Element.ByName("Countries");
    protected Element GatewaysListItem = Element.ByName("Gateways");
    protected Element ProfilesListItem = Element.ByName("Profiles");

    protected Element CountryTabs = Element.ByAutomationId("CountriesFeaturesList");

    protected Element CreateProfileButton = Element.ByAutomationId("CreateProfileButton");

    protected Element SearchTextBox = Element.ByAutomationId("SearchTextBox");
    protected Element CountryExpanderButton = Element.ByAutomationId("ExpanderButton");
    protected Element CitySecondaryButton = Element.ByAutomationId("SecondaryButton");
    protected Element SpecificServerConnectionButton = Element.ByAutomationId("ConnectionRowHeader");

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

    public SidebarRobot ConnectToCountry(string country)
    {
        ConnectViaServerList(country);
        return this;
    }

    public SidebarRobot ConnectToCity(string cityName)
    {
        ConnectViaServerList(cityName);
        return this;
    }

    public SidebarRobot ConnectToFastest()
    {
        ConnectViaServerList(FASTEST_PROFILE);
        return this;
    }
    public SidebarRobot ConnectToFirstSpecificServer()
    {
        ConnectViaServerList("Spectific_Server");
        return this;
    }

    public SidebarRobot DisconnectViaCountry(string country)
    {
        DisconnectViaSidebarButton(country);
        return this;
    }

    public SidebarRobot DisconnectViaCity(string city)
    {
        DisconnectViaSidebarButton(city);
        return this;
    }

    public SidebarRobot DisconnectViaSpecificServer()
    {
        DisconnectViaSidebarButton("Spectific_Server");
        return this;
    }

    public SidebarRobot CreateProfile()
    {
        CreateProfileButton.Click();
        return this;
    }

    public SidebarRobot SearchFor(string query)
    {
        SearchTextBox.Click();
        SearchTextBox.SetText(query);
        return this;
    }

    public SidebarRobot ExpandCities()
    {
        CountryExpanderButton.Click();
        // Remove when VPNWIN-2261 is implemented. 
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public SidebarRobot ExpandSpecificServerList()
    {
        CitySecondaryButton.Click();
        return this;
    }

    private SidebarRobot NavigateToCountriesTab(int index)
    {
        NavigateToCountries();
        CountryTabs.ClickItem(index);
        return this;
    }

    private SidebarRobot DisconnectViaSidebarButton(string connectionValue)
    {
        Element countryButton = Element.ByAutomationId($"Disconnect_from_{connectionValue}");
        countryButton.FindChild(Element.ByAutomationId("ConnectionRowHeader")).Click();
        return this;
    }

    private SidebarRobot ConnectViaServerList(string connectionValue)
    {
        Element countryButton = Element.ByAutomationId($"Connect_to_{connectionValue}");
        countryButton.FindChild(Element.ByAutomationId("ConnectionRowHeader")).Click();
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