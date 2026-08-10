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

using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("4")]
public class MiscTests : FreshSessionSetUp
{
    private const string IP_ADDRESS_TO_ADD = "208.95.112.1";
    private const Country EXCLUDED_LOCATION = Country.Albania;
    private const Country COUNTRY_NAME = Country.Austria;

    private const string RESET_PASSWORD_WINDOW = "Reset your Proton Account password";
    private const string RESET_USERNAME_WINDOW = "Find your Proton Account username";
    private const string CREATE_ACCOUNT_WINDOW = "Proton VPN: Sign-up";

    private const string VPN_ACCELERATOR_ON = "\"split-tcp\": true";
    private const string VPN_ACCELERATOR_OFF = "\"split-tcp\": false";
    private const string LINE_TO_LOOK_FOR = "split-tcp";
    private static readonly string _serviceLogsPath = TestEnvironment.GetServiceLogsPath();

    private static readonly string _restoreDefaultSettingsTitle = LanguageHelper.GetTranslatedString("Settings_RestoreDefault_Confirmation_Title");
    private static readonly string _restoreDefaultSettingsPrimaryButton = LanguageHelper.GetTranslatedString("Settings_RestoreDefaultAndReconnect_Confirmation_Action");
    private static readonly string _cancelButton = LanguageHelper.GetTranslatedString("Common_Actions_Cancel");
    private static readonly string _splitTunnelingMode = LanguageHelper.GetTranslatedString("Settings_Connection_SplitTunneling_Apps_Included_FormattedHeader").Replace("({0})", "(1)");

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        LanguageHelper.CurrentLanguage = Language.English;
    }

    [Test]
    [Property("TestCaseId", "602426")]
    public void HomeScreenLocalizationAfterLanguageChange()
    {
        LanguageHelper.CurrentLanguage = Language.French;

        string fastestCountryConnectionCardTitle = LanguageHelper.GetTranslatedString("Country_Fastest");
        string fastestCountryConnectionCardDescription = LanguageHelper.GetTranslatedString("Settings_Connection_Default_Fastest_Description");
        string connectionPreferecesDropdownOption = LanguageHelper.GetTranslatedString("Settings_Connection_Default_Random");
        string connectButton = LanguageHelper.GetTranslatedString("Common_Actions_Connect");

        string unprotectedStatus = LanguageHelper.GetTranslatedString("Home_ConnectionDetails_Unprotected");
        string yourIpText = LanguageHelper.GetTranslatedString("Home_ConnectionDetails_YourIpAddress");
        string yourCountryText = LanguageHelper.GetTranslatedString("Home_ConnectionDetails_Country");
        string yourProviderText = LanguageHelper.GetTranslatedString("Home_ConnectionDetails_Isp");

        string netShieldFeatureName = LanguageHelper.GetTranslatedString("Settings_Connection_NetShield");
        string killSwitchFeatureName = LanguageHelper.GetTranslatedString("Settings_Connection_KillSwitch");
        string portForwardingFeatureName = LanguageHelper.GetTranslatedString("Settings_Connection_PortForwarding");
        string splitTunnelingFeatureName = LanguageHelper.GetTranslatedString("Settings_Connection_SplitTunneling");

        Country countryNameOne = Country.Belgium;
        string cityName = ApiTranslationHelper.GetTranslatedString("City_Brussels");
        Country countryNameTwo = Country.UnitedStates;
        string stateName = ApiTranslationHelper.GetTranslatedString("State_California");

        SettingRobot
            .OpenSettings()
            .SelectLanguage(LanguageHelper.CurrentLanguage)
            .CloseSettings();

        HomeRobot
            .Verify.ConnectionCardTitleEquals(fastestCountryConnectionCardTitle)
                   .ConnectionCardDescriptionContains(fastestCountryConnectionCardDescription)
                   .ConnectionPreferencesDropdownContains([connectionPreferecesDropdownOption])
                   .ConnectionCardConnectButtonEquals(connectButton)
                   .ProtectionStatusEquals(unprotectedStatus)
                   .LocationDetailsContains(yourIpText)
                   .LocationDetailsContains(yourCountryText)
                   .LocationDetailsContains(yourProviderText);
        //TODO: Map pin tooltip assert: "Connect - [Country]"

        FeaturesRobot
            .Verify.NetShieldFeatureNameEquals(netShieldFeatureName)
                   .KillSwitchFeatureNameEquals(killSwitchFeatureName)
                   .PortForwardingFeatureNameEquals(portForwardingFeatureName)
                   .SplitTunnelingFeatureNameEquals(splitTunnelingFeatureName);

        SidebarRobot
            .NavigateToAllCountriesTab()
            .ExpandCities(countryNameOne)
            .Verify.CountriesListContains(countryNameOne.GetName())
                   .CountriesListContains(cityName)
            .SearchFor(countryNameTwo.GetName())
            .ExpandCities(countryNameTwo)
            .Verify.SidebarSearchResultContains(stateName);
    }

    [Test]
    [Property("TestCaseId", "602427")]
    [Category("5")]
    [Retry(2)]
    public void ToggleFeaturesFromFlyout()
    {
        FeaturesRobot
            .HoverOverKillSwitchWidget()
            .HoverOverNetShieldWidget()
            .EnableNetShield(NetShieldMode.BlockMalwareOnly)
            .EnableNetShield(NetShieldMode.BlockAdsMalwareTrackers)
            .EnableNetShield(NetShieldMode.BlockAdsMalwareTrackersAdultContent)
            .DisableFeature();

        FeaturesRobot
            .HoverOverKillSwitchWidget()
            .EnableKillSwitch(KillSwitchMode.Advanced)
            .EnableKillSwitch(KillSwitchMode.Standard)
            .DisableFeature();

        FeaturesRobot
            .HoverOverPortForwardingWidget()
            .EnableFeature()
            .DisableFeature();

        FeaturesRobot
            .HoverOverSplitTunnelingWidget()
            .EnableSplitTunneling(SplitTunnelingMode.Include)
            .EnableSplitTunneling(SplitTunnelingMode.Exclude)
            .DisableFeature();

        HomeRobot.ClickOnConnectionCardTitle();
    }

    [Test]
    [Property("TestCaseId", "602428")]
    [Category("5")]
    public void FlyoutDisplaysFeatureStatusWhileConnected()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        FeaturesRobot
            .HoverOverNetShieldWidget()
            .EnableNetShield(NetShieldMode.BlockAdsMalwareTrackersAdultContent)
            .Verify.IsNetShieldTextInFlyoutMenu();

        FeaturesRobot
            .HoverOverKillSwitchWidget()
            .EnableKillSwitch(KillSwitchMode.Advanced)
            .Verify.IsAdvancedKillSwitchTextInFlyoutMenu(isConnected: true)
            .DisableFeature();

        FeaturesRobot
            .HoverOverPortForwardingWidget()
            .EnableFeature()
            .Verify.IsPortForwardingEnabled();

        FeaturesRobot
            .HoverOverSplitTunnelingWidget()
            .EnableSplitTunneling(SplitTunnelingMode.Include);

        ConfirmationRobot.PrimaryAction();
        Thread.Sleep(TestConstants.OneSecondTimeout);
        HomeRobot.Verify.IsConnected();

        FeaturesRobot
            .HoverOverSplitTunnelingWidget()
            .Verify.IsSplitTunnelingAppUnavailableInFlyoutMenu(_splitTunnelingMode)
            .DisableFeature();

        ConfirmationRobot.PrimaryAction();
        Thread.Sleep(TestConstants.OneSecondTimeout);
        HomeRobot.Verify.IsConnected();

        HomeRobot.ClickOnConnectionCardTitle();
    }

    [Test]
    [Property("TestCaseId", "602446")]
    public void VpnAccelerator()
    {
        VerifyVpnAccelerator(SimpleToggle.On, VPN_ACCELERATOR_ON);

        VerifyVpnAccelerator(SimpleToggle.Off, VPN_ACCELERATOR_OFF);
    }

    [Test]
    [Property("TestCaseId", "609956")]
    public void InformationLegendShowsCorrectInfo()
    {
        SidebarRobot
            .NavigateToAllCountriesTab()
            .ExpandCities(COUNTRY_NAME)
            .ExpandSpecificServerList()
            .ClickServerLoadInfoButton()
            .Verify.IsServerLoadInfoShown()
            .CloseTabInfoModal();

        SidebarRobot
            .NavigateToSecureCoreCountriesTab()
            .ClickTabInfoButton()
            .Verify.IsSecureCoreInfoShown()
            .CloseTabInfoModal();

        SidebarRobot
            .NavigateToP2PCountriesTab()
            .ClickTabInfoButton()
            .Verify.IsP2PInfoShown()
            .CloseTabInfoModal();

        SidebarRobot
            .NavigateToTorCountriesTab()
            .ClickTabInfoButton()
            .Verify.IsTorInfoShown()
            .CloseTabInfoModal();

        SidebarRobot
            .NavigateToProfiles()
            .ClickTabInfoButton()
            .Verify.IsProfilesInfoShown()
            .CloseTabInfoModal();
    }

    [Test]
    [Property("TestCaseId", "202")]
    [Category("5")]
    [Retry(3)]
    public void ForgotPassword()
    {
        BrowserUtils.KillAllBrowsers();

        CommonUiFlows.Logout();

        LoginRobot
            .NavigateToForgotPassword();

        DesktopRobot
            .Verify.IsWindowTitlePresent(RESET_PASSWORD_WINDOW);
        //TODO: https://account.protonvpn.com/reset-password 
        BrowserUtils.KillAllBrowsers();
    }

    [Test]
    [Property("TestCaseId", "203")]
    [Category("5")]
    [Retry(3)]
    public void ForgotUsername()
    {
        BrowserUtils.KillAllBrowsers();

        CommonUiFlows.Logout();

        LoginRobot
            .NavigateToForgotUsername();

        DesktopRobot
            .Verify.IsWindowTitlePresent(RESET_USERNAME_WINDOW);
        //TODO: https://account.protonvpn.com/forgot-username 
        BrowserUtils.KillAllBrowsers();
    }

    [Test]
    [Property("TestCaseId", "602333")]
    [Category("5")]
    [Retry(3)]
    public void CreateAccount()
    {
        BrowserUtils.KillAllBrowsers();

        CommonUiFlows.Logout();

        LoginRobot
            .ClickCreateAccountButton();

        DesktopRobot
            .Verify.IsWindowTitlePresent(CREATE_ACCOUNT_WINDOW);
        //TODO: https://account.protonvpn.com/signup?ref=windows 
        //Note: it's important that ?ref=windows is added at the end of the URL when performing the test from Windows client;
        BrowserUtils.KillAllBrowsers();
    }

    [Test]
    [Property("TestCaseId", "760744")]
    [Retry(3)]
    public void RestoreDefaultSettings()
    {
        RandomizeSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        SettingRobot
            .OpenSettings()
            .ClickRestoreDefaultSettings();

        ConfirmationRobot
            .Verify.IsOverlayDisplayed()
            .OverlayTextContains(_restoreDefaultSettingsTitle)
            .OverlayButtonsEquals(
                primary: _restoreDefaultSettingsPrimaryButton,
                cancel: _cancelButton)
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        Thread.Sleep(TestConstants.OneSecondTimeout);
        HomeRobot
            .Verify.IsConnected();

        VerifyDefaultSettings();

        HomeRobot
            .Verify.IsConnected();
    }

    private static void RandomizeSettings()
    {
        SettingRobot
            .OpenSettings()
            .OpenProtocolSettings()
            .DisableProtunToggle()
            .SelectProtocol(Protocol.OpenVpnUdp)
            .ApplySettings();

        SettingRobot
            .OpenNetShieldSettings()
            .DisableNetShieldToggle()
            .ApplySettings();

        SettingRobot
            .OpenKillSwitchSettings()
            .EnableKillSwitchToggle()
            .SelectKillSwitchMode(KillSwitchMode.Standard)
            .ApplySettings();

        SettingRobot
            .OpenSplitTunnelingSettings();
        SplitTunnelingRobot
            .EnableSplitTunnelingToggle()
            .SelectIncludeMode()
            .EditSplitTunnelingIps();
        IpSelectorRobot
            .AddIpAddress(IP_ADDRESS_TO_ADD);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();
        SettingRobot
            .ApplySettings();

        SettingRobot
            .OpenConnectionPreferencesSettingsCard()
            .OpenExcludedLocationsSelector()
            .SelectExcludedCountry(EXCLUDED_LOCATION)
            .Verify.IsRemoveExcludedLocationButtonDisplayed()
            .Verify.IsExcludedLocationDisplayed(EXCLUDED_LOCATION)
            .ApplySettings();

        SettingRobot
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .DisableLanToggle()
            .SelectNatType(NatType.Moderate)
            .SelectOpenVpnAdapter(OpenVpnAdapter.TAP);
        SettingRobot
            .ApplySettings()
            .DisableNotificationsToggle()
            .CloseSettings();
    }

    private static void VerifyDefaultSettings()
    {
        SettingRobot
            .OpenSettings()
            .OpenProtocolSettings()
            .Verify.IsProTunEnabled()
                   .IsCorrectProtocolChecked(Protocol.Smart)
            .GoBack();

        SettingRobot
            .Verify.IsNetshieldEnabledStateDisplayed()
                   .IsKillSwitchDisabledStateDisplayed()
                   .IsSplitTunnelingDisabledStateDisplayed()
                   .AreNotificationsDisabled();

        SettingRobot
            .OpenConnectionPreferencesSettingsCard()
            .Verify.IsExcludedLocationNotDisplayed(EXCLUDED_LOCATION)
            .GoBack();

        SettingRobot
            .OpenAdvancedSettings();

        AdvancedSettingsRobot
            .Verify.IsLanEnabled()
                   .IsCorrectNatTypeChecked(NatType.Strict)
                   .IsCorrectOpenVpnChecked(OpenVpnAdapter.TUN);

        SettingRobot
            .CloseSettings();
    }

    private static void VerifyVpnAccelerator(SimpleToggle vpnAcceleratorMode, string wordToLookFor)
    {
        SettingRobot
            .OpenSettings()
            .OpenVpnAcceleratorSettings();

        if (vpnAcceleratorMode == SimpleToggle.Off)
        {
            SettingRobot
                .DisableVpnAcceleratorToggle()
                .ApplySettings();
        }
        else
        {
            SettingRobot
                .Verify.IsVpnAcceleratorEnabled();
        }

        SettingRobot
            .CloseSettings();

        HomeRobot.ClickOnConnectionCardTitle();
        CommonUiFlows.EnsureUserIsDisconnected();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        //give it time to populate the service-logs after connecting
        Thread.Sleep(TestConstants.OneSecondTimeout);

        WindowsUtils.AssertLogFile(_serviceLogsPath, LINE_TO_LOOK_FOR, wordToLookFor);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        ScriptHelper.RestoreInternet();
    }
}