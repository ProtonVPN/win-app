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

using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("4")]
[Category("ARM")]
public class ConnectionFeedbackTests : FreshSessionSetUp
{
    private static string ClientLogsPath => TestConstants.ClientLogsPath;
    private static readonly string _positiveFeedback = "\"user_feedback\":\"positive\"";
    private static readonly string _negativeFeedback = "\"user_feedback\":\"negative\"";
    private static readonly string _ignoreFeedback = "\"user_feedback\":\"ignore\"";
    private static readonly string _unknownFeedback = "\"user_feedback\":\"unknown\"";
    private static readonly string _feedbackLogKey = "\"user_feedback\"";
    private static int _feedbackLogLinesBeforeScenario;

    private const Country COUNTRY = Country.Austria;

    [SetUp]
    public void SetUp()
    {
        bool isFreeUserTest = TestContext.CurrentContext.Test.MethodName == nameof(ConnectionFeedbackFree);
        CommonUiFlows.FullLogin(isFreeUserTest ? TestUserData.FreeUser : TestUserData.PlusUser);
    }

    [Test]
    [Property("TestCaseId", "890313")]
    public void ConnectionFeedbackDisappearsAfterTenSeconds()
    {
        for (int i = 0; i < 2; i++)
        {
            if (i == 0)
            {
                //fresh connect
                HomeRobot.ConnectViaConnectionCard();
            }
            else
            {
                //reconnect
                SidebarRobot.ConnectToCountry(COUNTRY);
            }

            Thread.Sleep(TestConstants.OneSecondTimeout);
            HomeRobot.Verify.IsConnected();

            TriggerTheConnectionFeedbackPopUp(ConnectionFeedbackTrigger.UnfocusApp);

            Thread.Sleep(TestConstants.TenSecondsTimeout);

            HomeRobot.Verify.IsConnectionFeedbackMissing();
        }
    }

    [Test]
    [Property("TestCaseId", "890318")]
    public void ConnectionFeedbackWithKeyboard()
    {
        for (int i = 0; i < 2; i++)
        {
            HomeRobot
                .ConnectViaConnectionCard()
                .Verify.IsConnected();

            if (i == 0)
            {
                TriggerTheConnectionFeedbackPopUp(ConnectionFeedbackTrigger.UnfocusApp);
                TriggerTheConnectionFeedbackScenario(ConnectionFeedbackScenario.ThumbsUp, useKeyboard: true);
                HomeRobot.Disconnect();
                CheckTheConnectionFeedbackScenario(ConnectionFeedbackScenario.ThumbsUp);
            }
            else
            {
                TriggerTheConnectionFeedbackPopUp(ConnectionFeedbackTrigger.OpenTrayApp);
                TriggerTheConnectionFeedbackScenario(ConnectionFeedbackScenario.ThumbsDown, useKeyboard: true, isTray: true);
                HomeRobot.Disconnect();
                CheckTheConnectionFeedbackScenario(ConnectionFeedbackScenario.ThumbsDown);

                TrayRobot.DoubleClickTrayApp();
            }
        }
    }

    [Test]
    [Property("TestCaseId", "889722")]
    public void ConnectionFeedbackPaid()
    {
        for (int i = 0; i < 3; i++)
        {
            HomeRobot
                .ConnectViaConnectionCard()
                .Verify.IsConnected();

            if (i == 0)
            {
                TriggerTheConnectionFeedbackPopUp(ConnectionFeedbackTrigger.MinimizeApp);
                TriggerTheConnectionFeedbackScenario(ConnectionFeedbackScenario.ThumbsUp);
                HomeRobot.Disconnect();
                CheckTheConnectionFeedbackScenario(ConnectionFeedbackScenario.ThumbsUp);
            }
            if (i == 1)
            {
                TriggerTheConnectionFeedbackPopUp(ConnectionFeedbackTrigger.UnfocusApp);
                TriggerTheConnectionFeedbackScenario(ConnectionFeedbackScenario.GoInSettings);
                HomeRobot.Disconnect();
                CheckTheConnectionFeedbackScenario(ConnectionFeedbackScenario.GoInSettings);
            }
            if (i == 2)
            {
                TriggerTheConnectionFeedbackPopUp(ConnectionFeedbackTrigger.OpenTrayApp);
                TriggerTheConnectionFeedbackScenario(ConnectionFeedbackScenario.JustDisconnect);
                CheckTheConnectionFeedbackScenario(ConnectionFeedbackScenario.JustDisconnect);

                TrayRobot.DoubleClickTrayApp();
            }
        }
    }

    [Test]
    [Property("TestCaseId", "889723")]
    public void ConnectionFeedbackFree()
    {
        for (int i = 0; i < 3; i++)
        {
            HomeRobot
                .ConnectViaConnectionCard()
                .Verify.IsConnected();

            if (i == 0)
            {
                TriggerTheConnectionFeedbackPopUp(ConnectionFeedbackTrigger.MinimizeApp);
                TriggerTheConnectionFeedbackScenario(ConnectionFeedbackScenario.GoInSettings);
                HomeRobot.Disconnect();
                CheckTheConnectionFeedbackScenario(ConnectionFeedbackScenario.GoInSettings);
            }
            if (i == 1)
            {
                TriggerTheConnectionFeedbackPopUp(ConnectionFeedbackTrigger.UnfocusApp);
                TriggerTheConnectionFeedbackScenario(ConnectionFeedbackScenario.JustDisconnect);
                CheckTheConnectionFeedbackScenario(ConnectionFeedbackScenario.JustDisconnect);
            }
            if (i == 2)
            {
                TriggerTheConnectionFeedbackPopUp(ConnectionFeedbackTrigger.OpenTrayApp);
                TriggerTheConnectionFeedbackScenario(ConnectionFeedbackScenario.ChangeServer);
                HomeRobot.Disconnect();
                CheckTheConnectionFeedbackScenario(ConnectionFeedbackScenario.ChangeServer);

                TrayRobot.DoubleClickTrayApp();
            }
        }
    }

    private static void TriggerTheConnectionFeedbackPopUp(ConnectionFeedbackTrigger trigger)
    {
        if (trigger == ConnectionFeedbackTrigger.MinimizeApp)
        {
            HomeRobot.MinimizeClientViaMinimizeButton();
            TrayRobot.DoubleClickTrayApp();
            HomeRobot.ClickOnConnectionCardTitle();
        }
        if (trigger == ConnectionFeedbackTrigger.UnfocusApp)
        {
            TrayRobot.ClickTaskbar();
            HomeRobot.ClickOnConnectionCardTitle();
        }
        if (trigger == ConnectionFeedbackTrigger.OpenTrayApp)
        {
            HomeRobot.CloseClientViaCloseButton();

            using (TrayApp)
            {
                TrayRobot.Verify.IsTrayFocused(true);
            }
        }
        if (trigger == ConnectionFeedbackTrigger.PutLaptopToSleep)
        {
            Assert.Inconclusive("Unable to automate");
        }

        HomeRobot.Verify.IsConnectionFeedbackDisplayed();

        if (TestConstants.IsDebugVersion)
        {
            _feedbackLogLinesBeforeScenario = LogFileHelper.CountLogLines(ClientLogsPath, _feedbackLogKey);
        }
    }

    private static void TriggerTheConnectionFeedbackScenario(ConnectionFeedbackScenario scenario, bool useKeyboard = false, bool isTray = false)
    {
        switch (scenario)
        {
            case ConnectionFeedbackScenario.ThumbsUp:
                HomeRobot.PressThumbsUpInTheConnectionFeedback(useKeyboard, isTray);
                break;
            case ConnectionFeedbackScenario.ThumbsDown:
                HomeRobot.PressThumbsDownInTheConnectionFeedback(useKeyboard, isTray);
                break;
            case ConnectionFeedbackScenario.JustDisconnect:
                HomeRobot.Disconnect();
                break;
            case ConnectionFeedbackScenario.ChangeServer:
                HomeRobot.ChangeServer();
                break;
            case ConnectionFeedbackScenario.GoInSettings:
                SettingRobot.OpenSettings();
                Thread.Sleep(TestConstants.TenSecondsTimeout);
                SettingRobot.CloseSettings();
                break;
        }

        HomeRobot.Verify.IsConnectionFeedbackMissing();
    }

    private static void CheckTheConnectionFeedbackScenario(ConnectionFeedbackScenario scenario)
    {
        if (!TestConstants.IsDebugVersion)
        {
            return;
        }

        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        int linesNow = LogFileHelper.CountLogLines(ClientLogsPath, _feedbackLogKey);
        Assert.That(linesNow, Is.GreaterThan(_feedbackLogLinesBeforeScenario),
            $"Expected at least one new '{_feedbackLogKey}' log line for scenario {scenario}.");

        switch (scenario)
        {
            case ConnectionFeedbackScenario.ThumbsUp:
                LogFileHelper.AssertLogFile(ClientLogsPath, _feedbackLogKey, _positiveFeedback);
                break;
            case ConnectionFeedbackScenario.ThumbsDown:
                LogFileHelper.AssertLogFile(ClientLogsPath, _feedbackLogKey, _negativeFeedback);
                break;
            case ConnectionFeedbackScenario.JustDisconnect:
                LogFileHelper.AssertLogFile(ClientLogsPath, _feedbackLogKey, _ignoreFeedback);
                break;
            case ConnectionFeedbackScenario.ChangeServer:
                LogFileHelper.AssertLogFile(ClientLogsPath, _feedbackLogKey, _unknownFeedback);
                break;
            case ConnectionFeedbackScenario.GoInSettings:
                LogFileHelper.AssertLogFile(ClientLogsPath, _feedbackLogKey, _ignoreFeedback);
                break;
        }
    }
}