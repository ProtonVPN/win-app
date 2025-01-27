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
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.SliTests;

[TestFixture]
[Category("SLI")]
[Workflow("protocol_performance")]
public class StealthSLIs : SliSetUp
{
    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Duration, TestStatus]
    [Sli("wireguard_tls")]
    public void WireguardTlsConnectionSpeed()
    {
        SettingRobot
            .OpenSettings()
            .OpenProtocolSettings()
            .SelectProtocol(Protocol.WireGuardTls)
            .ApplySettings()
            .CloseSettings();

        // Two time connection is needed to test real conditions, when everything was setup.
        HomeRobot.ConnectViaConnectionCard()
            .Verify.IsConnected();
        HomeRobot.Disconnect();

        // Imitate users delay
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        HomeRobot.ConnectViaConnectionCard();
        SliHelper.MeasureTime(() =>
        {
            HomeRobot.Verify.IsConnected();
        });
        SliHelper.MeasureTestStatus(() =>
        {
            HomeRobot.Verify.IsProtocolDisplayed(Protocol.WireGuardTls);
        });

        HomeRobot.Disconnect();
    } 
}
