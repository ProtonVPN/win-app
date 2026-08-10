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

using System.IO;
using System.Threading;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
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
public class GeneralSettingsTests : FreshSessionSetUp
{
    private const string SUPPORT_CENTER_WINDOW = "Proton VPN Support";

    private const string IP_ADDRESS_TO_ADD = "208.95.112.1";

    private const Country EXCLUDED_LOCATION = Country.Albania;

    private const Protocol NON_DEFAULT_PROTOCOL = Protocol.OpenVpnUdp;
    private const KillSwitchMode NON_DEFAULT_KILL_SWITCH_MODE = KillSwitchMode.Standard;
    private const NatType NON_DEFAULT_NAT_TYPE = NatType.Moderate;
    private const OpenVpnAdapter NON_DEFAULT_OPENVPN_ADAPTER = OpenVpnAdapter.TAP;
    private const SplitTunnelingMode NON_DEFAULT_SPLIT_TUNNELING_MODE = SplitTunnelingMode.Include;

    private static readonly string _connectedToastText = LanguageHelper.GetTranslatedString("SystemNotification_ConnectedTo").Replace(" {0}.", "");
    private static readonly string _disconnectedToastText = LanguageHelper.GetTranslatedString("SystemNotification_Disconnected");

    private static string ApplicationLogsPath => Path.GetDirectoryName(TestConstants.ClientLogsPath)!;
    private static string ServiceLogsPath => Path.GetDirectoryName(TestEnvironment.GetServiceLogsPath())!;

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        LanguageHelper.CurrentLanguage = Language.English;
    }

    [Test]
    [Property("TestCaseId", "609952")]
    [Retry(2)]
    public void PreferencesDontTransferAfterAccountSwitch()
    {
        SetNonDefaultSettings();

        CommonUiFlows.Logout();
        CommonUiFlows.FullLogin(TestUserData.UnlimitedUser);

        VerifyDefaultSettings();

        CommonUiFlows.Logout();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);

        VerifyNonDefaultSettings();
    }

    [Test]
    [Property("TestCaseId", "609954")]
    [Retry(2)]
    public void Notifications()
    {
        try
        {
            SettingRobot
                .OpenSettings()
                .Verify.AreNotificationsEnabled();

            HomeRobot.MinimizeClientViaMinimizeButton();

            using (TrayApp)
            {
                HomeRobot.ConnectViaConnectionCard();
            }
            DesktopRobot
                .Verify.IsToastDisplayed()
                       .DoesToastContainConnectionState(_connectedToastText);
            DesktopRobot.DismissOldToastsIfVisible();

            using (TrayApp)
            {
                HomeRobot.Disconnect();
            }
            DesktopRobot
                .Verify.IsToastDisplayed()
                       .DoesToastContainConnectionState(_disconnectedToastText);
            DesktopRobot.DismissOldToastsIfVisible();

            TrayRobot.DoubleClickTrayApp();

            SettingRobot
                .OpenSettings()
                .DisableNotificationsToggle();
            HomeRobot.MinimizeClientViaMinimizeButton();

            using (TrayApp)
            {
                HomeRobot.ConnectViaConnectionCard();
            }
            DesktopRobot.Verify.IsToastNotDisplayed();

            using (TrayApp)
            {
                HomeRobot.Disconnect();
            }
            DesktopRobot.Verify.IsToastNotDisplayed();
        }
        finally
        {
            TrayRobot.DoubleClickTrayApp();
        }
    }

    [Test]
    [Property("TestCaseId", "610992")]
    [Ignore("unskip when implementing VPNWIN-3331")]
    public void OtherLanguages()
    {
        LanguageHelper.CurrentLanguage = Language.German;

        SettingRobot
            .OpenSettings()
            .SelectLanguage(LanguageHelper.CurrentLanguage)
            .CloseSettings();

        /*TODO:
        Perform exploratory testing of the app;

        Expected
        All screens and modals are translated into the newly selected language;

        Expected
        If unsure about strings not being translated, close (exit) the app and restart. If the string is translated after restarting the app, report it.

        Note: Proton apps are localized with volunteers and some volunteers are more active than others. You might see discrepancies between localized/unlocalized strings in different languages frequently.
        */
    }

    [Test]
    [Property("TestCaseId", "611251")]
    [Ignore("unskip when implementing VPNWIN-3322")]
    public void LightTheme()
    {
        SettingRobot
            .OpenSettings()
            //.SelectTheme("Light") //verify options - default,light,dark
            .CloseSettings();

        /*TODO:
        Perform exploratory testing of the app;

        Expected
        All screens and modals are converted to Light theme;
        Text is visible on any screen/modal;
        */
    }

    [Test]
    [Property("TestCaseId", "611252")]
    [Category("5")]
    [Retry(3)]
    public void SupportCenter()
    {
        BrowserUtils.KillAllBrowsers();

        SettingRobot
            .OpenSettings()
            .ClickSupportCenterSettingsCard();

        DesktopRobot
            .Verify.IsWindowTitlePresent(SUPPORT_CENTER_WINDOW);
        //TODO: https://protonvpn.com/support
        BrowserUtils.KillAllBrowsers();
    }

    [Test]
    [Property("TestCaseId", "611253")]
    public void Logs()
    {
        try
        {

            SettingRobot
                .OpenSettings()
                .ClickDebugLogsSettingsCard()
                .ClickApplicationLogsSettingsCard();

            VerifyLogPath(ApplicationLogsPath);

            SettingRobot
                .ClickServiceLogsSettingsCard();

            VerifyLogPath(ServiceLogsPath);
        }
        finally
        {
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_W);
        }
    }

    private static void VerifyLogPath(string pathToCheck)
    {
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_L);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);

        Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_C);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);

        Assert.That(DesktopRobot.ReadClipboardText(), Does.Contain(pathToCheck));

        Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_W);
    }

    private static void SetNonDefaultSettings()
    {
        SettingRobot
            .OpenSettings()
            .OpenProtocolSettings()
            .DisableProtunToggle()
            .SelectProtocol(NON_DEFAULT_PROTOCOL)
            .ApplySettings();

        SettingRobot
            .OpenNetShieldSettings()
            .DisableNetShieldToggle()
            .ApplySettings();

        SettingRobot
            .OpenKillSwitchSettings()
            .EnableKillSwitchToggle()
            .SelectKillSwitchMode(NON_DEFAULT_KILL_SWITCH_MODE)
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
            .SelectNatType(NON_DEFAULT_NAT_TYPE)
            .SelectOpenVpnAdapter(NON_DEFAULT_OPENVPN_ADAPTER);
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
                   .IsKillSwitchEnabledStateDisplayed(NON_DEFAULT_KILL_SWITCH_MODE)
                   .IsSplitTunnelingDisabledStateDisplayed()
                   .AreNotificationsEnabled();

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

    private static void VerifyNonDefaultSettings()
    {
        SettingRobot
            .OpenSettings()
            .OpenProtocolSettings()
            .Verify.IsProTunDisabled()
                   .IsCorrectProtocolChecked(NON_DEFAULT_PROTOCOL)
            .GoBack();

        SettingRobot
            .Verify.IsNetshieldDisabledStateDisplayed()
                   .IsKillSwitchEnabledStateDisplayed(NON_DEFAULT_KILL_SWITCH_MODE)
                   .IsSplitTunnelingEnabledStateDisplayed(NON_DEFAULT_SPLIT_TUNNELING_MODE)
                   .AreNotificationsDisabled();

        SettingRobot
            .OpenConnectionPreferencesSettingsCard()
            .Verify.IsExcludedLocationDisplayed(EXCLUDED_LOCATION)
            .GoBack();

        SettingRobot
            .OpenAdvancedSettings();

        AdvancedSettingsRobot
            .Verify.IsLanDisabled()
                   .IsCorrectNatTypeChecked(NON_DEFAULT_NAT_TYPE)
                   .IsCorrectOpenVpnChecked(NON_DEFAULT_OPENVPN_ADAPTER);

        SettingRobot
            .CloseSettings();
    }
}