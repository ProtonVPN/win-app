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

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
[Category("ARM")]
public class SupportTests : FreshSessionSetUp
{
    [Test]
    public void SendBugReportViaLoginScreen()
    {
        LoginRobot.NavigateToBugReport();
        SupportRobot
            .FillBugReportForm("Connecting to VPN")
            .TickIncludeLogsCheckbox()
            .Verify.IsNoLogsAttachedWarningDisplayed()
            .SendBugReport()
            .Verify.IsSendingSuccessful();
    }

    [Test]
    public void SendBugReportViaKebabMenuFreeUser()
    {
        CommonUiFlows.FullLogin(TestUserData.FreeUser);
        HomeRobot.ExpandKebabMenuButton()
            .ClickOnHelpButton();
        SupportRobot.FillBugReportForm("Connecting to VPN")
            .SendBugReport()
            .Verify.IsSendingSuccessful();
    }

    [Test]
    public void SendBugReportViaSettings()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        SettingRobot.OpenSettings()
            .OpenBugReportSetting();
        SupportRobot.FillBugReportForm("Connecting to VPN")
            .SendBugReport()
            .Verify.IsSendingSuccessful();
    }
}