/*
 * Copyright (c) 2025 Proton AG
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

using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class ConnectionTests : FreshSessionSetUp
{
    private const string FAST_CONNECTION = "Fastest country";

    private const string RANDOM_COUNTRY = "Random country";

    // These 4 countries are all available options in the All, Secure Core, P2P, and Tor tabs.
    // Trying United States first as it has the most servers available and there are less chances for them to be all under maintenance at the same time
    private static readonly List<string> _countries = ["United States", "France", "Germany", "Hong Kong"];

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Category("ARM")]
    public void QuickConnect()
    {
        string ipAddressNotConnected = NetworkUtils.GetIpAddressWithRetry();

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        HomeRobot
            .Verify.IsDisconnected()
            .ConnectViaConnectionCard()
            .Verify.IsConnecting()
                   .IsConnected();

        string ipAddressConnected = NetworkUtils.GetIpAddressWithRetry();

        HomeRobot.Verify.AssertVpnConnectionEstablished(ipAddressNotConnected, ipAddressConnected);

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();

        NavigationRobot
            .Verify.IsOnLocationDetailsPage();
    }

    [Test]
    [Category("ARM")]
    public void ConnectToFastestCountry()
    {
        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnConnectionsPage()
                   .IsOnLocationDetailsPage();

        HomeRobot
            .Verify.IsDisconnected();

        SidebarRobot
            .ConnectToFastest();

        HomeRobot
            .Verify.IsConnecting()
                   .IsConnected();

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();

        NavigationRobot
            .Verify.IsOnLocationDetailsPage();
    }

    [Test]
    [Retry(3)]
    [Category("ARM")]
    public void ConnectAndCancel()
    {
        HomeRobot.ConnectViaConnectionCard()
            .Verify.IsConnecting();
        // Imitate user's delay
        Thread.Sleep(500);
        HomeRobot.CancelConnection()
            .Verify.IsDisconnected();
    }

    [Test]
    public void LocalNetworkingIsReachableWhileConnected()
    {
        HomeRobot
            .Verify.IsDisconnected()
            .ConnectViaConnectionCard()
            .Verify.IsConnecting()
                   .IsConnected();

        NetworkUtils.VerifyIfLocalNetworkingWorks();
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

        App.Close();
        App.Dispose();

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

        App.Close();
        App.Dispose();

        LaunchApp(isFreshStart: false);

        NavigationRobot
            .Verify.IsOnMainPage();

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

        HomeRobot.ConnectViaConnectionCard()
            .Verify.IsConnected();

        string ipAddressBeforeClientKill = NetworkUtils.GetIpAddressWithRetry();

        // Allow some time for the app to settle down to imitate user's delay
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        App.Kill();
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
    public void ClosingTheAppDoesNotStopVpnConnection()
    {
        string ipAddressConnected = NetworkUtils.GetIpAddressWithRetry();

        HomeRobot.ConnectViaConnectionCard()
            .Verify.IsConnected()
            .CloseClientViaCloseButton();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(ipAddressConnected);
    }

    [Test]
    public void ConnectToVpnFastestCountryAndRandomCountry()
    {
        NavigationRobot
           .Verify.IsOnHomePage()
                  .IsOnConnectionsPage();
        HomeRobot
            .Verify.IsDisconnected()
            .SelectVpnConnectionOption(VpnConnectionOptions.Fast)
            .ConnectViaConnectionCard()
            .Verify.DoesConnectionCardTitleEqual(FAST_CONNECTION)
            .Disconnect();

        HomeRobot
            .Verify.IsDisconnected()
            .SelectVpnConnectionOption(VpnConnectionOptions.Random)
            .ConnectViaConnectionCard()
            .Verify.DoesConnectionCardTitleEqual(RANDOM_COUNTRY)
            .Disconnect();
    }

    [Test]
    public void ConnectToSecureCoreServerCountriesListAndDisconnectViaCountry()
    {
        ConnectAndDisconnectViaSearchCountry(CountriesTab.SecureCore);
    }

    [Test]
    public void ConnectToP2PServerCountriesListAndDisconnectViaCountry()
    {
        ConnectAndDisconnectViaSearchCountry(CountriesTab.P2P);
    }

    [Test]
    public void ConnectToTorServerCountriesListAndDisconnectViaCountry()
    {
        ConnectAndDisconnectViaSearchCountry(CountriesTab.Tor);
    }

    private void ConnectAndDisconnectViaSearchCountry(CountriesTab tab)
    {
        string ipBeforeConnection = NetworkUtils.GetIpAddressWithRetry();

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnConnectionsPage();

        SearchAndConnectToCountry(tab, out string countryCode);

        string ipAfterConnection = NetworkUtils.GetIpAddressWithRetry();

        HomeRobot
            .Verify.AssertVpnConnectionEstablished(ipBeforeConnection, ipAfterConnection);

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();

        SidebarRobot.DisconnectViaCountry(countryCode);

        HomeRobot
            .Verify.IsDisconnected();
        NavigationRobot
            .Verify.IsOnLocationDetailsPage();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(ipBeforeConnection);
    }

    private void SearchAndConnectToCountry(CountriesTab tab, out string countryCode)
    {
        countryCode = string.Empty;
        string failureMessages = string.Empty;

        foreach (string country in _countries)
        {
            try
            {
                countryCode = CountryCodes.GetCode(country);

                SidebarRobot
                    .SearchFor(country)
                    .NavigateToCountriesTabAfterSearch(tab)
                    .ConnectToCountry(countryCode);

                HomeRobot
                    .Verify.IsConnecting()
                           .IsConnected();

                Thread.Sleep(1000);

                return;
            }
            catch (AssertionException e)
            {
                failureMessages += $"Failed to connect to {countryCode} ({tab}): {e.Message}\n";
            }
        }

        Assert.Fail(failureMessages);
    }
}