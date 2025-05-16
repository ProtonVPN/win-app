/*
 * Copyright (c) 2024 Proton AG
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

        Assert.That(ipAddressNotConnected.Equals(ipAddressConnected), Is.False,
            $"User was not connected to VPN server. IP Address not connected: {ipAddressNotConnected}." +
            $" IP Address connected: {ipAddressConnected}");

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
        HomeRobot.Verify.IsConnected();
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
        HomeRobot.Verify.IsDisconnected();
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
        Assert.That(ipAddressBeforeClientKill.Equals(ipAddressAfterClientKill), Is.True,
            $"VPN Connection was lost after app was killed. IP Address before client was killed: {ipAddressBeforeClientKill}." +
            $" IP Address after client was killed: {ipAddressAfterClientKill}");

        LaunchApp(isFreshStart: false);
        HomeRobot.Verify.IsConnected();

        string ipAddressAfterClientIsRestored = NetworkUtils.GetIpAddressWithRetry();
        Assert.That(ipAddressBeforeClientKill.Equals(ipAddressAfterClientIsRestored), Is.True,
            $"VPN Connection was lost/reconnected after client was resumed. IP Address before client was killed: {ipAddressAfterClientIsRestored}." +
            $" IP Address after client was restored: {ipAddressAfterClientIsRestored}");
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
}