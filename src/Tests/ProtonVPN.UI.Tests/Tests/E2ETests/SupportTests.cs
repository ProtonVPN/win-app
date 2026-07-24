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
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
[Category("ARM")]
public class SupportTests : FreshSessionSetUp
{
    private static readonly string _reportConnectingToVpn = ApiTranslationHelper.GetTranslatedString("ReportIssue_ConnectingToVpn");
    private static readonly string _reportBrowsingSpeed = ApiTranslationHelper.GetTranslatedString("ReportIssue_BrowsingSpeed");
    private static readonly string _reportWeakConnection = ApiTranslationHelper.GetTranslatedString("ReportIssue_WeakConnection");

    [Test]
    [Property("TestCaseId", "602385")]
    [Retry(3)]
    public void SendBugReportViaLoginScreen()
    {
        LoginRobot
            .NavigateToBugReport();
        SupportRobot
            .SelectBugType(_reportConnectingToVpn)
            .ClickContactUs()
            .FillBugReportForm()
            .TickIncludeLogsCheckbox()
            .Verify.IsNoLogsAttachedWarningDisplayed()
            .SendBugReport()
            .Verify.IsSendingSuccessful();
    }

    [Test]
    [Property("TestCaseId", "602384")]
    [Retry(3)]
    public void SendBugReportViaKebabMenuFreeUser()
    {
        CommonUiFlows.FullLogin(TestUserData.FreeUser);

        HomeRobot
            .ExpandKebabMenuButton()
            .ClickOnHelpButton();
        SupportRobot
            .SelectBugType(_reportBrowsingSpeed)
            .ClickContactUs()
            .FillBugReportForm()
            .SendBugReport()
            .Verify.IsSendingSuccessful();
    }

    [Test]
    [Property("TestCaseId", "602386")]
    [Retry(3)]
    [Category("SMOKE_1")]
    public void SendBugReportViaSettings()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);

        SettingRobot
            .OpenSettings()
            .OpenBugReportSetting();
        SupportRobot
            .SelectBugType(_reportWeakConnection)
            .ClickContactUs()
            .FillBugReportForm()
            .SendBugReport()
            .Verify.IsSendingSuccessful();
    }

    [TearDown]
    public void TearDown()
    {
        BrowserUtils.KillAllBrowsers();
    }
}