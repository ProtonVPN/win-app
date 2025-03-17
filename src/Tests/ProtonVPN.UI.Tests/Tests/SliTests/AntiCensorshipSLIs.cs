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
using ProtonVPN.UI.Tests.ApiClient.TestEnv;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.SliTests;

[TestFixture]
[Category("SLI-BTI-PROD")]
[Workflow("anti-censorship")]
public class AntiCensorshipSLIs : SliSetUp
{
    [Test]
    [Duration, TestStatus]
    [Sli("alt_routing_login")]
    public void AlternativeRoutingSliMeasurement()
    {
        // Scenario has to be set before app is launched
        BtiController.SetScenario(Scenarios.RESET);
        BtiController.SetScenario(Scenarios.BLOCK_PROD_ENDPOINT);
        // Allow some time for network to settle down
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        LaunchApp();

        LoginRobot.Login(TestUserData.PlusUser);
        SliHelper.MeasureTime(() =>
        {
            HomeRobot.Verify.IsWelcomeModalDisplayed();
        });
    }

    [Test]
    [Duration, TestStatus]
    [Sli("guest_holes_login")]
    public void GuestHolesSliMeasurements()
    {
        // Scenario has to be set before app is launched
        BtiController.SetScenario(Scenarios.RESET);
        BtiController.SetScenario(Scenarios.BLOCK_DOH_ENDPOINT);
        // Allow some time for network to settle down
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        LaunchApp();

        LoginRobot.Login(TestUserData.PlusUser);
        SliHelper.MeasureTime(() =>
        {
            HomeRobot.Verify.IsWelcomeModalDisplayed();
        });
    }

    [TearDown]
    public void TearDown()
    {
        BtiController.SetScenario(Scenarios.RESET);
    }
}
