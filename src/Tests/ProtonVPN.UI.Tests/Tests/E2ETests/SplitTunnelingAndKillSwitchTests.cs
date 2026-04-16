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
using System.IO;
using System.Threading;
using System.Diagnostics;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Extensions;
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

    private const string ADD_FIREWALL_RULES_SCRIPT = @"
    New-NetFirewallRule -DisplayName 'Block Chrome Outbound' -Direction Outbound -Program 'C:\Program Files\Google\Chrome\Application\chrome.exe' -Action Block
    New-NetFirewallRule -DisplayName 'Block Chrome Inbound' -Direction Inbound -Program 'C:\Program Files\Google\Chrome\Application\chrome.exe' -Action Block
    ";
    private const string REMOVE_FIREWALL_RULES_SCRIPT = @"
    Remove-NetFirewallRule -DisplayName 'Block Chrome Outbound'
    Remove-NetFirewallRule -DisplayName 'Block Chrome Inbound'
    ";

    private static readonly string _installedServicePath = Path.Combine(TestEnvironment.GetProtonClientFolder(), "ProtonVPNService.exe");

    private const string VPN_QOS_POLICY_NAME = "LimitProtonVPN";

    private static readonly string _setVpnLimitScript = $"New-NetQosPolicy -Name '{VPN_QOS_POLICY_NAME}' -AppPathNameMatchCondition '{_installedServicePath}' -ThrottleRateActionBitsPerSecond 512";

    private readonly string _removeVpnLimitScript = $"Remove-NetQosPolicy -Name '{VPN_QOS_POLICY_NAME}' -Confirm:$false";

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

        ConfirmationRobot.DismissExcludedLocationsPrompt();

        //needs a 5sec wait locally
        //Thread.Sleep(TestConstants.FiveSecondsTimeout);
        NetworkUtils.AssertInternetAvailability(false);
    }

    [Test, Order(1)]
    [Retry(3)]
    public void SplitTunnelingAndAdvancedKillSwitchEnabledConnectWithDifferentProtocols()
    {
        CompletePreconditionsSplitTunnelingIp();

        MakeSureUserIsDisconnected();

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
                       .IsProtocolDisplayed(protocolToChoose);

            NetworkUtils.AssertInternetAvailability(true);
        }
    }

    [Test, Order(2)]
    public void FirewallRulesRespectedWithSplitTunnelingIncludeModeAndAdvancedKillSwitchEnabled()
    {
        //unable to test locally due to MDM
        WindowsUtils.RunPowerShellScript(ADD_FIREWALL_RULES_SCRIPT);

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

        WindowsUtils.RunPowerShellScript(REMOVE_FIREWALL_RULES_SCRIPT);
    }

    [Test, Order(4)]
    public void IncludedAppLossesInternetWhileInConnectingState()
    {
        CompletePreconditionsSplitTunnelingApp(SplitTunnelingMode.Include);

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        WindowsUtils.RunPowerShellScript(_setVpnLimitScript);

        KillVpnService();

        HomeRobot
            .Verify.IsConnecting();

        BrowserUtils.AssertBrowserInternetAvailability(APP_TO_CHECK, false);

        WindowsUtils.RunPowerShellScript(_removeVpnLimitScript);

        HomeRobot
            .Verify.IsConnected();

        BrowserUtils.AssertBrowserInternetAvailability(APP_TO_CHECK, true);
    }

    [Test, Order(5)]
    public void SplitTunnelingAndAdvancedKillSwitchEnabledBlocksInternetAfterRestart()
    {
        WindowsUtils.RunPowerShellScript(_removeVpnLimitScript);

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
        HomeRobot
            .ExpandKebabMenuButton();
        SettingRobot
            .SignOut()
            .ConfirmSignOut();

        NavigationRobot
            .Verify.IsOnLoginPage();

        Thread.Sleep(TestConstants.OneSecondTimeout);

        LoginRobot
            .Verify.IsAdvancedKillSwitchDisplayed()
            .DisableKillSwitch();

        NetworkUtils.AssertInternetAvailability(true);
    }

    private void MakeSureUserIsDisconnected()
    {
        try
        {
            HomeRobot
                .Verify.IsAdvancedKillSwitchActivated();
        }
        catch
        {
            HomeRobot
                .Disconnect()
                .Verify.IsAdvancedKillSwitchActivated();
        }
    }

    private void CompletePreconditionsKillSwitch()
    {
        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings()
            .ToggleKillSwitchSetting()
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
            .PrimaryAction();

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
            .PrimaryAction();

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
        WindowsUtils.RunPowerShellScript(_removeVpnLimitScript, true);
        WindowsUtils.RunPowerShellScript(REMOVE_FIREWALL_RULES_SCRIPT, true);
    }
}