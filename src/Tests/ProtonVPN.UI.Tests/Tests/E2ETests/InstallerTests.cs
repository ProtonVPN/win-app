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
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Enums.Installer;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("INSTALLER")]
public class InstallerTests : BaseTest
{
    [SetUp]
    public void Setup()
    {
        InstallerRobot.CloseRunningInstallerProcesses();

        try
        {
            InstallerRobot
                .RunSilentUninstall(ProtonApp.Vpn)
                .RunSilentUninstall(ProtonApp.Mail)
                .RunSilentUninstall(ProtonApp.Drive)
                .RunSilentUninstall(ProtonApp.Pass);
        }
        catch (Exception exception)
        {
            TestContext.WriteLine($"Setup cleanup failed: {exception}");
        }

        //Allow it some time
        Thread.Sleep(TestConstants.EighteenSecondsTimeout);

        InstallerRobot
            .Verify.IsAppNotInstalled(ProtonApp.Vpn)
            .Verify.IsAppNotInstalled(ProtonApp.Mail)
            .Verify.IsAppNotInstalled(ProtonApp.Drive)
            .Verify.IsAppNotInstalled(ProtonApp.Pass);
    }

    [Test, Order(0)]
    [Property("TestCaseId", "602351")]
    public void FreshInstallationOfTheApp()
    {
        InstallerRobot
            .StartInstaller()
            .Verify.IsLanguageSelectionDisplayed()
            .SelectLanguage()
            .TrySkipFolderSelection()
            .Verify.IsUniversalInstallerDisplayed()
            .UncheckApps()
            .ClickInstall()
            .Verify.IsInstallationInProgress()
            .Verify.IsInstallationCompleted();

        InstallerRobot
            .Verify.IsAppInstalled(ProtonApp.Vpn)
            .Verify.IsAppNotInstalled(ProtonApp.Mail)
            .Verify.IsAppNotInstalled(ProtonApp.Drive)
            .Verify.IsAppNotInstalled(ProtonApp.Pass);

        AttachToAppAndCloseIt();
    }

    [Test, Order(1)]
    [Property("TestCaseId", "602377")]
    public void SilentInstallationInstallsOnlyVpn()
    {
        InstallerRobot
            .RunSilentInstallation()
            .Verify.IsInstallationCompleted();

        //Allow it some time
        Thread.Sleep(TestConstants.EighteenSecondsTimeout);

        InstallerRobot
            .Verify.IsAppInstalled(ProtonApp.Vpn)
            .Verify.IsAppNotInstalled(ProtonApp.Mail)
            .Verify.IsAppNotInstalled(ProtonApp.Drive)
            .Verify.IsAppNotInstalled(ProtonApp.Pass);
    }

    [Test, Order(2)]
    [Property("TestCaseId", "602350")]
    public void InstallingOlderVersionFails()
    {
        string olderInstallerPath = GetVersionsHelper.GetOldVersionPath();

        InstallerRobot
            .RunVerySilentInstallation()
            .Verify.IsAppInstalled(ProtonApp.Vpn);

        InstallerRobot
            .StartInstallerWithCustomPath(olderInstallerPath)
            .SelectLanguage()
            .Verify.IsNewerVersionInstalledMessageDisplayed()
            .CloseSameVersionWindow();
    }

    [Test, Order(3)]
    [Property("TestCaseId", "602349")]
    public void InstallSameVersionOfTheApp()
    {
        InstallerRobot
            .RunVerySilentInstallation()
            .Verify.IsAppInstalled(ProtonApp.Vpn);

        //Allow it some time
        Thread.Sleep(TestConstants.ThirtySecondsTimeout);

        InstallerRobot
            .StartInstaller()
            .SelectLanguage()
            .UncheckApps()
            .ClickInstall()
            .Verify.IsInstallationInProgress()
            .Verify.IsInstallationCompleted();

        InstallerRobot
            .Verify.IsAppInstalled(ProtonApp.Vpn)
            .Verify.IsAppNotInstalled(ProtonApp.Mail)
            .Verify.IsAppNotInstalled(ProtonApp.Drive)
            .Verify.IsAppNotInstalled(ProtonApp.Pass);

        try
        {
            AttachToAppAndCloseIt();
        }
        catch { }
    }

    [Test, Order(4)]
    [Property("TestCaseId", "602375")]
    public void InstalledProtonAppsAreNotDisplayedInUniversalInstaller()
    {
        //temp
        string protonMailPath = Path.Combine(TestConstants.LocalAppData, @"proton_mail");
        if (Directory.Exists(protonMailPath))
        {
            Directory.Delete(protonMailPath, recursive: true);
        }
        //

        InstallerRobot
            .StartInstaller()
            .SelectLanguage()
            .TrySkipFolderSelection()
            .Verify.IsUniversalInstallerDisplayed()
            .ClickInstall()
            .Verify.IsInstallationInProgress()
            .Verify.IsInstallationCompleted();

        //Allow it some time
        //Thread.Sleep(TestConstants.OneMinuteTimeout);

        //temp
        Thread.Sleep(TestConstants.OneMinuteTimeout);
        Thread.Sleep(TestConstants.TwoMinutesTimeout);
        TestContext.WriteLine($"Contents of '{protonMailPath}':");
        foreach (string entry in Directory.GetFileSystemEntries(protonMailPath))
        {
            TestContext.WriteLine($"  {entry}");
        }
        //

        InstallerRobot
            .Verify.IsAppInstalled(ProtonApp.Vpn)
            //.Verify.IsAppInstalled(ProtonApp.Mail) //temp
            .Verify.IsAppInstalled(ProtonApp.Drive)
            .Verify.IsAppInstalled(ProtonApp.Pass);

        AttachToAppAndCloseIt();

        InstallerRobot
            .StartInstaller()
            .SelectLanguage()
            .Verify.IsRegularInstallerDisplayed();
    }

    [Test, Order(5)]
    [Property("TestCaseId", "602348")]
    public void UninstallingTheApp()
    {
        InstallerRobot
            .RunVerySilentInstallation()
            .Verify.IsAppInstalled(ProtonApp.Vpn);

        DesktopRobot
            .PinAppToTaskbar()
            .Verify.IsAppPinnedToTaskbar();

        LaunchClient();

        //Allow it some time
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        DesktopRobot.Verify.IsTrayIconDisplayed();

        InstallerRobot
            .StartUninstaller()
            .Verify.IsUninstallerDisplayed()
            .ConfirmUninstall()
            .Verify.IsUninstallationCompleted()
            .CloseUninstallationCompletedWindow();

        InstallerRobot
            .Verify.IsAppNotInstalled(ProtonApp.Vpn);

        DesktopRobot
            .Verify.IsAppNotPinnedToTaskbar()
            .IsTrayIconNotDisplayed();
    }

    private static void AttachToAppAndCloseIt()
    {
        //Allow it some time
        Thread.Sleep(TestConstants.EighteenSecondsTimeout);

        Process process = Retry.WhileNull(
            () => Process.GetProcessesByName("ProtonVPN.Client").FirstOrDefault(),
            TestConstants.OneMinuteTimeout,
            interval: TestConstants.TenSecondsTimeout).Result ?? throw new InvalidOperationException("ProtonVPN.Client did not start within 60s after installation.");

        App = Application.Attach(process);
        Cleanup();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        //so the next group of tests have the app installed
        if (!InstallerRobot.IsInstalled(ProtonApp.Vpn))
        {
            InstallerRobot.CloseRunningInstallerProcesses();
            InstallerRobot.RunVerySilentInstallation();
        }
    }
}