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

using System;
using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.ApiClient.TestEnv;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("BTI")]
public class AlternativeRoutingTests : BaseTest
{
    private const string LINE_TO_LOOK_FOR = "dns-query";

    private static DateTime Deadline => DateTime.UtcNow.AddMinutes(3);

    private static string ClientLogsPath => TestConstants.ClientLogsPath;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Scenario has to be set before app is launched
        BtiController.SetScenario(Scenarios.RESET);
        BtiController.SetScenario(Scenarios.BLOCK_PROD_ENDPOINT);
        // Allow some time for network to settle down
        Thread.Sleep(TestConstants.FiveSecondsTimeout);
    }

    [SetUp]
    public void SetUp()
    {
        LaunchClient(ClientLaunchParams.StartWithoutDisconnectingFromWireGuard);
    }

    [Test, Order(0)]
    [Property("TestCaseId", "602358")]
    [Retry(3)]
    public void LoginUsingAlternativeRouting()
    {
        NetworkUtils.AssertInternetAvailability(true);

        CommonUiFlows.FullLogin(TestUserData.VisionaryUser, TestConstants.IsProTunVersion);

        WindowsUtils.AssertLogFile(ClientLogsPath, LINE_TO_LOOK_FOR);

        CommonUiFlows.Logout(TestConstants.FourMinutesTimeout);
    }

    [Test, Order(1)]
    [Property("TestCaseId", "760483")]
    [Retry(3)]
    public void CancelLoginUsingAlternativeRouting()
    {
        LoginRobot.Login(TestUserData.VisionaryUser);

        WaitForAlternativeRoutingToKickIn();

        LoginRobot
            .CancelLogin()
            .Verify.IsLoginWindowDisplayed(TestConstants.FourMinutesTimeout);
    }

    [Test, Order(2)]
    [Property("TestCaseId", "602359")]
    [Retry(3)]
    public void HumanVerificationUsingAlternativeRouting()
    {
        LoginRobot.Login(TestUserData.HumanVerificationUser); //OR try with invalid pass ~7times - BUT THAT WILL TAKE TIME!

        WaitForCaptchaToShow();

        WindowsUtils.AssertLogFile(ClientLogsPath, LINE_TO_LOOK_FOR);

        LoginRobot.ClickCloseCaptchaButton();
    }

    private void WaitForAlternativeRoutingToKickIn()
    {
        while (DateTime.UtcNow < Deadline)
        {
            if (WindowsUtils.GetLastLogLine(ClientLogsPath, LINE_TO_LOOK_FOR) != null)
            {
                return;
            }

            Thread.Sleep(TestConstants.FiveSecondsTimeout);
        }

        Assert.Fail($"Timed out waiting for '{LINE_TO_LOOK_FOR}' in {ClientLogsPath} after 3 minutes.");
    }

    private void WaitForCaptchaToShow()
    {
        while (DateTime.UtcNow < Deadline)
        {
            try
            {
                LoginRobot.Verify.IsCaptchaDisplayed();
                return;
            }
            catch { }

            Thread.Sleep(TestConstants.FiveSecondsTimeout);
        }

        Assert.Fail($"Timed out waiting for Captcha to show after 3 minutes.");
    }

    [TearDown]
    public void TearDown()
    {
        Cleanup();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        BtiController.SetScenario(Scenarios.RESET);
    }
}
