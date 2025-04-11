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

using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Annotations;
using ProtonVPN.UI.Tests.ApiClient.Prod;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.SliTests;

[TestFixture]
[Category("SLI")]
[Workflow("main_measurements")]
public class ConnectionSLIs : SliSetUp
{
    private ProdTestApiClient _prodTestApiClient = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Duration, TestStatus]
    [Sli("specific_server_connect")]
    public async Task ConnectionToSpecificServer()
    { 
        SecureString password = new NetworkCredential("", TestUserData.PlusUser.Password).SecurePassword;
        string serverName = await _prodTestApiClient.GetRandomSpecificPaidServerAsync(TestUserData.PlusUser.Username, password);

        SidebarRobot
            .SearchFor(serverName)
            .ConnectToFirstSpecificServer();

        HomeRobot.Verify.IsConnected();
        HomeRobot.Disconnect();

        // Simulate users delay
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        SidebarRobot
            .SearchFor(serverName)
            .ConnectToFirstSpecificServer();

        SliHelper.MeasureTime(() =>
        {
            HomeRobot.Verify.IsConnected();
        });

        HomeRobot.Disconnect();
    }

    [Test]
    [Duration, TestStatus]
    [Sli("quick_connect")]
    public void QuickConnectPerformance()
    {
        // First connection is made to make sure that everything is setup
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        HomeRobot.Disconnect();

        // Simulate users delay
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        HomeRobot.ConnectViaConnectionCard();

        SliHelper.MeasureTime(() =>
        {
            HomeRobot.Verify.IsConnected();
        });

        HomeRobot.Disconnect();
    }
}
