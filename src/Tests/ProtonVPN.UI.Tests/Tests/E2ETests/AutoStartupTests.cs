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

using System.Drawing;
using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class AutoStartupTests : FreshSessionSetUp
{
    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Property("TestCaseId", "602380")]
    public void AutoConnectionOn()
    {
        SetAutoLaunchOption(AutoLaunchOption.OpenOnDesktop);
        SettingRobot
            .Verify.IsAutoConnectEnabled()
            .ApplySettings();

        RestartApp();

        NavigationRobot
            .Verify.IsOnMainPage();

        HomeRobot
            .Verify.IsConnected();
    }

    [Test]
    [Property("TestCaseId", "602379")]
    public void AutoConnectionOff()
    {
        SetAutoLaunchOption(AutoLaunchOption.OpenOnDesktop);
        SettingRobot
            .DisableAutoConnectionSetting()
            .ApplySettings();

        RestartApp();

        NavigationRobot
            .Verify.IsOnMainPage();

        //wait to see that it doesnt reconnect
        Thread.Sleep(TestConstants.TenSecondsTimeout);
        HomeRobot
            .Verify.IsDisconnected();
    }

    [Test]
    [Property("TestCaseId", "602435")]
    public void AutoLaunchOpenOnDesktop()
    {
        SetAutoLaunchOption(AutoLaunchOption.OpenOnDesktop);
        SettingRobot.ApplySettings();

        RestartApp();

        TrayRobot
            .Verify.IsTrayIconDisplayed()
                   .IsHomeFocused(true);
    }

    [Test]
    [Property("TestCaseId", "602434")]
    public void AutoLaunchMinimizeToSystemTray()
    {
        SetAutoLaunchOption(AutoLaunchOption.MinimizeToSystemTray);
        SettingRobot
            .Verify.IsApplyButtonMissing();

        RestartApp(shouldRefreshWindow: false);

        TrayRobot
            .Verify.IsTrayIconDisplayed()
                   .IsHomeFocused(false)
            .DoubleClickTrayApp()
            .Verify.IsHomeFocused(true);
    }

    [Test]
    [Property("TestCaseId", "602436")]
    public void AutoLaunchTurnedOff()
    {
        WindowsUtils.AssertVpnRunsOnStartup(shouldRunOnStartup: true);

        SettingRobot
            .OpenSettings()
            .OpenAutoStartupSettings()
            .DisableAutoLaunchSetting()
            .ApplySettings();

        WindowsUtils.AssertVpnRunsOnStartup(shouldRunOnStartup: false);
    }

    [Test]
    [Property("TestCaseId", "658368")]
    public void AppPositionAndSizeAfterSignOut()
    {
        SetAutoLaunchOption(AutoLaunchOption.OpenOnDesktop);
        SettingRobot.ApplySettings();

        HomeRobot.MaximizeClientSizeViaMaximizeButton();
        TestContext.WriteLine("Checking with Maximize");
        VerifyAppSizeAndPosition(ExitMethod.ReLogin);

        HomeRobot.RestoreClientSizeViaRestoreButton();

        UiActions.ResizeAndRepositionWindow();
        TestContext.WriteLine("Checking with Custom Position");
        VerifyAppSizeAndPosition(ExitMethod.ReLogin);
    }

    [Test]
    [Property("TestCaseId", "658367")]
    public void AppPositionAndSizeAfterExit()
    {
        SetAutoLaunchOption(AutoLaunchOption.OpenOnDesktop);
        SettingRobot.ApplySettings();

        HomeRobot.MaximizeClientSizeViaMaximizeButton();
        TestContext.WriteLine("Checking with Maximize");
        VerifyAppSizeAndPosition(ExitMethod.Restart);

        HomeRobot.RestoreClientSizeViaRestoreButton();

        UiActions.ResizeAndRepositionWindow();
        TestContext.WriteLine("Checking with custom position");
        VerifyAppSizeAndPosition(ExitMethod.Restart);
    }

    private static void VerifyAppSizeAndPosition(ExitMethod exitMethod)
    {
        (Point Position, Size Size) beforeRestart = UiActions.GetWindowSizeAndPosition();

        if (exitMethod == ExitMethod.Restart)
        {
            RestartApp();
        }
        else if (exitMethod == ExitMethod.ReLogin)
        {
            CommonUiFlows.Logout();
            LoginRobot.Login(TestUserData.PlusUser);
        }

        NavigationRobot
            .Verify.IsOnMainPage();

        (Point Position, Size Size) afterRestart = UiActions.GetWindowSizeAndPosition();

        bool hasSamePosition = beforeRestart.Position.X == afterRestart.Position.X && beforeRestart.Position.Y == afterRestart.Position.Y;
        bool hasSameSize = beforeRestart.Size.Width == afterRestart.Size.Width && beforeRestart.Size.Height == afterRestart.Size.Height;

        Assert.That(hasSamePosition, Is.True, "Window position changed." +
            $"Before with {exitMethod}: X: {beforeRestart.Position.X}, Y: {beforeRestart.Position.Y}" +
            $"After with {exitMethod}: X: {afterRestart.Position.X}, Y: {afterRestart.Position.Y}");

        Assert.That(hasSameSize, Is.True, "Window size changed." +
              $"Before with {exitMethod}: Width: {beforeRestart.Size.Width}, Height: {beforeRestart.Size.Height}" +
              $"After with {exitMethod}: Width: {afterRestart.Size.Width}, Height: {afterRestart.Size.Height}");
    }

    private static void SetAutoLaunchOption(AutoLaunchOption autoLaunchOption)
    {
        SettingRobot
            .OpenSettings()
            .OpenAutoStartupSettings()
            .SelectAutoLaunchSetting(autoLaunchOption);
    }
}