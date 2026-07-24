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
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.SliTests;

[TestFixture]
[Category("SLI")]
[Workflow("main_measurements")]
public class ConnectionSLIs : SliSetUp
{
    private const Country SECURE_CORE_COUNTRY = Country.Australia;
    private const Country P2P_COUNTRY = Country.Algeria;
    private const Country TOR_COUNTRY = Country.France;

    private ProdTestApiClient _prodTestApiClient = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchClient();
        CommonUiFlows.FullLogin(TestUserData.PlusUser, TestConstants.IsProTunVersion);

        try
        {
            HomeRobot
                .Verify.IsUpsellModalDisplayed()
                .DismissUpsellModal();
        }
        catch { }
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

        if (!TestConstants.IsProTunVersion)
        {
            ConfirmationRobot
                .CancelAction();
        }

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

        ConnectAndDisconnectServer(CountryTab.All, serverName);
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

    private void ConnectAndDisconnect(CountryTab tab, Country countryName)
    {
        // First connection is made to make sure that everything is setup
        SidebarRobot
            .SearchFor(countryName.GetName())
            .NavigateToCountriesTabAfterSearch(tab)
            .ConnectToCountry(countryName);
        
        HomeRobot
            .Verify.IsConnected()
            .Disconnect();

        // Simulate users delay
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        SidebarRobot
            .SearchFor(countryName.GetName())
            .NavigateToCountriesTabAfterSearch(tab)
            .ConnectToCountry(countryName);

        SliHelper.MeasureTime(() =>
        {
            HomeRobot.Verify.IsConnected();
        });

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();
    }

    private void ConnectAndDisconnectServer(CountryTab tab, string serverName)
    {
        SidebarRobot
            .SearchFor(serverName)
            .NavigateToCountriesTabAfterSearch(tab)
            .ConnectToServer();

        HomeRobot
            .Verify.IsConnected()
            .Disconnect();

        Thread.Sleep(TestConstants.TenSecondsTimeout);

        SidebarRobot
            .SearchFor(serverName)
            .NavigateToCountriesTabAfterSearch(tab)
            .ConnectToServer();

        SliHelper.MeasureTime(() =>
        {
            HomeRobot.Verify.IsConnected();
        });

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();
    }
}
