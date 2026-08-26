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

using System;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using FlaUI.Core.AutomationElements;
using NUnit.Framework;
using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;
using ProtonVPN.UI.Tests.TestsHelper.TestData;

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
            .FillEmailInBugReportForm()
            .FillOtherFieldsInBugReportForm()
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
            .FillEmailInBugReportForm()
            .FillOtherFieldsInBugReportForm()
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
            .FillEmailInBugReportForm()
            .FillOtherFieldsInBugReportForm()
            .SendBugReport()
            .Verify.IsSendingSuccessful();
    }

    [Test]
    [Property("TestCaseId", "890321")]
    [Ignore("JIRA - VPNWIN-3344")]
    public void BugReportWithMaxChars()
    {
        LoginRobot
            .NavigateToBugReport();
        SupportRobot
            .SelectBugType(_reportWeakConnection)
            .ClickContactUs();

        (Point Position, Size Size) emailBefore = UiActions.GetElementSizeAndPosition(SupportRobot.EmailInputField);

        List<(Point Position, Size Size)> otherFieldsBefore = GetElementsSizeAndPosition(SupportRobot.GetOtherFieldsElements);

        UiActions.VerifyElementSizeAndPosition(SupportRobot.EmailInputField, emailBefore, () =>
            SupportRobot
                .FillEmailInBugReportForm(InputTestData.LongInput)
        );

        VerifyElementsSizeAndPosition(SupportRobot.GetOtherFieldsElements, otherFieldsBefore, () =>
            SupportRobot
                .FillOtherFieldsInBugReportForm(InputTestData.LongInput)
        );

        SupportRobot
            .ScrollUpAndDown()
            .FillEmailInBugReportForm()
            .TickIncludeLogsCheckbox()
            .Verify.IsNoLogsAttachedWarningDisplayed()
            .SendBugReport()
            .Verify.IsSendingSuccessful();

        HomeRobot.CloseClientViaCloseButton();
        Thread.Sleep(TestConstants.TenSecondsTimeout);
        CommonAssertions.VerifyAppIsNotRunning();
    }

    public static List<(Point Position, Size Size)> GetElementsSizeAndPosition(Func<AutomationElement[]> elementsFunc)
    {
        Thread.Sleep(TestConstants.TwoSecondsTimeout);

        AutomationElement[] elements = elementsFunc();

        return elements.Select(e => (
            Position: new Point(e.BoundingRectangle.X, e.BoundingRectangle.Y),
            Size: new Size(e.BoundingRectangle.Width, e.BoundingRectangle.Height))).ToList();
    }

    public static void VerifyElementsSizeAndPosition(
        Func<AutomationElement[]> elementsFunc,
        List<(Point Position, Size Size)> elementsBeforeTyping,
        Action typeAction,
        int sizeTolerancePx = 50,
        int positionTolerancePx = 100)
    {
        typeAction();

        AutomationElement[] elementsAfterTyping = elementsFunc();

        Assert.That(elementsAfterTyping.Length, Is.EqualTo(elementsBeforeTyping.Count), "Number of elements changed after typing.");

        for (int i = 0; i < elementsBeforeTyping.Count; i++)
        {
            Rectangle rect = elementsAfterTyping[i].BoundingRectangle;
            Point positionAfter = new(rect.X, rect.Y);
            Size sizeAfter = new(rect.Width, rect.Height);

            bool hasSamePosition = Math.Abs(elementsBeforeTyping[i].Position.X - positionAfter.X) <= positionTolerancePx && Math.Abs(elementsBeforeTyping[i].Position.Y - positionAfter.Y) <= positionTolerancePx;
            bool hasSameSize = Math.Abs(elementsBeforeTyping[i].Size.Width - sizeAfter.Width) <= sizeTolerancePx && Math.Abs(elementsBeforeTyping[i].Size.Height - sizeAfter.Height) <= sizeTolerancePx;

            Assert.That(hasSamePosition, Is.True, $"Element[{i}] position changed beyond tolerance ({positionTolerancePx}px)." +
                $"Before: X: {elementsBeforeTyping[i].Position.X}, Y: {elementsBeforeTyping[i].Position.Y}" +
                $"After: X: {positionAfter.X}, Y: {positionAfter.Y}");

            Assert.That(hasSameSize, Is.True, $"Element[{i}] size changed beyond tolerance ({sizeTolerancePx}px)." +
                $"Before: Width: {elementsBeforeTyping[i].Size.Width}, Height: {elementsBeforeTyping[i].Size.Height}" +
                $"After: Width: {sizeAfter.Width}, Height: {sizeAfter.Height}");
        }
    }

    [TearDown]
    public void TearDown()
    {
        BrowserUtils.KillAllBrowsers();
    }
}