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
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class ConnectionTests : FreshSessionSetUp
{
    private const string FAST_CONNECTION = "Fastest country";

    private const string COUNTRY_NAME_ONE = "Angola";
    private const string COUNTRY_NAME_TWO = "Austria";
    private const string CITY_NAME_ONE = "Vienna";
    private const string SLOW_TOR_COUNTRY = "United States";

    private const string APP_TO_CHECK = "Google Chrome";

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Category("ARM")]
    public void QuickConnectToServerAndDisconnect()
    {
        CommonUiFlows.EnsureUserIsDisconnected();

        string ipAddressNotConnected = NetworkUtils.GetIpAddressWithRetry();

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        HomeRobot
            .Verify.IsDisconnected()
                   .ConnectionCardTitleEquals(FAST_CONNECTION)
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
                   .ConnectionCardTitleEquals(FAST_CONNECTION);

        string ipAddressConnected = NetworkUtils.GetIpAddressWithRetry();

        HomeRobot
            .Verify.AssertVpnConnectionEstablished(ipAddressNotConnected, ipAddressConnected);

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();

        NavigationRobot
            .Verify.IsOnLocationDetailsPage();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(ipAddressNotConnected);
    }

    [Test]
    [Retry(3)]
    [Category("ARM")]
    public void ConnectAndCancel()
    {
        CommonUiFlows.EnsureUserIsDisconnected();

        SidebarRobot
            .NavigateToTorCountriesTab()
            .ConnectToCountry(SLOW_TOR_COUNTRY);
        HomeRobot
            .Verify.IsConnecting()
            .CancelConnection(TestConstants.MoreFrequentRetryInterval)
            .Verify.IsDisconnected();
    }

    [Test]
    public void LocalNetworkingIsReachableWhileConnected()
    {
        SettingRobot
            .OpenSettings()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .Verify.IsLanEnabled();
        SettingRobot
            .CloseSettings();

        HomeRobot
            .Verify.IsDisconnected()
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        NetworkUtils.VerifyLocalNetworking(isLanEnabled: true);

        SettingRobot
            .OpenSettings()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .DisableLanToggle();
        SettingRobot
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .Verify.IsConnected();

        NetworkUtils.VerifyLocalNetworking(isLanEnabled: false);
    }

    [Test]
    public void AutoConnectionOn()
    {
        SettingRobot
            .OpenSettings()
            .OpenAutoStartupSettings()
            .Verify.IsAutoConnectEnabled()
            .ToggleAutoLaunchSetting()
            .ApplySettings();

        App?.Close();
        App?.Dispose();

        LaunchApp(isFreshStart: false);

        NavigationRobot
            .Verify.IsOnMainPage();

        HomeRobot
            .Verify.IsConnected();
    }

    [Test]
    public void AutoConnectionOff()
    {
        SettingRobot
            .OpenSettings()
            .OpenAutoStartupSettings()
            .Verify.IsAutoConnectEnabled()
            .ToggleAutoLaunchSetting()
            .ToggleAutoConnectionSetting()
            .ApplySettings();

        App?.Close();
        App?.Dispose();

        LaunchApp(isFreshStart: false);

        NavigationRobot
            .Verify.IsOnMainPage();

        //wait to see that it doesnt reconnect
        Thread.Sleep(TestConstants.TenSecondsTimeout);
        HomeRobot
            .Verify.IsDisconnected();
    }

    [Test]
    public void ClientKillDoesNotStopVpnConnection()
    {
        SettingRobot
            .OpenSettings()
            .OpenAutoStartupSettings()
            .ToggleAutoLaunchSetting()
            .ToggleAutoConnectionSetting()
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        string ipAddressBeforeClientKill = NetworkUtils.GetIpAddressWithRetry();

        // Allow some time for the app to settle down to imitate user's delay
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        App?.Kill();
        // Delay to make sure that connection is not lost even after brief delay.
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        string ipAddressAfterClientKill = NetworkUtils.GetIpAddressWithRetry();

        HomeRobot.Verify.AssertVpnConnectionAfterKill(ipAddressBeforeClientKill, ipAddressAfterClientKill);

        LaunchApp(isFreshStart: false);
        HomeRobot.Verify.IsConnected();

        string ipAddressAfterClientIsRestored = NetworkUtils.GetIpAddressWithRetry();
        HomeRobot.Verify.AssertVpnConnectionAfterRestored(ipAddressBeforeClientKill, ipAddressAfterClientIsRestored);
    }

    [Test]
    public void AppExitStopsVpnConnection()
    {
        string ipAddressBeforeConnected = NetworkUtils.GetIpAddressWithRetry();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .ExpandKebabMenuButton()
            .ExitViaKebabMenuWithConfirmation();

        // Delay to make sure that connection is not lost even after brief delay.
        Thread.Sleep(TestConstants.FiveSecondsTimeout);
        NetworkUtils.VerifyIpAddressMatchesWithRetry(ipAddressBeforeConnected);
    }

    [Test]
    public void AppWindowCloseDoesNotStopVpnConnection()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .CloseClientViaCloseButton();

        string ipAddressAfterConnected = NetworkUtils.GetIpAddressWithRetry();

        // Delay to make sure that connection is not lost even after brief delay.
        Thread.Sleep(TestConstants.FiveSecondsTimeout);
        NetworkUtils.VerifyIpAddressMatchesWithRetry(ipAddressAfterConnected);

        TrayRobot.DoubleClickTrayApp();
    }

    [Test]
    public void ConnectToServerFromCountriesList()
    {
        SidebarRobot
            .NavigateToAllCountriesTab();
        ConnectToCountryAndVerify(COUNTRY_NAME_ONE);

        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(COUNTRY_NAME_ONE);

        SidebarRobot
            .ExpandCities(COUNTRY_NAME_TWO)
            .ConnectToCity(CITY_NAME_ONE);
        HomeRobot
            .Verify.IsConnected();

        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(COUNTRY_NAME_TWO);

        SidebarRobot
            .ExpandSpecificServerList()
            .ConnectToServer();
        HomeRobot
            .Verify.IsConnected();

        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(COUNTRY_NAME_TWO);
    }

    [Test]
    public void DisconnectFromCountriesList()
    {
        string ipAddressNotConnected = NetworkUtils.GetIpAddressWithRetry();

        SidebarRobot
            .NavigateToAllCountriesTab();

        ConnectToCountryAndVerify();
        SidebarRobot
            .DisconnectViaCountry(COUNTRY_NAME_TWO);
        HomeRobot
            .Verify.IsDisconnected();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(ipAddressNotConnected);

        ConnectToCountryAndVerify();
        SidebarRobot
            .ExpandCities(COUNTRY_NAME_TWO)
            .DisconnectViaCity(CITY_NAME_ONE);
        HomeRobot
            .Verify.IsDisconnected();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(ipAddressNotConnected);

        ConnectToCountryAndVerify();
        SidebarRobot
            .ExpandSpecificServerList()
            .DisconnectViaServer();
        HomeRobot
            .Verify.IsDisconnected();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(ipAddressNotConnected);
    }

    [Test]
    public void CorrectIpIsShown()
    {
        HomeRobot
            .Verify.IsDisconnected()
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        string ipAddressConnected = NetworkUtils.GetIpAddressWithRetry();
        string vpnServerIp = HomeRobot.GetVpnServerIp()!;

        HomeRobot
            .Verify.AssertVPNIpAndExternalIpMatch(vpnServerIp, ipAddressConnected)
            .Disconnect()
            .Verify.IsDisconnected();
    }

    [Test]
    [Ignore("Native WireGuard causes infinite connecting on the ProTUN build")]
    public void FreshSignInWhileConnectedToWireGuard()
    {
        LoginFreshWithWireGuardOn();

        HomeRobot
            .Verify.IsLocationDetailsPanelEmpty();
        //TODO: There is no (red) pin on the map displaying user's current location;

        ScriptHelper.DisconnectFromWireGuard();

        HomeRobot
            .Verify.AreLocationDetailsShown();
        //TODO: A(red) pin on the map displays user's current country;

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();
    }

    [Test]
    public void FirewallRulesAreNotIgnored()
    {
        ScriptHelper.AddChromeFirewallRule();

        try
        {
            EnableKillSwitch(KillSwitchMode.Advanced);
            HomeRobot
                .ConnectViaConnectionCard()
                .Verify.IsConnected();

            BrowserUtils.AssertBrowserInternetAvailability(APP_TO_CHECK, false);
            BrowserUtils.KillAllBrowsers();

            EnableKillSwitch(KillSwitchMode.Standard);
            HomeRobot
                .Verify.IsConnected();

            BrowserUtils.AssertBrowserInternetAvailability(APP_TO_CHECK, false);
            BrowserUtils.KillAllBrowsers();
        }
        finally
        {
            DisableKillSwitch();
            ScriptHelper.RemoveChromeFirewallRule();
        }
    }

    [Test]
    public void ConnectionRestoresAfterStoppingVpnService()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        KillVpnService();

        try
        {
            HomeRobot.Verify.IsConnecting();
        }
        catch (TimeoutException)
        {
            //do nothing
        }

        HomeRobot.Verify.IsConnected();

        //Note: DNS leaks are expected in this scenario, unless Kill Switch is set to "Advanced"
        BrowserUtils.AssertBrowserInternetAvailability(APP_TO_CHECK, true);
    }

    [Test]
    public void ConnectWithoutInternet()
    {
        try
        {
            ScriptHelper.DisableInternet();
            NetworkUtils.AssertInternetAvailability(false);

            HomeRobot
                .ConnectViaConnectionCard()
                .Verify.IsConnecting();

            Thread.Sleep(TestConstants.ThirtySecondsTimeout);

            ScriptHelper.EnableInternet();
            NetworkUtils.AssertInternetAvailability(true);

            HomeRobot
                .Verify.IsConnected();
        }
        finally
        {
            ScriptHelper.EnableInternet();
            NetworkUtils.AssertInternetAvailability(true);
        }
    }

    private void ConnectToCountryAndVerify(string countryName = COUNTRY_NAME_TWO)
    {
        SidebarRobot
            .ConnectToCountry(countryName);
        HomeRobot
            .Verify.IsConnected();
    }

    private void LoginFreshWithWireGuardOn()
    {
        App?.Close();
        App?.Dispose();
        ScriptHelper.ConnectToWireGuard();
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        ScriptHelper.VerifyWireGuardIsConnected();
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    private void EnableKillSwitch(KillSwitchMode mode)
    {
        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings()
            .EnableKillSwitchToggle()
            .SelectKillSwitchMode(mode)
            .ApplySettings()
            .CloseSettings();
    }

    private void DisableKillSwitch()
    {
        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings()
            .DisableKillSwitchToggle()
            .ApplySettings()
            .CloseSettings();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        ScriptHelper.RemoveChromeFirewallRule();
        ScriptHelper.EnableInternet();
    }
}