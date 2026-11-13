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
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class ConnectionTests : FreshSessionSetUp
{
    private const Country COUNTRY_NAME_ONE = Country.Angola;
    private const Country COUNTRY_NAME_TWO = Country.Austria;
    private const Country SLOW_TOR_COUNTRY = Country.UnitedStates;
    private const City CITY_NAME_ONE = City.Vienna;

    private const Browser APP_TO_CHECK = Browser.GoogleChrome;

    private static readonly string _fastestCountry = LanguageHelper.GetTranslatedString("Country_Fastest");

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Property("TestCaseId", "602341,602342")]
    [Category("ARM")]
    [Category("SMOKE_1")]
    public void QuickConnectToServerAndDisconnect()
    {
        CommonUiFlows.EnsureUserIsDisconnected();

        string ipAddressNotConnected = NetworkHelper.GetIpAddressWithRetry();

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        HomeRobot
            .Verify.IsDisconnected()
                   .ConnectionCardTitleEquals(_fastestCountry)
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
                   .ConnectionCardTitleEquals(_fastestCountry);

        string ipAddressConnected = NetworkHelper.GetIpAddressWithRetry();

        HomeRobot
            .Verify.AssertVpnConnectionEstablished(ipAddressNotConnected, ipAddressConnected);

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();

        NavigationRobot
            .Verify.IsOnLocationDetailsPage();

        NetworkHelper.VerifyIpAddressMatchesWithRetry(ipAddressNotConnected);
    }

    [Test]
    [Property("TestCaseId", "602346")]
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
    [Property("TestCaseId", "602363")]
    [Category("ARM")]
    [Category("SMOKE_1")]
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

        NetworkHelper.VerifyLocalNetworking(isLanEnabled: true);

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

        NetworkHelper.VerifyLocalNetworking(isLanEnabled: false);
    }

    [Test]
    [Property("TestCaseId", "602340")]
    public void ClientKillDoesNotStopVpnConnection()
    {
        SettingRobot
           .OpenSettings()
           .OpenAutoStartupSettings()
           .DisableAutoLaunchSetting()
           .DisableAutoConnectionSetting()
           .ApplySettings()
           .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        string ipAddressBeforeClientKill = NetworkHelper.GetIpAddressWithRetry();

        // Allow some time for the app to settle down to imitate user's delay
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        App?.Kill();
        // Delay to make sure that connection is not lost even after brief delay.
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        string ipAddressAfterClientKill = NetworkHelper.GetIpAddressWithRetry();

        HomeRobot.Verify.AssertVpnConnectionAfterKill(ipAddressBeforeClientKill, ipAddressAfterClientKill);

        LaunchClient(ClientLaunchParams.StartWithNoOnboarding);

        HomeRobot.Verify.IsConnected();

        string ipAddressAfterClientIsRestored = NetworkHelper.GetIpAddressWithRetry();
        HomeRobot.Verify.AssertVpnConnectionAfterRestored(ipAddressBeforeClientKill, ipAddressAfterClientIsRestored);
    }

    [Test]
    [Property("TestCaseId", "789177")]
    [Retry(3)]
    public void AppExitStopsVpnConnection()
    {
        string ipAddressBeforeConnected = NetworkHelper.GetIpAddressWithRetry();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .ExpandKebabMenuButton()
            .ExitViaKebabMenuWithConfirmation();

        // Delay to make sure that connection is not lost even after brief delay.
        Thread.Sleep(TestConstants.FiveSecondsTimeout);
        NetworkHelper.VerifyIpAddressMatchesWithRetry(ipAddressBeforeConnected);
    }

    [Test]
    [Property("TestCaseId", "602378")]
    public void AppWindowCloseDoesNotStopVpnConnection()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .CloseClientViaCloseButton();

        string ipAddressAfterConnected = NetworkHelper.GetIpAddressWithRetry();

        // Delay to make sure that connection is not lost even after brief delay.
        Thread.Sleep(TestConstants.FiveSecondsTimeout);
        NetworkHelper.VerifyIpAddressMatchesWithRetry(ipAddressAfterConnected);

        TrayRobot.DoubleClickTrayApp();
    }

    [Test]
    [Property("TestCaseId", "602343")]
    public void ConnectToServerFromCountriesList()
    {
        SidebarRobot
            .NavigateToAllCountriesTab();
        ConnectToCountryAndVerify(COUNTRY_NAME_ONE);

        NetworkHelper.VerifyUserIsConnectedToExpectedCountry(COUNTRY_NAME_ONE);

        SidebarRobot
            .ExpandCities(COUNTRY_NAME_TWO)
            .ConnectToCity(CITY_NAME_ONE);
        HomeRobot
            .Verify.IsConnected();

        NetworkHelper.VerifyUserIsConnectedToExpectedCountry(COUNTRY_NAME_TWO);

        SidebarRobot
            .ExpandSpecificServerList()
            .ConnectToServer();
        HomeRobot
            .Verify.IsConnected();

        NetworkHelper.VerifyUserIsConnectedToExpectedCountry(COUNTRY_NAME_TWO);
    }

    [Test]
    [Property("TestCaseId", "602344")]
    [Retry(3)]
    public void DisconnectFromCountriesList()
    {
        string ipAddressNotConnected = NetworkHelper.GetIpAddressWithRetry();

        SidebarRobot
            .NavigateToAllCountriesTab();

        ConnectToCountryAndVerify();
        SidebarRobot
            .DisconnectViaCountry(COUNTRY_NAME_TWO);
        HomeRobot
            .Verify.IsDisconnected();

        NetworkHelper.VerifyIpAddressMatchesWithRetry(ipAddressNotConnected);

        ConnectToCountryAndVerify();
        SidebarRobot
            .ExpandCities(COUNTRY_NAME_TWO)
            .DisconnectViaCity(CITY_NAME_ONE);
        HomeRobot
            .Verify.IsDisconnected();

        NetworkHelper.VerifyIpAddressMatchesWithRetry(ipAddressNotConnected);

        SidebarRobot
            .ExpandSpecificServerList()
            .ConnectToServer();
        HomeRobot
            .Verify.IsConnected();
        SidebarRobot
            .ExpandSpecificServerList()
            .DisconnectViaServer();
        HomeRobot
            .Verify.IsDisconnected();

        NetworkHelper.VerifyIpAddressMatchesWithRetry(ipAddressNotConnected);
    }

    [Test]
    [Property("TestCaseId", "602347")]
    public void CorrectIpIsShown()
    {
        HomeRobot
            .Verify.IsDisconnected()
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        string ipAddressConnected = NetworkHelper.GetIpAddressWithRetry();
        string vpnServerIp = HomeRobot.GetVpnServerIp()!;

        HomeRobot
            .Verify.AssertVPNIpAndExternalIpMatch(vpnServerIp, ipAddressConnected)
            .Disconnect()
            .Verify.IsDisconnected();
    }

    [Test]
    [Property("TestCaseId", "602424")]
    [Retry(3)]
    public void FreshSignInWhileConnectedToWireGuard()
    {
        try
        {
            LoginFreshWithWireGuardOn();

            HomeRobot
                .Verify.IsLocationDetailsPanelEmpty();
        }
        finally
        {
            ScriptHelper.DisconnectFromWireGuard();
            Thread.Sleep(TestConstants.TenSecondsTimeout);
        }

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();
    }

    [Test]
    [Property("TestCaseId", "609951")]
    [Category("5")]
    public void FirewallRulesAreNotIgnored()
    {
        CommonUiFlows.EnsureUserIsDisconnected();

        ScriptHelper.AddChromeFirewallRule();

        try
        {
            EnableKillSwitch(KillSwitchMode.Advanced);
            HomeRobot
                .ConnectViaConnectionCard()
                .Verify.IsConnected();

            BrowserHelper.AssertBrowserInternetAvailability(APP_TO_CHECK, shouldBeAvailable: false);
            BrowserHelper.KillAllBrowsers();

            EnableKillSwitch(KillSwitchMode.Standard);
            HomeRobot
                .Verify.IsConnected();

            BrowserHelper.AssertBrowserInternetAvailability(APP_TO_CHECK, shouldBeAvailable: false);
            BrowserHelper.KillAllBrowsers();
        }
        finally
        {
            DisableKillSwitch();
            ScriptHelper.RemoveChromeFirewallRule();
        }
    }

    [Test]
    [Property("TestCaseId", "602422")]
    [Retry(3)]
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

        //Give it time to properly restore internet
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        //Note: DNS leaks are expected in this scenario, unless Kill Switch is set to "Advanced"
        BrowserHelper.AssertBrowserInternetAvailability(APP_TO_CHECK, shouldBeAvailable: true);

        BrowserHelper.KillAllBrowsers();
    }

    [Test]
    [Property("TestCaseId", "602345")]
    [Retry(3)]
    public void ConnectWithoutInternet()
    {
        try
        {
            ScriptHelper.DisableInternet();
            NetworkHelper.AssertInternetAvailability(false);

            HomeRobot
                .ConnectViaConnectionCard()
                .Verify.IsConnecting();

            Thread.Sleep(TestConstants.ThirtySecondsTimeout);

            ScriptHelper.EnableInternet();
            NetworkHelper.AssertInternetAvailability(true);

            HomeRobot
                .Verify.IsConnected();
        }
        finally
        {
            ScriptHelper.EnableInternet();
            NetworkHelper.AssertInternetAvailability(true);
        }
    }

    private void ConnectToCountryAndVerify(Country countryName = COUNTRY_NAME_TWO)
    {
        SidebarRobot
            .ConnectToCountry(countryName);
        HomeRobot
            .Verify.IsConnected();
    }

    [Test]
    [Property("TestCaseId", "945422")]
    [Retry(3)]
    public void StreamingInProgressUpsellNotDisplayedForPlusUser()
    {
        CommonUiFlows.EnsureUserIsDisconnected();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        try
        {
            foreach (string website in BrowserHelper.StreamingWebsites)
            {
                BrowserHelper.OpenStreamingWebsite(APP_TO_CHECK, website);

                // Free users get the upsell within 30 seconds (see FreeUserTests), so give it a fair chance to (wrongly) show up
                Thread.Sleep(TestConstants.ThirtySecondsTimeout);

                UpsellCarrouselRobot.Verify.IsStreamingInProgressUpsellNotDisplayed();
            }
        }
        finally
        {
            BrowserHelper.KillAllBrowsers();
        }
    }

    [Test]
    [Property("TestCaseId", "889724")]
    [Ignore("JIRA - VPNWIN-3039")]
    public void WeakHostSettingIsDisabledWhenConnected()
    {
        const string getMainInterfaceIndex = "(Get-NetRoute -DestinationPrefix 0.0.0.0/0 | Sort-Object RouteMetric | Select-Object -First 1).InterfaceIndex";
        const string weakHostState = "$i = Get-NetIPInterface -InterfaceIndex {0} -AddressFamily IPv4; $i.WeakHostSend.ToString() + '|' + $i.WeakHostReceive.ToString()";

        CommonUiFlows.EnsureUserIsDisconnected();

        string interfaceIndex = ScriptHelper.RunPowerShellScript(getMainInterfaceIndex);

        // Remember the runner's own values so they can be put back, whatever happens in the test
        string[] originalState = ScriptHelper.RunPowerShellScript(string.Format(weakHostState, interfaceIndex)).Split('|');
        string originalWeakHostSend = originalState[0];
        string originalWeakHostReceive = originalState[1];

        try
        {
            ScriptHelper.RunPowerShellScript($"Set-NetIPInterface -InterfaceIndex {interfaceIndex} -AddressFamily IPv4 -WeakHostSend Enabled -WeakHostReceive Enabled");

            Assert.That(
                ScriptHelper.RunPowerShellScript(string.Format(weakHostState, interfaceIndex)),
                Is.EqualTo("Enabled|Enabled"),
                "WeakHostSend and WeakHostReceive should be enabled on the main interface before connecting.");

            HomeRobot
                .ConnectViaConnectionCard()
                .Verify.IsConnected();

            Assert.That(
                ScriptHelper.RunPowerShellScript(string.Format(weakHostState, interfaceIndex)),
                Is.EqualTo("Disabled|Disabled"),
                "WeakHostSend and WeakHostReceive should be disabled on the main interface while connected.");
        }
        finally
        {
            try
            {
                CommonUiFlows.EnsureUserIsDisconnected();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Could not disconnect before restoring the WeakHost settings: {ex.Message}");
            }

            // Restoring must not hide the real test failure, so a restore error is only logged
            try
            {
                ScriptHelper.RunPowerShellScript($"Set-NetIPInterface -InterfaceIndex {interfaceIndex} -AddressFamily IPv4 -WeakHostSend {originalWeakHostSend} -WeakHostReceive {originalWeakHostReceive}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"WARNING: Could not restore WeakHostSend={originalWeakHostSend}, WeakHostReceive={originalWeakHostReceive} on interface {interfaceIndex}: {ex.Message}");
            }
        }
    }

    private void LoginFreshWithWireGuardOn()
    {
        Cleanup();
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        ScriptHelper.ConnectToWireGuard();
        Thread.Sleep(TestConstants.TenSecondsTimeout);
        LaunchClient(ClientLaunchParams.StartWithoutDisconnectingFromWireGuard);
        NetworkHelper.AssertInternetAvailability(true);
        CommonUiFlows.FullLogin(TestUserData.VisionaryUser);
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