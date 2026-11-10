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
using System.Linq;
using System.Threading;
using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Enums.Installer;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("INSTALLER")]
public class UpdateTests : BaseTest
{
    private string? _newInstallerPath = null;
    private string? _currentVersion = null;
    private readonly string _newVersion = GetVersionsHelper.NEW_VERSION;
    private readonly string _oldVersion = GetVersionsHelper.OLD_VERSION;

    private const KillSwitchMode NON_DEFAULT_KILL_SWITCH_MODE = KillSwitchMode.Standard;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        if (!InstallerRobot.IsInstalled(ProtonApp.Vpn))
        {
            InstallerRobot.CloseRunningInstallerProcesses();
            InstallerRobot.RunVerySilentInstallation();
        }

        _currentVersion = TestEnvironment.GetAppVersion();
        Assert.That(_currentVersion, Is.Not.Null);

        _newInstallerPath = GetVersionsHelper.GetNewVersionPath(_currentVersion);
        Assert.That(_newInstallerPath, Is.Not.Null);
    }

    [SetUp]
    public void Setup()
    {
        InstallerRobot.CloseRunningInstallerProcesses();

        try
        {
            InstallerRobot.RunSilentUninstall(ProtonApp.Vpn);
        }
        catch (Exception exception)
        {
            TestContext.WriteLine($"Setup cleanup failed: {exception}");
        }

        //Allow it some time
        Thread.Sleep(TestConstants.EighteenSecondsTimeout);

        InstallerRobot.Verify.IsAppNotInstalled(ProtonApp.Vpn);
    }

    [Test, Order(0)]
    [Property("TestCaseId", "602352")]
    [Ignore("JIRA - VPNWIN-3396")]
    public void UpdateFromCurrentToNewerVersion()
    {
        InstallerRobot
            .RunVerySilentInstallation()
            .Verify.IsAppInstalled(ProtonApp.Vpn);

        DesktopRobot
            .PinAppToTaskbar()
            .Verify.IsAppPinnedToTaskbar();

        LaunchClient();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        HomeRobot.CloseClientViaCloseButton();

        InstallAppFullFlow(_newInstallerPath!, _newVersion, skipFolderSelection: true);

        TrayRobot.DoubleClickTrayApp();

        HomeRobot.Verify.IsConnected();
        CheckAppVersion(_currentVersion!);
        ReopenAppFromTaskbarAndTray();

        Cleanup();
        DesktopRobot.LaunchAppFromTaskbar();
        AttachToApp();

        TrayRobot.DoubleClickTrayApp();
        HomeRobot.Verify.IsConnected();
        CheckAppVersion(_newVersion);
        ReopenAppFromTaskbarAndTray();

        Cleanup();
    }

    [Test, Order(1)]
    [Property("TestCaseId", "610979")]
    public void UpdateFromOlderToCurrentVersion()
    {
        string olderInstallerPath = GetVersionsHelper.GetOldVersionPath();

        InstallAppFullFlow(olderInstallerPath, _oldVersion, skipFolderSelection: false);
        AttachToApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser, isProTunVersion: false);
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        InstallerRobot
            .RunVerySilentInstallation()
            .Verify.IsAppInstalled(ProtonApp.Vpn);
        Assert.That(TestEnvironment.GetAppVersion(), Does.Contain(_currentVersion));

        HomeRobot.Verify.IsConnected();
        CheckAppVersion(_oldVersion);

        RestartApp(shouldRefreshWindow: false);
        TrayRobot.DoubleClickTrayApp();
        HomeRobot.Verify.IsConnected();
        CheckAppVersion(_currentVersion!);
        CommonUiFlows.EnsureUserIsDisconnected();
    }

    [Test, Order(2)]
    [Property("TestCaseId", "610980")]
    public void ProfilesAndPreferencesMigrationAfterUpdate()
    {
        InstallerRobot
            .RunVerySilentInstallation()
            .Verify.IsAppInstalled(ProtonApp.Vpn);

        LaunchClient();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        CheckAppVersion(_currentVersion!);
        SetNonDefaultSettings();
        Cleanup();

        InstallAppFullFlow(_newInstallerPath!, _newVersion, skipFolderSelection: true);
        LaunchClient(ClientLaunchParams.StartWithNoOnboardingNoRefresh);
        Thread.Sleep(TestConstants.TenSecondsTimeout);
        TrayRobot.DoubleClickTrayApp();

        AttachToApp();
        try
        {
            HomeRobot.DismissWelcomeModal();
        }
        catch { }

        CheckAppVersion(_newVersion);
        VerifyNonDefaultSettings();
        CommonUiFlows.EnsureUserIsDisconnected();
    }

    private static void AttachToApp()
    {
        //Allow it some time
        Thread.Sleep(TestConstants.EighteenSecondsTimeout);

        Process process = Retry.WhileNull(
            () => Process.GetProcessesByName("ProtonVPN.Client").FirstOrDefault(),
            TestConstants.OneMinuteTimeout,
            interval: TestConstants.TenSecondsTimeout).Result ?? throw new InvalidOperationException("ProtonVPN.Client did not start within 60s after installation.");

        App = Application.Attach(process);
    }

    private static void InstallAppFullFlow(string customPath, string expectedVersion, bool skipFolderSelection)
    {
        InstallerRobot
            .StartInstallerWithCustomPath(customPath)
            .SelectLanguage();

        if (!skipFolderSelection)
        {
            InstallerRobot.TrySkipFolderSelection();
        }

        InstallerRobot
            .UncheckApps()
            .ClickInstall()
            .Verify.IsInstallationInProgress()
            .Verify.IsInstallationCompleted();

        InstallerRobot
            .Verify.IsAppInstalled(ProtonApp.Vpn);

        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        Assert.That(TestEnvironment.GetAppVersion(), Does.Contain(expectedVersion));
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
    }

    private static void ReopenAppFromTaskbarAndTray()
    {
        HomeRobot.CloseClientViaCloseButton();
        DesktopRobot.LaunchAppFromTaskbar();
        TrayRobot.Verify.IsHomeFocused(true);

        HomeRobot.CloseClientViaCloseButton();
        TrayRobot.DoubleClickTrayApp();
    }

    private static void CheckAppVersion(string expectedVersion)
    {
        SettingRobot
            .OpenSettings()
            .ScrollToAboutSection()
            .Verify.IsCorrectAppVersionDisplayedInAboutSettingsCard(expectedVersion)
            .CloseSettings();
    }

    private static void SetNonDefaultSettings()
    {
        SettingRobot
            .OpenSettings()
            .OpenProtocolSettings()
            .DisableProtunToggle()
            .ApplySettings()
            .OpenNetShieldSettings()
            .DisableNetShieldToggle()
            .ApplySettings()
            .OpenKillSwitchSettings()
            .EnableKillSwitchToggle()
            .SelectKillSwitchMode(NON_DEFAULT_KILL_SWITCH_MODE)
            .ApplySettings()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .DisableLanToggle();
        SettingRobot
            .ApplySettings()
            .DisableNotificationsToggle()
            .CloseSettings();
    }

    private static void VerifyNonDefaultSettings()
    {
        SettingRobot
            .OpenSettings()
            .OpenProtocolSettings()
            .Verify.IsProTunDisabled()
            .GoBack()
            .Verify.IsNetshieldDisabledStateDisplayed()
                   .IsKillSwitchEnabledStateDisplayed(NON_DEFAULT_KILL_SWITCH_MODE)
                   .AreNotificationsDisabled()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .Verify.IsLanDisabled();
        SettingRobot
            .CloseSettings();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        InstallerRobot.CloseRunningInstallerProcesses();

        InstallerRobot
            .RunSilentUninstall(ProtonApp.Vpn)
            .Verify.IsAppNotInstalled(ProtonApp.Vpn);

        //so the next group of tests have the app installed
        InstallerRobot.RunVerySilentInstallation();
    }
}