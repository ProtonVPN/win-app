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

using System;
using System.Threading;
using System.Collections.Generic;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.Core.AutomationElements;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Enums.Locations;

namespace ProtonVPN.UI.Tests.Robots;

public class SidebarRobot
{
    private const int ALL_COUNTRIES_TAB_INDEX = 0;
    private const int SECURE_CORE_COUNTRIES_TAB_INDEX = 1;
    private const int P2P_COUNTRIES_TAB_INDEX = 2;
    private const int TOR_COUNTRIES_TAB_INDEX = 3;
    private const int MINIMUM_EXPECTED_COUNTRY_COUNT = 80;

    private static readonly string _serverLoadInfo = LanguageHelper.GetTranslatedString("Overlay_ServerLoad_Description").Replace("\\n", "\r\n");
    private static readonly string _profilesInfo = LanguageHelper.GetTranslatedString("Overlay_Profile_Description1") + "\r\n\r\n" + LanguageHelper.GetTranslatedString("Overlay_Profile_Description2");
    private static readonly string _secureCoreInfo = LanguageHelper.GetTranslatedString("Overlay_SecureCore_Description1") + "\r\n\r\n" + LanguageHelper.GetTranslatedString("Overlay_SecureCore_Description2");
    private static readonly string _p2pInfo = LanguageHelper.GetTranslatedString("Overlay_P2P_Bullet2_Description");
    private static readonly string _torInfo = LanguageHelper.GetTranslatedString("Overlay_Tor_Description");

    private static readonly string _worldWideCoverageLabelTranslated = LanguageHelper.GetTranslatedString("FreeConnections_WorldwideCoverageUpsell");
    private static readonly string _profileSidebarUpsellLabelTranslated = LanguageHelper.GetTranslatedString("Profiles_Page_Description");
    private static readonly string _secureCoreSidebarUpsellLabelTranslated = LanguageHelper.GetTranslatedString("FreeConnections_SecureCoreUpsell");
    private static readonly string _p2pSidebarUpsellLabelTranslated = LanguageHelper.GetTranslatedString("FreeConnections_P2PUpsell");
    private static readonly string _torSidebarUpsellLabelTranslated = LanguageHelper.GetTranslatedString("FreeConnections_TorUpsell");
    private static readonly string _createYourFirstProfileLabelTranslated = LanguageHelper.GetTranslatedString("Connections_Profiles_Empty_Title");
    private static readonly string _profileExplanationLabelTranslated = LanguageHelper.GetTranslatedString("Connections_Profiles_Empty_Description");
    private static readonly string _recentsListItemTranslated = LanguageHelper.GetTranslatedString("Home_Recents_Title");
    private static readonly string _profilesListItemTranslated = LanguageHelper.GetTranslatedString("Profiles_Page_Title");
    private static readonly string _gatewaysListItemTranslated = LanguageHelper.GetTranslatedString("Gateways_Page_Title");
    private static readonly string _noRecentsLabelTranslated = LanguageHelper.GetTranslatedString("Connections_Recents_Empty_Title");
    private static readonly string _disconnectButtonOnHoverTranslated = LanguageHelper.GetTranslatedString("Common_Actions_Disconnect");
    private static string CountriesListItemTranslated => LanguageHelper.GetTranslatedString("Countries");

    protected Element CountriesListItem => Element.ByName(CountriesListItemTranslated);
    protected Element WorldWideCoverageLabel = Element.ByName(_worldWideCoverageLabelTranslated);
    protected Element ProfileSidebarUpsellLabel = Element.ByName(_profileSidebarUpsellLabelTranslated);
    protected Element SecureCoreSidebarUpsellLabel = Element.ByName(_secureCoreSidebarUpsellLabelTranslated);
    protected Element P2PSidebarUpsellLabel = Element.ByName(_p2pSidebarUpsellLabelTranslated);
    protected Element TorSidebarUpsellLabel = Element.ByName(_torSidebarUpsellLabelTranslated);
    protected Element CreateYourFirstProfileLabel = Element.ByName(_createYourFirstProfileLabelTranslated);
    protected Element ProfileExplanationLabel = Element.ByName(_profileExplanationLabelTranslated);
    protected Element RecentsListItem = Element.ByName(_recentsListItemTranslated);
    protected Element ProfilesListItem = Element.ByName(_profilesListItemTranslated);
    protected Element GatewaysListItem = Element.ByName(_gatewaysListItemTranslated);
    protected Element NoRecentsLabel = Element.ByName(_noRecentsLabelTranslated);
    protected Element DisconnectButtonOnHover = Element.ByAutomationId("ConnectionRowAction").And(Element.ByName(_disconnectButtonOnHoverTranslated));

    protected Element SidebarComponent = Element.ByAutomationId("SidebarComponent");
    protected Element ConnectionsPage = Element.ByAutomationId("ConnectionsPage");
    protected Element RecentsPage = Element.ByAutomationId("RecentsPage");
    protected Element CountriesPage = Element.ByAutomationId("CountriesPage");
    protected Element ProfilesPage = Element.ByAutomationId("ProfilesPage");
    protected Element SearchResultsPage = Element.ByAutomationId("SearchResultsPage");

    protected Element PinRecentLabel = Element.ByAutomationId("PinRecentMenuItem");
    protected Element UnpinRecentLabel = Element.ByAutomationId("UnpinRecentMenuItem");
    protected Element RemoveRecentLabel = Element.ByAutomationId("RemoveMenuItem");

    protected Element CountryTabs = Element.ByAutomationId("CountriesFeaturesList");
    protected Element ConnectionItemsList = Element.ByAutomationId("ConnectionItemsList");

    protected Element CreateProfileButton = Element.ByAutomationId("CreateProfileButton");

    protected Element SearchTextBox = Element.ByAutomationId("SearchTextBox");
    protected Element SearchBackButton = Element.ByAutomationId("SearchBackButton");
    protected Element CountryExpanderButton = Element.ByAutomationId("ExpanderButton");
    protected Element SecondaryButton = Element.ByAutomationId("SecondaryButton");

    protected Element CountryInfoBanner => Element.ByAutomationId("ProminentBannerDescription");
    protected Element TabInfoButton = Element.ByAutomationId("TabInfoButton");
    protected Element ServerLoadInfoButton = Element.ByAutomationId("ServerLoadInfoButton");
    protected Element CloseContentDialogButton = Element.ByAutomationId("CloseContentDialogButton");
    protected Element OverlayMessage = Element.ByAutomationId("OverlayMessage");

    protected Element EditProfileLabel = Element.ByAutomationId("EditMenuItem");
    protected Element DuplicateProfileLabel = Element.ByAutomationId("DuplicateMenuItem");
    protected Element DeleteMenuItem = Element.ByAutomationId("DeleteMenuItem");

    protected Element CountriesListGroup = Element.ByClassName("ListViewHeaderItem");
    protected Element ConnectionItemsHeader = Element.ByAutomationId("ConnectionItemsHeader");

    protected Element DisconnectFromSpecificServer = Element.ByAutomationId("Disconnect_from_Specific_Server");
    protected Element ConnectToSpecificServer => Element.ByAutomationId("Connect_to_Specific_Server");

    public SidebarRobot NavigateToCountries()
    {
        CountriesListItem.Click();
        return this;
    }

    public SidebarRobot NavigateToRecents()
    {
        RecentsListItem.Click();
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
        Thread.Sleep(TestConstants.OneSecondTimeout);
        countryButton.ScrollIntoView();
        countryButton.FindChild(Element.ByAutomationId("ConnectionRowHeader")).Click();
        return this;
    }

    public SidebarRobot ConnectViaSecureCore(Country countryName, Country viaCountry)
    {
        Element countryButton = Element.ByAutomationId($"Connect_to_{countryName.GetName()}");
        countryButton.ScrollIntoView();
        Element.ByName(TestConstants.ViaPrefix + viaCountry.GetName()).Click();
        return this;
    }

    public SidebarRobot DisconnectViaSecureCore(Country countryName, Country viaCountry)
    {
        Element countryButton = Element.ByAutomationId($"Disconnect_from_{countryName.GetName()}");
        countryButton.ScrollIntoView();
        countryButton.FindChild(Element.ByName(TestConstants.ViaPrefix + viaCountry.GetName())).Click();
        return this;
    }

    public SidebarRobot ConnectToProfile(string profileName)
    {
        ConnectViaServerList(profileName);
        return this;
    }

    public SidebarRobot ConnectToCountry(Country countryName)
    {
        ConnectViaServerList(countryName.GetCode());
        return this;
    }

    public SidebarRobot ConnectToCity(City cityName)
    {
        ConnectViaServerList(cityName.GetEnumValue());
        return this;
    }

    public SidebarRobot ConnectToState(State stateName)
    {
        ConnectViaServerList(stateName.GetEnumValue());
        return this;
    }

    public SidebarRobot ConnectToFastest()
    {
        ConnectViaServerList("Fastest");
        return this;
    }

    public SidebarRobot ConnectToServer(string server)
    {
        ConnectToSpecificServer.And(Element.ByName(server)).Click();
        return this;
    }

    public SidebarRobot ConnectToServer()
    {
        ConnectViaServerList("Specific_Server");
        return this;
    }

    public SidebarRobot DisconnectViaProfile(string profileName)
    {
        DisconnectViaSidebarButton(profileName);
        return this;
    }

    public SidebarRobot DisconnectViaCountry(Country countryName)
    {
        DisconnectViaSidebarButton(countryName.GetCode());
        return this;
    }

    public SidebarRobot DisconnectViaCity(City cityName)
    {
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        DisconnectViaSidebarButton(cityName.GetEnumValue());
        return this;
    }

    public SidebarRobot DisconnectViaState(State stateName)
    {
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        DisconnectViaSidebarButton(stateName.GetEnumValue());
        return this;
    }

    public SidebarRobot DisconnectViaServer(string server)
    {
        DisconnectFromSpecificServer.And(Element.ByName(server)).Click();
        return this;
    }

    public SidebarRobot DisconnectViaServer()
    {
        DisconnectViaSidebarButton("Specific_Server");
        return this;
    }

    public SidebarRobot ClickCreateProfile()
    {
        Thread.Sleep(TestConstants.NavigationDelay);
        CreateProfileButton.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SidebarRobot ClickSearchBox()
    {
        SearchTextBox.Click();
        return this;
    }

    public SidebarRobot ClickXButtonInSearchBox()
    {
        SearchTextBox.ClearSearch();
        return this;
    }

    public SidebarRobot ClickBackButtonInSearchBox()
    {
        SearchTextBox.FindChild(SearchBackButton).Click();
        return this;
    }

    public SidebarRobot SearchFor(string query)
    {
        ClickSearchBox();
        SearchTextBox.SetText(query);
        return this;
    }

    public SidebarRobot ClickServerLoadInfoButton()
    {
        ServerLoadInfoButton.Click();
        return this;
    }

    public SidebarRobot ClickTabInfoButton()
    {
        TabInfoButton.Click();
        return this;
    }

    public SidebarRobot CloseTabInfoModal()
    {
        CloseContentDialogButton.Click();
        return this;
    }

    public SidebarRobot ExpandCities(Country countryName)
    {
        Thread.Sleep(TestConstants.OneSecondTimeout);
        Element.ByAutomationId($"Navigate_to_{countryName.GetCode()}").FindChild(CountryExpanderButton).ExpandItem();
        // Remove when VPNWIN-2599 is implemented. 
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public SidebarRobot ExpandSpecificServerList()
    {
        SecondaryButton.Invoke();
        Thread.Sleep(TestConstants.OneSecondTimeout);
        return this;
    }

    public SidebarRobot ExpandFirstSecondaryActions()
    {
        // Secondary actions expanding is problematic, that's why retry is needed.
        RetryResult<bool> retry = Retry.WhileFalse(() =>
        {
            SecondaryButton.Invoke();
            AutomationElement? descendant = BaseTest.Window?.FindFirstDescendant(DeleteMenuItem.Condition);
            return descendant != null && !descendant.IsOffscreen;
        }, TestConstants.TenSecondsTimeout, ignoreException: true, interval: TestConstants.RetryInterval);

        if (!retry.Success)
        {
            throw new Exception($"{retry.LastException?.Message}\n{retry.LastException?.StackTrace}");
        }

        // Remove when VPNWIN-2599 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public SidebarRobot ExpandSecondaryActionsForRecents(string connectionName)
    {
        ExpandSecondaryActions(connectionName, RemoveRecentLabel);
        return this;
    }

    public SidebarRobot ExpandSecondaryActionsForProfile(string connectionName)
    {
        ExpandSecondaryActions(connectionName, DeleteMenuItem);
        return this;
    }

    public SidebarRobot PinRecent()
    {
        PinRecentLabel.DoubleClick();
        return this;
    }

    public SidebarRobot UnpinRecent()
    {
        UnpinRecentLabel.DoubleClick();
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
        return BaseTest.Window?.FindAllDescendants(SecondaryButton.Condition).Length ?? 0;
    }

    private SidebarRobot NavigateToCountriesTab(int index)
    {
        NavigateToCountries();
        CountryTabs.ClickItem(index);
        Thread.Sleep(TestConstants.NavigationDelay);
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
        RetryResult<bool> retry = Retry.WhileFalse(() =>
        {
            Element countryButton = Element.ByAutomationId($"Actions_for_{connectionValue}");
            Element secondaryActionsButton = countryButton.FindChild(Element.ByAutomationId("SecondaryButton"));
            secondaryActionsButton.Invoke(TestConstants.OneSecondTimeout);
            AutomationElement? descendant = BaseTest.Window?.FindFirstDescendant(elementToWaitFor.Condition);
            return descendant != null && !descendant.IsOffscreen;
        }, TestConstants.TenSecondsTimeout, ignoreException: true, interval: TestConstants.OneSecondTimeout);

        if (!retry.Success)
        {
            throw new Exception($"{retry.LastException?.Message} \n {retry.LastException?.StackTrace}");
        }

        // Remove when VPNWIN-2599 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);

        return this;
    }

    public SidebarRobot NavigateToCountriesTabAfterSearch(CountryTab tab)
    {
        SearchResultsPage.ClickTabByName(tab.GetEnumValue());
        return this;
    }

    public class Verifications : SidebarRobot
    {
        public Verifications CountriesListContains(string connectionName)
        {
            CountriesListGroup.WaitUntilDisplayed();
            List<string> allChildren = CountriesListGroup.GetAllChildrenNames();
            Assert.That(allChildren, Does.Contain(connectionName));
            return this;
        }

        public Verifications IsServerLoadInfoShown()
        {
            List<string> allChildren = GetOverlayMessageChildren();
            Assert.That(allChildren, Does.Contain(_serverLoadInfo));
            return this;
        }

        public Verifications IsProfilesInfoShown()
        {
            List<string> allChildren = GetOverlayMessageChildren();
            Assert.That(allChildren, Does.Contain(_profilesInfo));
            return this;
        }

        public Verifications IsSecureCoreInfoShown()
        {
            List<string> allChildren = GetOverlayMessageChildren();
            Assert.That(allChildren, Does.Contain(_secureCoreInfo));
            return this;
        }

        public Verifications IsP2PInfoShown()
        {
            List<string> allChildren = GetOverlayMessageChildren();
            Assert.That(allChildren, Does.Contain(_p2pInfo));
            return this;
        }

        public Verifications IsTorInfoShown()
        {
            List<string> allChildren = GetOverlayMessageChildren();
            Assert.That(allChildren, Does.Contain(_torInfo));
            return this;
        }

        public Verifications AreAllServersDisplayed()
        {
            string totalCountries = ConnectionItemsHeader.GetAutomationElementName()!;
            int totalCountriesCount = int.Parse(totalCountries.Split('(', ')')[1]);
            Assert.That(totalCountriesCount, Is.GreaterThan(MINIMUM_EXPECTED_COUNTRY_COUNT));
            CountriesListGroup.WaitUntilItemDisplayed(0);
            ConnectionItemsList.Scroll(verticalPercent: 50);
            ConnectionItemsList.Scroll(verticalPercent: 100);
            CountriesListGroup.WaitUntilItemDisplayed(-1);
            return this;
        }

        public SidebarRobot IsBackButtonInSearchBoxDisplayed()
        {
            SearchTextBox.FindChild(SearchBackButton).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsSearchBoxFocused()
        {
            SearchTextBox.AssertIsFocused();
            return this;
        }

        public Verifications SidebarSearchResultContains(string textToLookFor)
        {
            List<string> allChildren = SearchResultsPage.GetAllChildrenNames();
            Assert.That(allChildren, Does.Contain(textToLookFor));
            return this;
        }

        public Verifications IsGreenDotDisplayed(string connectionValue)
        {
            AutomationElement[] greenDotWhileConnected = Element.ByAutomationId($"Disconnect_from_{connectionValue}").GetControlType(FlaUI.Core.Definitions.ControlType.Custom);
            if (greenDotWhileConnected.Length == 0)
            {
                Assert.Fail("Green dot is not present");
            }
            return this;
        }

        public Verifications IsDisconnectButtonOnHoverDisplayed(string connectionValue)
        {
            DisconnectButtonOnHover.WaitUntilDisplayed();
            return this;
        }

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
            Element connectionOption = Element.ByAutomationId($"Actions_for_{connectionValue}");
            connectionOption.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsConnectionOptionMissing(string connectionValue)
        {
            Element connectionOption = Element.ByAutomationId($"Actions_for_{connectionValue}");
            connectionOption.DoesNotExist();
            return this;
        }

        public Verifications IsRecentsCountDisplayed(int count)
        {
            string selectorSingle = LanguageHelper.GetTranslatedString("Connections_Recents_One").Replace("({0})", $"({count})");
            string selectorPlural = LanguageHelper.GetTranslatedString("Connections_Recents_Other").Replace("({0})", $"({count})");

            string selector = count == 1 ? selectorSingle : selectorPlural;

            Element recentsLabel = Element.ByName(selector);
            recentsLabel.WaitUntilDisplayed();

            return this;
        }

        public Verifications IsPinnedCountDisplayed(int count)
        {
            string selector = LanguageHelper.GetTranslatedString("Connections_Recents_Pinned_One").Replace("({0})", $"({count})");

            Element pinnedLabel = Element.ByName(selector);
            pinnedLabel.WaitUntilDisplayed();

            return this;
        }

        public Verifications IsPinnedCountMissing()
        {
            string selector = LanguageHelper.GetTranslatedString("Connections_Recents_Pinned_One").Replace("({0})", "(1)");

            Element pinnedLabel = Element.ByName(selector);
            pinnedLabel.DoesNotExist();

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

        public Verifications IsConnectionItemMissing(string connectionItemName)
        {
            Element.ByName(connectionItemName).DoesNotExist();
            return this;
        }

        public Verifications IsCountryInfoBannerDisplayed(string description)
        {
            AutomationElement? banner = CountryInfoBanner.WaitUntilDisplayed();
            Assert.That(banner, Is.Not.Null);
            Assert.That(banner?.Name, Does.Contain(description));
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

        public Verifications IsP2PUpsellDisplayed()
        {
            P2PSidebarUpsellLabel.WaitUntilDisplayed();
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

        private List<string> GetOverlayMessageChildren()
        {
            OverlayMessage.WaitUntilDisplayed();
            return OverlayMessage.GetAllChildrenNames();
        }
    }

    public Verifications Verify => new();
}