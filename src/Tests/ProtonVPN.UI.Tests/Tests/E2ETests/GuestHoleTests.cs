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
using ProtonVPN.UI.Tests.ApiClient.TestEnv;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("BTI")]
public class GuestHoleTests : BaseTest
{
    private const string CREATE_ACCOUNT_WINDOW = "Proton VPN: Sign-up";
    private const string GH_CONNECTION_REQUESTED_LINE_TO_LOOK_FOR = "Guest hole connection requested";
    private const string GH_CONNECTED_LINE_TO_LOOK_FOR = "Status updated to Connected (Guest hole)";
    private const string GH_DISCONNECTED_LINE_TO_LOOK_FOR = "Disconnected from guest hole";

    private static string ClientLogsPath => TestConstants.ClientLogsPath;
    private static DateTime Deadline => DateTime.UtcNow.AddMinutes(3);

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Scenario has to be set before app is launched
        BtiController.SetScenario(Scenarios.RESET);
        BtiController.SetScenario(Scenarios.BLOCK_DOH_ENDPOINT);
        // Allow some time for network to settle down
        Thread.Sleep(TestConstants.FiveSecondsTimeout);
    }

    [SetUp]
    public void SetUp()
    {
        LaunchClient(ClientLaunchParams.StartWithoutDisconnectingFromWireGuard);
    }

    [Test, Order(0)]
    [Property("TestCaseId", "760484")]
    [Retry(3)]
    public void SignUpUsingGuestHoles()
    {
        BrowserUtils.KillAllBrowsers();

        LoginRobot.ClickCreateAccountButton();

        DesktopRobot
            .Verify.IsWindowTitlePresent(CREATE_ACCOUNT_WINDOW);
        //TODO: https://account.protonvpn.com/signup?ref=windows 
        //Note: it's important that ?ref=windows is added at the end of the URL when performing the test from Windows client;
        BrowserUtils.KillAllBrowsers();
    }

    [Test, Order(1)]
    [Property("TestCaseId", "602362")]
    [Retry(3)]
    public void LoginUsingGuestHoles()
    {
        NetworkUtils.AssertInternetAvailability(true);

        CommonUiFlows.FullLogin(TestUserData.VisionaryUser, TestConstants.IsProTunVersion);

        WindowsUtils.AssertLogFile(ClientLogsPath, GH_CONNECTION_REQUESTED_LINE_TO_LOOK_FOR);
        WindowsUtils.AssertLogFile(ClientLogsPath, GH_CONNECTED_LINE_TO_LOOK_FOR);
        WindowsUtils.AssertLogFile(ClientLogsPath, GH_DISCONNECTED_LINE_TO_LOOK_FOR);

        CommonUiFlows.Logout(TestConstants.FourMinutesTimeout);
    }

    [Test, Order(2)]
    [Property("TestCaseId", "760484")]
    [Retry(3)]
    public void CancelLoginUsingGuestHoles()
    {
        LoginRobot.Login(TestUserData.VisionaryUser);

        WaitForGuestHolesToKickIn();

        LoginRobot
            .CancelLogin()
            .Verify.IsLoginWindowDisplayed(TestConstants.FourMinutesTimeout);

        WindowsUtils.AssertLogFile(ClientLogsPath, GH_DISCONNECTED_LINE_TO_LOOK_FOR);
    }

    [Test, Order(3)]
    [Property("TestCaseId", "760484")]
    [Retry(3)]
    public void GuestHolesAreTriggeredOnEachLogin()
    {
        for (int i = 0; i < 2; i++)
        {
            LoginRobot.Login(TestUserData.IncorrectUserAndPass);

            WaitForGuestHolesToKickIn();

            CommonUiFlows.CloseConnectionHelpModalIfDisaplyed(TestConstants.TwoMinutesTimeout);

            LoginRobot.Verify.IsLoginWindowDisplayed(TestConstants.FourMinutesTimeout);

            WindowsUtils.AssertLogFile(ClientLogsPath, GH_DISCONNECTED_LINE_TO_LOOK_FOR);
        }
    }

    [Test, Order(4)]
    [Property("TestCaseId", "602360")]
    [Retry(3)]
    public void HumanVerificationUsingGuestHoles()
    {
        LoginRobot.Login(TestUserData.HumanVerificationUser); //OR try with invalid pass ~7times - BUT THAT WILL TAKE TIME!

        WaitForCaptchaToShow();

        WindowsUtils.AssertLogFile(ClientLogsPath, GH_CONNECTION_REQUESTED_LINE_TO_LOOK_FOR);

        LoginRobot.ClickCloseCaptchaButton();
    }

    private void WaitForGuestHolesToKickIn()
    {
        while (DateTime.UtcNow < Deadline)
        {
            if (WindowsUtils.GetLastLogLine(ClientLogsPath, GH_CONNECTION_REQUESTED_LINE_TO_LOOK_FOR) != null)
            {
                return;
            }

            Thread.Sleep(TestConstants.FiveSecondsTimeout);
        }

        Assert.Fail($"Timed out waiting for '{GH_CONNECTION_REQUESTED_LINE_TO_LOOK_FOR}' in {ClientLogsPath} after 3 minutes.");
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
