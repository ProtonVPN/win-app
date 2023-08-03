/*
 * Copyright (c) 2023 Proton AG
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

using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.ReportIssue;
using ProtonVPN.UI.Tests.Robots.Settings;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests;

[TestFixture]
[Category("UI")]
public class ReportIssueTests : TestSession
{
    private const string SETTINGS_PAGE_TITLE = "Settings";
    private const string REPORT_ISSUE_WINDOW_TITLE = "Report an issue";
    private const string BROWSING_SPEED_PAGE_TITLE = "Browsing speed";

    private ShellRobot _shellRobot = new();
    private SettingsRobot _settingsRobot = new();
    private HomeRobot _homeRobot = new();
    private LoginRobot _loginRobot = new();
    private ReportIssueRobot _reportIssueRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();

        _shellRobot
            .Wait(TestConstants.InitializationDelay);
    }

    [Test]
    public void ReportIssueFromSettingsPage()
    {
        Login();

        _shellRobot
            .DoNavigateToSettingsPage()
            .VerifyCurrentPage(SETTINGS_PAGE_TITLE, false);

        _settingsRobot
            .DoReportAnIssue();

        _reportIssueRobot
            .RefreshReportIssueWindow()
            .VerifyReportIssueWindowIsOpened()
            .VerifyReportIssueStep(1, 3)
            .VerifyReportIssueWindowTitle(REPORT_ISSUE_WINDOW_TITLE);
    }

    [Test]
    public void ReportIssueThenNavigate()
    {
        ReportIssueFromSettingsPage();

        _reportIssueRobot
            .DoSelectBrowsingSpeedCategory()
            .Wait(TestConstants.DefaultNavigationDelay)
            .VerifyReportIssueStep(2, 3)
            .VerifyReportIssueWindowTitle($"{REPORT_ISSUE_WINDOW_TITLE} - {BROWSING_SPEED_PAGE_TITLE}");
        
        _reportIssueRobot
            .DoGoToContactForm()
            .Wait(TestConstants.DefaultNavigationDelay)
            .VerifyReportIssueStep(3, 3)
            .VerifyReportIssueWindowTitle($"{REPORT_ISSUE_WINDOW_TITLE} - {BROWSING_SPEED_PAGE_TITLE}");

        _reportIssueRobot
            .DoGoBack()
            .Wait(TestConstants.DefaultNavigationDelay)
            .VerifyReportIssueStep(2, 3)
            .VerifyReportIssueWindowTitle($"{REPORT_ISSUE_WINDOW_TITLE} - {BROWSING_SPEED_PAGE_TITLE}");

        _reportIssueRobot
            .DoGoBack()
            .Wait(TestConstants.DefaultNavigationDelay)
            .VerifyReportIssueStep(1, 3)
            .VerifyReportIssueWindowTitle(REPORT_ISSUE_WINDOW_TITLE);
    }

    [Test]
    public void ReportIssueFromHelpComponent()
    {
        Login();

        _homeRobot
            .DoReportBrowsingSpeedIssue();

        _reportIssueRobot
            .RefreshReportIssueWindow()
            .VerifyReportIssueWindowIsOpened()
            .VerifyReportIssueStep(2, 3)
            .VerifyReportIssueWindowTitle($"{REPORT_ISSUE_WINDOW_TITLE} - {BROWSING_SPEED_PAGE_TITLE}");
    }

    [Test]
    public void ReportIssueFromLoginScreen()
    {
        _loginRobot
            .Wait(TestConstants.InitializationDelay)
            .DoReportAnIssue();

        _reportIssueRobot
            .RefreshReportIssueWindow()
            .VerifyReportIssueWindowIsOpened()
            .VerifyReportIssueStep(1, 3)
            .VerifyReportIssueWindowTitle(REPORT_ISSUE_WINDOW_TITLE);
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }

    private void Login()
    {
        _loginRobot
            .Wait(TestConstants.InitializationDelay)
            .DoLogin(TestUserData.PlusUser);

        _homeRobot
            .DoWaitForVpnStatusSubtitleLabel();
    }
}