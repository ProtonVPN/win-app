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

using System.Threading;
using System.Collections.Generic;
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

    private const string RANDOM_COUNTRY = "Random country";

    private const string SLOW_TOR_COUNTRY = "United States";

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Category("ARM")]
    public void QuickConnect()
    {
        CommonUiFlows.EnsureUserIsDisconnected();

        string ipAddressNotConnected = NetworkUtils.GetIpAddressWithRetry();

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        HomeRobot
            .Verify.IsDisconnected()
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

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
            .Verify.IsConnected();

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
        HomeRobot
            .Verify.IsDisconnected()
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

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
    public void ClosingTheAppDoesStopVpnConnection()
    {
        string ipAddressBeforeConnected = NetworkUtils.GetIpAddressWithRetry();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .CloseClientViaCloseButton();

        // Delay to make sure that connection is not lost even after brief delay.
        Thread.Sleep(TestConstants.FiveSecondsTimeout);
        NetworkUtils.VerifyIpAddressMatchesWithRetry(ipAddressBeforeConnected);
    }

    [Test]
    public void ConnectToVpnFastestCountryAndRandomCountry()
    {
        NavigationRobot
           .Verify.IsOnHomePage()
                  .IsOnConnectionsPage();

        HomeRobot
            .Verify.IsDisconnected()
            .SelectDefaultConnectionOption(VpnConnectionOption.Fast)
            .ConnectViaConnectionCard()
            .Verify.ConnectionCardTitleEquals(FAST_CONNECTION)
                   .IsConnected()
            .Disconnect();

        HomeRobot
            .Verify.IsDisconnected()
            .SelectDefaultConnectionOption(VpnConnectionOption.Random)
            .ConnectViaConnectionCard()
            .Verify.ConnectionCardTitleEquals(RANDOM_COUNTRY)
                   .IsConnected()
            .Disconnect();
    }
}