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

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class ConnectionTests : FreshSessionSetUp
{
    private const string COUNTRY_CODE = "AU";

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void QuickConnect()
    {
        string ipAddressNotConnected = NetworkUtils.GetIpAddress();

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        HomeRobot
            .Verify.IsDisconnected()
            .ConnectToDefaultConnection()
            .Verify.IsConnecting()
                   .IsConnected();

        string ipAddressConnected = NetworkUtils.GetIpAddress();

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
    public void ConnectAndCancel()
    {
        HomeRobot.ConnectToDefaultConnection()
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
            .ConnectToDefaultConnection()
            .Verify.IsConnecting()
                   .IsConnected();

        NetworkUtils.VerifyIfLocalNetworkingWorks();
    }
}