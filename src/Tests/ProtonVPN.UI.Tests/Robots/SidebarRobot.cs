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

using System;
using System.Threading;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.TestBase;
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
    protected Element ConnectionsPage = Element.ByAutomationId("ConnectionsPage");
    protected Element RecentsPage = Element.ByAutomationId("RecentsPage");
    protected Element CountriesPage = Element.ByAutomationId("CountriesPage");
    protected Element ProfilesPage = Element.ByAutomationId("ProfilesPage");
    protected Element SearchResultsPage = Element.ByAutomationId("SearchResultsPage");

    protected Element NoRecentsLabel = Element.ByName("No recents yet");
    protected Element RemoveRecentLabel = Element.ByName("Remove").FindChild(Element.ByAutomationId("TextBlock"));

    protected Element RecentsLabel = Element.ByAutomationId("ConnectionsPageItem").FindChild(Element.ByName("Recents"));
    protected Element CountriesListItem = Element.ByName("Countries");
    protected Element GatewaysListItem = Element.ByName("Gateways");
    protected Element ProfilesListItem = Element.ByName("Profiles");

    protected Element CountryTabs = Element.ByAutomationId("CountriesFeaturesList");
    protected Element ConnectionItemsList = Element.ByAutomationId("ConnectionItemsList");

    protected Element CreateProfileButton = Element.ByAutomationId("CreateProfileButton");

    protected Element SearchTextBox = Element.ByAutomationId("SearchTextBox");
    protected Element CountryExpanderButton = Element.ByAutomationId("ExpanderButton");
    protected Element SecondaryButton = Element.ByAutomationId("SecondaryButton");
    protected Element SpecificServerConnectionButton = Element.ByAutomationId("ConnectionRowHeader");

    protected Element NetshieldButton = Element.ByName("NetShield");
    protected Element PortForwardingButton = Element.ByName("Port forwarding");
    protected Element SplitTunnelingButton = Element.ByName("Split tunneling");

    protected Element WorldWideCoverageLabel = Element.ByName("Get worldwide coverage with VPN Plus");
    protected Element ProfileSidebarUpsellLabel = Element.ByName("Configure your own VPN settings and connect in one click.");
    protected Element SecureCoreSidebarUpsellLabel = Element.ByName("Add another layer of encryption to your VPN connection");
    protected Element P2pSidebarUpsellLabel = Element.ByName("Download files through BitTorrent and other file sharing protocols");
    protected Element TorSidebarUpsellLabel = Element.ByName("Use the Tor network over your VPN connection for extra privacy");
    protected Element CreateYourFirstProfileLabel = Element.ByName("Create your first profile");
    protected Element ProfileExplanationLabel = Element.ByName("Profiles are saved connections with your choice of location, server, and protocol.");

    protected Element EditProfileLabel = Element.ByName("Edit").FindChild(Element.ByAutomationId("TextBlock"));
    protected Element PinLabel = Element.ByName("Pin").FindChild(Element.ByAutomationId("TextBlock"));
    protected Element DuplicateProfileLabel = Element.ByName("Duplicate").FindChild(Element.ByAutomationId("TextBlock"));
    protected Element DeleteMenuItem = Element.ByAutomationId("DeleteMenuItem");

    public SidebarRobot NavigateToCountries()
    {
        CountriesListItem.Click();
        return this;
    }

    public SidebarRobot NavigateToRecents()
    {
        RecentsLabel.Click();
        return this;
    }

    public SidebarRobot ClickOnNetshieldSetting()
    {
        NetshieldButton.Click();
        return this;
    }

    public SidebarRobot ClickOnPortForwardingButton()
    {
        PortForwardingButton.Click();
        return this;
    }

    public SidebarRobot ClickOnSplitTunnelingButton()
    {
        SplitTunnelingButton.Click();
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
   
    public SidebarRobot ConnectViaServerList(string connectionValue)
    {
        Element countryButton = Element.ByAutomationId($"Connect_to_{connectionValue}");
        countryButton.ScrollIntoView();
        countryButton.FindChild(Element.ByAutomationId("ConnectionRowHeader")).Click();
        return this;
    }

    public SidebarRobot ConnectToProfile(string profileName)
    {
        ConnectViaServerList(profileName);
        return this;
    }

    public SidebarRobot ConnectToCountry(string countryCode)
    {
        ConnectViaServerList(countryCode);
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

    public SidebarRobot DisconnectViaProfile(string profileName)
    {
        DisconnectViaSidebarButton(profileName);
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
        CountryExpanderButton.ExpandItem();
        // Remove when VPNWIN-2599 is implemented. 
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public SidebarRobot ExpandSpecificServerList()
    {
        SecondaryButton.Invoke();
        return this;
    }

    public SidebarRobot ExpandFirstSecondaryActions()
    {
        // Secondary actions expanding is problematic, that's why retry is needed.
        RetryResult<bool> retry = Retry.WhileFalse(() => {
            SecondaryButton.Invoke();
            return !BaseTest.Window.FindFirstDescendant(DeleteMenuItem.Condition).IsOffscreen;
        }, TestConstants.TenSecondsTimeout, ignoreException: true, interval: TestConstants.RetryInterval);

        if (!retry.Success)
        {
            throw new Exception($"{retry.LastException.Message}\n{retry.LastException.StackTrace}");
        }

        // Remove when VPNWIN-2599 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public SidebarRobot ExpandSecondaryActionsForRecents(string connectionName)
    {
        ExpandSecondaryActions(connectionName, PinLabel);
        return this;
    }

    public SidebarRobot ExpandSecondaryActionsForProfile(string connectionName)
    {
        ExpandSecondaryActions(connectionName, DeleteMenuItem);
        return this;
    }

    public SidebarRobot RemoveRecent()
    {
        // First click does not work due to focus on first click.
        // One click is needed for focus, other for clicking.
        RemoveRecentLabel.DoubleClick();
        return this;
    }

    public SidebarRobot EditProfile()
    {
        // First click does not work due to focus on first click.
        // One click is needed for focus, other for clicking.
        EditProfileLabel.DoubleClick();
        return this;
    }

    public SidebarRobot DuplicateProfile()
    {
        // First click does not work due to focus on first click.
        // One click is needed for focus, other for clicking.
        DuplicateProfileLabel.DoubleClick();
        return this;
    }

    public SidebarRobot DeleteProfile()
    {
        DeleteMenuItem.Invoke();
        return this;
    }

    public SidebarRobot ClickOnSidebar()
    {
        SidebarComponent.Click();
        return this;
    }

    public SidebarRobot ShortcutTo(VirtualKeyShort key)
    {
        Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, key);
        return this;
    }

    public SidebarRobot ExitSearchWithTab()
    {
        Keyboard.Type(VirtualKeyShort.TAB);
        return this;
    }

    public SidebarRobot ScrollToProfile(string profileName)
    {
        Element profile = Element.ByAutomationId($"Actions_for_{profileName}");
        profile.ScrollIntoView();
        return this;
    }

    public int GetProfileCount()
    {
        SecondaryButton.WaitUntilDisplayed();
        return BaseTest.Window.FindAllDescendants(SecondaryButton.Condition).Length;
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

    private SidebarRobot ExpandSecondaryActions(string connectionValue, Element elementToWaitFor)
    {
        // Secondary actions expanding is problematic, that's why retry is needed.
        RetryResult<bool> retry = Retry.WhileFalse(() => {
            Element countryButton = Element.ByAutomationId($"Actions_for_{connectionValue}");
            Element secondaryActionsButton = countryButton.FindChild(Element.ByAutomationId("SecondaryButton"));
            secondaryActionsButton.Invoke(TestConstants.OneSecondTimeout);
            return !BaseTest.Window.FindFirstDescendant(elementToWaitFor.Condition).IsOffscreen;
        }, TestConstants.TenSecondsTimeout, ignoreException: true, interval: TestConstants.OneSecondTimeout);

        if (!retry.Success)
        {
            throw new Exception($"{retry.LastException.Message}\n{retry.LastException.StackTrace}");
        }

        // Remove when VPNWIN-2599 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);

        return this;
    }

    public SidebarRobot NavigateToCountriesTabAfterSearch(CountriesTab tab)
    {
        SearchResultsPage.ClickTabByName(tab.ToString());
        return this;
    }

    public class Verifications : SidebarRobot
    {
        public Verifications IsNoRecentsLabelDisplayed()
        {
            NoRecentsLabel.WaitUntilDisplayed();
            return this;
        }

        public Verifications HasNoRecentsLabel()
        {
            NoRecentsLabel.DoesNotExist();
            return this;
        }

        public Verifications IsConnectionOptionDisplayed(string connectionValue)
        {
            Element countryButton = Element.ByAutomationId($"Actions_for_{connectionValue}");
            countryButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsRecentsCountDisplayed(int count)
        {
            string selector = $"Recent{(count == 1 ? "" : "s")} ({count})";

            Element recentsLabel = Element.ByName(selector);
            recentsLabel.WaitUntilDisplayed();

            return this;
        }

        public Verifications IsSidebarAvailable()
        {
            SidebarComponent.WaitUntilDisplayed();
            return this;
        }

        public Verifications DoesConnectionItemExist(string connectionItemName)
        {
            Element.ByName(connectionItemName).WaitUntilDisplayed();
            return this;
        }

        public Verifications DoesConnectionItemNotExist(string connectionItemName)
        {
            Element.ByName(connectionItemName).DoesNotExist();
            return this;
        }

        public Verifications IsAllCountriesUpsellDisplayed()
        {
            WorldWideCoverageLabel.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsSecureCoreUpsellDisplayed()
        {
            SecureCoreSidebarUpsellLabel.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsP2pUpsellDisplayed()
        {
            P2pSidebarUpsellLabel.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsTorUpsellDisplayed()
        {
            TorSidebarUpsellLabel.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsProfileUpsellLabelDisplayed()
        {
            ProfileSidebarUpsellLabel.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsSidebarConnectionsDisplayed() 
        {
            ConnectionsPage.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsSidebarProfilesDisplayed() 
        {
            ProfilesPage.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsSidebarRecentsDisplayed()
        {
            RecentsPage.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsSidebarCountriesDisplayed()
        {
            CountriesPage.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsSidebarSearchResultsDisplayed()
        {
            SearchResultsPage.WaitUntilDisplayed();
            return this;
        }

        public Verifications NoProfilesLabelIsDisplayed()
        {
            CreateYourFirstProfileLabel.WaitUntilDisplayed();
            ProfileExplanationLabel.WaitUntilDisplayed();
            return this;
        }
    }

    public Verifications Verify => new Verifications();
}