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
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
public class SplitTunnelingAndKillSwitchTests : BaseTest
{
    private string? _ipAddressNotConnected = null;
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

    [OneTimeSetUp]
    public void SetUp()
    {
        _ipAddressNotConnected = NetworkUtils.GetIpAddressWithRetry();
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    public void SplitTunnelingAndAdvancedKillSwitchEnabledBlockInternetConnection()
    {
        CompletePrecondtions();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        NetworkUtils.VerifyIpAddressDoesNotMatchWithRetry(_ipAddressNotConnected);

        HomeRobot
            .Disconnect()
            .Verify.IsAdvancedKillSwitchActivated();

        //needs a 5sec wait locally
        //Thread.Sleep(TestConstants.FiveSecondsTimeout);
        NetworkUtils.AssertInternetAvailability(false);
    }

    [Test, Order(1)]
    [CancelAfter(180000)]
    public void SplitTunnelingAndAdvancedKillSwitchEnabledConnectWithDifferentProtocols()
    {
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

            NetworkUtils.VerifyIpAddressDoesNotMatchWithRetry(_ipAddressNotConnected);
            NetworkUtils.AssertInternetAvailability(true);
        }
    }

    [Test, Order(2)]
    public void FirewallRulesRespectedWithSplitTunnelingIncludeModeAndAdvancedKillSwitchEnabled()
    {
        //unable to test locally due to MDM
        WindowsUtils.RunPowerShellScript(ADD_FIREWALL_RULES_SCRIPT);

        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EditSplitTunnelingIps();
        IpSelectorRobot
            .DeleteAllIps();
        ConfirmationRobot
            .PrimaryAction();

        SplitTunnelingRobot
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .AddSuggestedApp(APP_TO_CHECK);
        ConfirmationRobot
            .PrimaryAction();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();

        BrowserUtils.AssertBrowserInternetAvailability(APP_TO_CHECK, false);
        BrowserUtils.KillAllBrowsers();
    }

    [Test, Order(3)]
    public void FirewallRulesIgnoredWithSplitTunnelingExcludeModeAndAdvancedKillSwitchEnabled()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .SelectExcludeMode()
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .AddSuggestedApp(APP_TO_CHECK);
        ConfirmationRobot
            .PrimaryAction();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();

        BrowserUtils.AssertBrowserInternetAvailability(APP_TO_CHECK, true);
        BrowserUtils.KillAllBrowsers();

        WindowsUtils.RunPowerShellScript(REMOVE_FIREWALL_RULES_SCRIPT);
    }

    [Test, Order(4)]
    public void IncludedAppLossesInternetWhileInConnectingState()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .SelectIncludeMode()
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .Verify.IsAppChecked(APP_TO_CHECK);
        ConfirmationRobot
            .CancelAction();

        SettingRobot
            .Reconnect();

        HomeRobot
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

        SettingRobot
            .OpenSettings()
            .OpenAutoStartupSettings()
            .ToggleAutoLaunchSetting()
            .ToggleAutoConnectionSetting()
            .ApplySettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .Verify.IsAppChecked(APP_TO_CHECK);
        ConfirmationRobot
            .CancelAction();

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
        Thread.Sleep(TestConstants.OneSecondTimeout);
        HomeRobot.ExpandKebabMenuButton();
        SettingRobot.SignOut()
            .ConfirmSignOut();

        NavigationRobot
            .Verify.IsOnLoginPage();

        Thread.Sleep(TestConstants.OneSecondTimeout);

        LoginRobot
            .Verify.IsAdvancedKillSwitchDisplayed()
            .DisableKillSwitch();

        NetworkUtils.AssertInternetAvailability(true);
    }

    private void CompletePrecondtions()
    {
        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings()
            .ToggleKillSwitchSetting()
            .SelectKillSwitchMode(KillSwitchMode.Advanced)
            .ApplySettings()
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
        BrowserUtils.KillAllBrowsers();
        WindowsUtils.RunPowerShellScript(_removeVpnLimitScript);
        WindowsUtils.RunPowerShellScript(REMOVE_FIREWALL_RULES_SCRIPT);
        Cleanup();
    }
}