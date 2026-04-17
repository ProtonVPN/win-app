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
using System.Diagnostics;
using System.Collections.Generic;
using NUnit.Framework;
using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.Extensions;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class SplitTunnelingIncludeTests : BaseTest
{
    private string? _ipAddressNotConnected = null;
    private const string IP_ADDRESS_TO_INCLUDE = "208.95.112.1";

    private const string APP_TO_INCLUDE = "Google Chrome";
    private const string OTHER_APP = "Edge";

    private const string ORIGINAL_CHROME_FOLDER = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
    private const string RENAMED_CHROME_FOLDER = @"C:\Program Files\Google\Chrome\Application\chrome_disabled.exe";

    private const string APP_NOT_FOUND_TEXT = "Application not found";
    private const string NO_APP_SELECTED_TEXT = "Select apps";
    private const string SPLIT_TUNNELING_MODE = "Included apps (1)"; //Excluded apps (1)


    [OneTimeSetUp]
    public void SetUp()
    {
        _ipAddressNotConnected = NetworkUtils.GetIpAddressWithRetry();
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    public void SplitTunnelingIncludeIpAddress()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .ToggleSplitTunnelingSwitch()
            .SelectIncludeMode()
            .EditSplitTunnelingIps();

        IpSelectorRobot
            .Verify.IsIpSelectorOpened()
            .AddIpAddress(IP_ADDRESS_TO_INCLUDE)
            .Verify.WasIpAdded(IP_ADDRESS_TO_INCLUDE);
        ConfirmationRobot
            .PrimaryAction();

        SettingRobot
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        NetworkUtils.VerifyIpAddressDoesNotMatchWithRetry(_ipAddressNotConnected);
    }

    [Test, Order(1)]
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
            .PrimaryAction();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    [Test, Order(2)]
    public void SplitTunnelingIncludeModeApp()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .Verify.AssertAppAvailability(APP_TO_INCLUDE, true)
            .AddSuggestedApp(APP_TO_INCLUDE)
            .Verify.IsAppChecked(APP_TO_INCLUDE);
        ConfirmationRobot
            .PrimaryAction();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();
        string? ipAddressToCompare = HomeRobot.GetVpnServerIp();

        BrowserUtils.VerifyBrowserIpWithRetry(APP_TO_INCLUDE, true, ipAddressToCompare);
        BrowserUtils.VerifyBrowserIpWithRetry(OTHER_APP, false, ipAddressToCompare);
        BrowserUtils.KillAllBrowsers();

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();

        ConfirmationRobot.DismissExcludedLocationsPrompt();

        BrowserUtils.VerifyBrowserIpWithRetry(APP_TO_INCLUDE, false, ipAddressToCompare);
        BrowserUtils.VerifyBrowserIpWithRetry(OTHER_APP, false, ipAddressToCompare);
    }

    [Test, Order(3)]
    public void SplitTunnelingWithUninstalledApp()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .Verify.IsAppChecked(APP_TO_INCLUDE)
                   .AssertAppAvailability(APP_TO_INCLUDE, true);
        ConfirmationRobot
            .CancelAction();

        SettingRobot
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .HoverOverSplitTunnelingFlyoutWidget();
        IsSplitTunnelingAppInFlyoutMenu(true, SPLIT_TUNNELING_MODE);

        HomeRobot
            .ClickOnConnectionCardTitle();

        RenameChrome();
        Thread.Sleep(1_000);

        HomeRobot
            .HoverOverSplitTunnelingFlyoutWidget();
        IsSplitTunnelingAppInFlyoutMenu(false, SPLIT_TUNNELING_MODE);

        Thread.Sleep(1_000);
        //it glitches after the hover, so clicking the sidebar just in case
        SidebarRobot
            .ClickOnSidebar();

        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .Verify.AssertAppAvailability(APP_TO_INCLUDE, false)
                   .AssertAppAvailability(APP_NOT_FOUND_TEXT, true);
    }

    private void IsSplitTunnelingAppInFlyoutMenu(bool isAppAvailable, string splitTunnelingMode)
    {
        Thread.Sleep(TestConstants.OneSecondTimeout);
        string appName = isAppAvailable ? APP_TO_INCLUDE : APP_NOT_FOUND_TEXT;

        List<string> allChildren = Element.ByAutomationId("WidgetFlyout").GetAllChildrenNames();
        Assert.That(allChildren, isAppAvailable ? Does.Contain(splitTunnelingMode) : Does.Contain(splitTunnelingMode.Replace("1", "0")));
        Assert.That(allChildren, isAppAvailable ? Does.Not.Contain(NO_APP_SELECTED_TEXT) : Does.Contain(NO_APP_SELECTED_TEXT));

        SplitTunnelingRobot
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .Verify.AssertAppAvailability(appName, true);
        ConfirmationRobot
            .CancelAction();
    }

    private static void RenameChrome()
    {
        foreach (Process process in Process.GetProcessesByName("chrome"))
        {
            process.Kill();
            process.WaitForExit();
        }

        if (File.Exists(ORIGINAL_CHROME_FOLDER))
        {
            File.Move(ORIGINAL_CHROME_FOLDER, RENAMED_CHROME_FOLDER);
        }
    }

    private static void RestoreChrome()
    {
        if (File.Exists(RENAMED_CHROME_FOLDER))
        {
            File.Move(RENAMED_CHROME_FOLDER, ORIGINAL_CHROME_FOLDER);
        }
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        BrowserUtils.KillAllBrowsers();
        RestoreChrome();
        Cleanup();
    }
}