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

using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Annotations;
using ProtonVPN.UI.Tests.ApiClient.Prod;

namespace ProtonVPN.UI.Tests.Tests.SliTests;

[TestFixture]
[Category("SLI")]
[Workflow("main_measurements")]
public class ConnectionSLIs : SliSetUp
{
    private const string SECURE_CORE_COUNTRY = "Australia";
    private const string P2P_COUNTRY = "Algeria";
    private const string TOR_COUNTRY = "France";

    private ProdTestApiClient _prodTestApiClient = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Duration, TestStatus]
    [Sli("quick_connect")]
    public void QuickConnectPerformance()
    {
        // First connection is made to make sure that everything is setup
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .Disconnect();

        // Simulate users delay
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        HomeRobot.ConnectViaConnectionCard();

        SliHelper.MeasureTime(() =>
        {
            HomeRobot.Verify.IsConnected();
        });

        HomeRobot.Disconnect();
    }

    [Test]
    [Duration, TestStatus]
    [Sli("specific_server_connect")]
    public async Task ConnectToSpecificServerPerformanceAsync()
    {
        SecureString password = new NetworkCredential("", TestUserData.PlusUser.Password).SecurePassword;
        string serverName = await _prodTestApiClient.GetRandomSpecificPaidServerAsync(TestUserData.PlusUser.Username, password);

        ConnectAndDisconnect(CountryTab.All, serverName, true);
    }

    [Test]
    [Duration, TestStatus]
    [Sli("secure_core_connect")]
    public void ConnectToSecureCorePerformance()
    {
        ConnectAndDisconnect(CountryTab.SecureCore, SECURE_CORE_COUNTRY);
    }

    [Test]
    [Duration, TestStatus]
    [Sli("p2p_connect")]
    public void ConnectToP2PPerformance()
    {
        ConnectAndDisconnect(CountryTab.P2P, P2P_COUNTRY);
    }

    [Test]
    [Duration, TestStatus]
    [Sli("tor_connect")]
    public void ConnectToTorPerformance()
    {
        ConnectAndDisconnect(CountryTab.Tor, TOR_COUNTRY);
    }

    private void ConnectAndDisconnect(CountryTab tab, string connection, bool isServer = false)
    {
        // First connection is made to make sure that everything is setup
        SidebarRobot
            .SearchFor(connection)
            .NavigateToCountriesTabAfterSearch(tab);

        if (isServer)
        {
            SidebarRobot.ConnectToServer();
        }
        else
        {
            SidebarRobot.ConnectToCountry(CountryCodes.GetCode(connection));
        }

        HomeRobot
            .Verify.IsConnected()
            .Disconnect();

        // Simulate users delay
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        SidebarRobot
            .SearchFor(connection)
            .NavigateToCountriesTabAfterSearch(tab);

        if (isServer)
        {
            SidebarRobot.ConnectToServer();
        }
        else
        {
            SidebarRobot.ConnectToCountry(CountryCodes.GetCode(connection));
        }

        SliHelper.MeasureTime(() =>
        {
            HomeRobot.Verify.IsConnected();
        });

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();
    }
}
