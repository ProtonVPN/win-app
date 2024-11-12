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
using ProtonVPN.UI.Tests.Annotations;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.SliTests;

[TestFixture]
[Category("SLI")]
[Workflow("main_measurements")]
public class VpnSpeedSLIs : SliSetUp
{
    private const string COUNTRY_NAME = "Germany";
    private const string COUNTRY_CODE = "DE";

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Sli("network_speed")]
    public void VpnSpeedMeasurements()
    {
        SliHelper.AddNetworkSpeedToMetrics("download_speed_disconnected", "upload_speed_disconnected");

        SidebarRobot
            .SearchFor(COUNTRY_NAME)
            .ConnectToCountry(COUNTRY_CODE);

        HomeRobot.Verify.IsConnected();

        // Allow some time for the network to settle down.
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        SliHelper.AddNetworkSpeedToMetrics("download_speed_connected", "upload_speed_connected");

        HomeRobot.Disconnect();
    }  
}
