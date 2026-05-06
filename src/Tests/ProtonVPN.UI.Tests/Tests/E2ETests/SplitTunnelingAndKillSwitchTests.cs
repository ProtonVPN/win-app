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
using System.Diagnostics;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
public class SplitTunnelingAndKillSwitchTests : FreshSessionSetUp
{
    private const string IP_ADDRESS_TO_ADD = "208.95.112.1";

    private const string APP_TO_CHECK = "Google Chrome";

    [SetUp]
    public void SetUp()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        CompletePreconditionsKillSwitch();
    }

    [Test, Order(0)]
    public void SplitTunnelingAndAdvancedKillSwitchEnabledBlockInternetConnection()
    {
        CompletePreconditionsSplitTunnelingIp();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .Disconnect()
            .Verify.IsAdvancedKillSwitchActivated();

        NetworkUtils.AssertInternetAvailability(false);
    }

    [Test, Order(1)]
    [Retry(3)]
    public void SplitTunnelingAndAdvancedKillSwitchEnabledConnectWithDifferentProtocols()
    {
        CompletePreconditionsSplitTunnelingIp();

        CommonUiFlows.EnsureUserIsDisconnected(shouldVerifyKillSwitch: true);

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        foreach (Protocol protocolToChoose in Enum.GetValues(typeof(Protocol)))
        {
            HomeRobot
                .ClickOnProtocolConnectionDetails()
                .ClickChangeProtocolButton();

            SettingRobot
                .SelectProtocol(protocolToChoose)
                .Reconnect();

            HomeRobot
                .Verify.IsConnected()
                       .IsProtocolDisplayed(protocolToChoose, TestConstants.IsProtunVersion);

            NetworkUtils.AssertInternetAvailability(true);
        }
    }

    [Test, Order(2)]
    public void FirewallRulesRespectedWithSplitTunnelingIncludeModeAndAdvancedKillSwitchEnabled()
    {
        //unable to test locally due to MDM
        ScriptHelper.AddChromeFirewallRule();

        CompletePreconditionsSplitTunnelingApp(SplitTunnelingMode.Include);

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        BrowserUtils.AssertBrowserInternetAvailability(APP_TO_CHECK, false);
        BrowserUtils.KillAllBrowsers();
    }

    [Test, Order(3)]
    public void FirewallRulesIgnoredWithSplitTunnelingExcludeModeAndAdvancedKillSwitchEnabled()
    {
        CompletePreconditionsSplitTunnelingApp(SplitTunnelingMode.Exclude);

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        BrowserUtils.AssertBrowserInternetAvailability(APP_TO_CHECK, true);
        BrowserUtils.KillAllBrowsers();

        ScriptHelper.RemoveChromeFirewallRule();
    }

    [Test, Order(4)]
    public void IncludedAppLossesInternetWhileInConnectingState()
    {
        CompletePreconditionsSplitTunnelingApp(SplitTunnelingMode.Include);

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        ScriptHelper.SetVpnSpeedLimit();

        KillVpnService();

        HomeRobot
            .Verify.IsConnecting();

        BrowserUtils.AssertBrowserInternetAvailability(APP_TO_CHECK, false);

        ScriptHelper.RemoveVpnSpeedLimit();

        HomeRobot
            .Verify.IsConnected();
        BrowserUtils.AssertBrowserInternetAvailability(APP_TO_CHECK, true);
    }

    [Test, Order(5)]
    public void SplitTunnelingAndAdvancedKillSwitchEnabledBlocksInternetAfterRestart()
    {
        ScriptHelper.RemoveVpnSpeedLimit();

        CompletePreconditionsSplitTunnelingApp(SplitTunnelingMode.Include);

        SettingRobot
            .OpenSettings()
            .OpenAutoStartupSettings()
            .ToggleAutoLaunchSetting()
            .ToggleAutoConnectionSetting()
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ExpandKebabMenuButton()
            .ExitViaKebabMenuWithConfirmation();

        Thread.Sleep(TestConstants.TwoSecondsTimeout);

        LaunchApp(isFreshStart: false);

        NavigationRobot
            .Verify.IsOnMainPage();

        //wait to see that it doesnt reconnect
        Thread.Sleep(TestConstants.TenSecondsTimeout);
        HomeRobot
            .Verify.IsAdvancedKillSwitchActivated();

        BrowserUtils.AssertBrowserInternetAvailability(APP_TO_CHECK, false);
    }

    [Test, Order(6)]
    public void TempTcDisableAdvancedKillSwitchFromSignInPage()
    {
        CommonUiFlows.Logout();

        Thread.Sleep(TestConstants.OneSecondTimeout);

        LoginRobot
            .Verify.IsAdvancedKillSwitchDisplayed()
            .DisableKillSwitch();

        NetworkUtils.AssertInternetAvailability(true);
    }

    private void CompletePreconditionsKillSwitch()
    {
        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings()
            .EnableKillSwitchToggle()
            .SelectKillSwitchMode(KillSwitchMode.Advanced)
            .ApplySettings()
            .CloseSettings();
    }

    private void CompletePreconditionsSplitTunnelingIp()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .ToggleSplitTunnelingSwitch()
            .SelectIncludeMode()
            .EditSplitTunnelingIps();

        IpSelectorRobot
            .AddIpAddress(IP_ADDRESS_TO_ADD);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .CloseSettings();
    }

    private void CompletePreconditionsSplitTunnelingApp(SplitTunnelingMode splitTunnelingMode)
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .ToggleSplitTunnelingSwitch();

        switch (splitTunnelingMode)
        {
            case SplitTunnelingMode.Include:
                SplitTunnelingRobot.SelectIncludeMode();
                break;
            case SplitTunnelingMode.Exclude:
                SplitTunnelingRobot.SelectExcludeMode();
                break;
        }

        SplitTunnelingRobot
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .AddSuggestedApp(APP_TO_CHECK)
            .Verify.IsAppChecked(APP_TO_CHECK);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .CloseSettings();
    }

    private void KillVpnService()
    {
        Thread.Sleep(TestConstants.OneSecondTimeout);

        foreach (Process process in Process.GetProcessesByName("ProtonVPNService"))
        {
            try
            {
                process.Kill(true);
            }
            catch { }
        }
        Thread.Sleep(TestConstants.OneSecondTimeout);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        //these are all backups
        DeleteProtonData();
        BrowserUtils.KillAllBrowsers();
        ScriptHelper.RemoveVpnSpeedLimit();
        ScriptHelper.RemoveChromeFirewallRule();
    }
}