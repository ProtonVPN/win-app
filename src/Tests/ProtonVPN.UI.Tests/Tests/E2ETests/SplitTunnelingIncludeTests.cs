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
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Enums;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
[Category("SMOKE_3")]
public class SplitTunnelingIncludeTests : BaseTest
{
    private string? _ipAddressNotConnected = null;
    private const string IP_ADDRESS_TO_INCLUDE = "208.95.112.1";

    private const Browser APP_TO_INCLUDE = Browser.GoogleChrome;
    private const Browser OTHER_APP = Browser.Edge;

    private static readonly string _appNotFoundText = LanguageHelper.GetTranslatedString("Common_Message_AppNotFound");
    private static readonly string _splitTunnelingMode = LanguageHelper.GetTranslatedString("Settings_Connection_SplitTunneling_Apps_Included_FormattedHeader").Replace("({0})", "(1)");

    [OneTimeSetUp]
    public void SetUp()
    {
        WindowsUtils.RestoreChrome();
        LaunchClient();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        NetworkUtils.AssertInternetAvailability(true);
        _ipAddressNotConnected = NetworkUtils.GetIpAddressWithRetry();
    }

    [Test, Order(0)]
    [Property("TestCaseId", "602416")]
    public void SplitTunnelingIncludeIpAddress()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EnableSplitTunnelingToggle()
            .SelectIncludeMode()
            .EditSplitTunnelingIps();

        IpSelectorRobot
            .Verify.IsIpSelectorOpened()
            .AddIpAddress(IP_ADDRESS_TO_INCLUDE)
            .Verify.WasIpAdded(IP_ADDRESS_TO_INCLUDE);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        NetworkUtils.VerifyIpAddressDoesNotMatchWithRetry(_ipAddressNotConnected);
    }

    [Test, Order(1)]
    [Property("TestCaseId", "602415")]
    public void SplitTunnelingDisableIpAddress()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EditSplitTunnelingIps();
        IpSelectorRobot
            .Verify.IsIpSelectorOpened()
            .TickIpAddressCheckBox(IP_ADDRESS_TO_INCLUDE);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    [Test, Order(2)]
    [Property("TestCaseId", "787610")]
    public void SplitTunnelingIncludeModeApp()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .Verify.AssertAppAvailability(APP_TO_INCLUDE, shouldBeAvailable: true)
            .AddSuggestedApp(APP_TO_INCLUDE)
            .Verify.IsAppChecked(APP_TO_INCLUDE);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();

        string? ipAddressToCompare = HomeRobot.GetVpnServerIp();

        BrowserUtils.VerifyBrowserIpWithRetry(APP_TO_INCLUDE, hasVpn: true, ipAddressToCompare);
        BrowserUtils.VerifyBrowserIpWithRetry(OTHER_APP, hasVpn: false, ipAddressToCompare);
        BrowserUtils.KillAllBrowsers();

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();

        BrowserUtils.VerifyBrowserIpWithRetry(APP_TO_INCLUDE, hasVpn: false, ipAddressToCompare);
        BrowserUtils.VerifyBrowserIpWithRetry(OTHER_APP, hasVpn: false, ipAddressToCompare);
    }

    [Test, Order(3)]
    [Property("TestCaseId", "724450")]
    public void SplitTunnelingWithUninstalledApp()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .Verify.IsAppChecked(APP_TO_INCLUDE)
                   .AssertAppAvailability(APP_TO_INCLUDE, shouldBeAvailable: true);
        ConfirmationRobot
            .CancelAction();

        SettingRobot
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();
        FeaturesRobot
            .HoverOverSplitTunnelingWidget();
        VerifyIsSplitTunnelingAppInFlyoutMenu(isAppAvailable: true);

        HomeRobot
            .ClickOnConnectionCardTitle();

        BrowserUtils.KillAllBrowsers();
        WindowsUtils.RenameChrome();
        Thread.Sleep(TestConstants.OneSecondTimeout);

        FeaturesRobot
            .HoverOverSplitTunnelingWidget();
        VerifyIsSplitTunnelingAppInFlyoutMenu(isAppAvailable: false);

        Thread.Sleep(TestConstants.OneSecondTimeout);
        //it glitches after the hover, so clicking the sidebar just in case
        SidebarRobot
            .ClickOnSidebar();

        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .Verify.AssertAppAvailability(APP_TO_INCLUDE, shouldBeAvailable: false)
                   .AssertAppAvailability(_appNotFoundText, shouldBeAvailable: true);
    }

    private static void VerifyIsSplitTunnelingAppInFlyoutMenu(bool isAppAvailable)
    {
        string appName = isAppAvailable ? APP_TO_INCLUDE.GetEnumValue() : _appNotFoundText;

        if (isAppAvailable)
        {
            FeaturesRobot
                .Verify.IsSplitTunnelingAppAvailableInFlyoutMenu(_splitTunnelingMode);
        }
        else
        {
            FeaturesRobot
                .Verify.IsSplitTunnelingAppUnavailableInFlyoutMenu(_splitTunnelingMode);
        }

        SplitTunnelingRobot
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .Verify.AssertAppAvailability(appName, shouldBeAvailable: true);
        ConfirmationRobot
            .CancelAction();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        BrowserUtils.KillAllBrowsers();
        WindowsUtils.RestoreChrome();
        Cleanup();
    }
}