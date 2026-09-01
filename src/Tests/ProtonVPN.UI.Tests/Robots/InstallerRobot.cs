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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using Microsoft.Win32;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums.Installer;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots;

public class InstallerRobot
{
    private const string INSTALLER_LANGUAGE_WINDOW = "Select Setup Language";
    private const string INSTALLER_ALREADY_INSTALLED_WINDOW = "Setup";
    private const string INSTALLER_WINDOW = "Proton VPN";
    private const string UNINSTALLER_WINDOW = "Proton VPN Uninstall";

    private const string LANGUAGE_SELECTION = "LanguageSelection.png";
    private const string FOLDER_SELECTION = "FolderSelection.png";
    private const string UNIVERSAL_INSTALLER = "UniversalInstaller.png";
    private const string REGULAR_INSTALLER = "RegularInstaller.png";
    private const string IN_PROGRESS = "InProgress.png";
    private const string NEWER_VERSION_INSTALLED = "NewerVersionInstalled.png";
    private const string UNINSTALL_CONFIRMATION = "UninstallConfirmation.png";
    private const string UNINSTALL_COMPLETED = "UninstallCompleted.png";

    private static readonly string _protonVpnExePath = @"C:\Program Files\Proton\VPN\ProtonVPN.Launcher.exe";
    private static readonly string _protonMailPath = Path.Combine(TestConstants.LocalAppData, @"proton_mail\packages");
    private static readonly string _protonDriveExePath = Path.Combine(TestConstants.LocalAppData, @"Programs\Proton\Drive\ProtonDrive.exe");
    private static readonly string _protonPassPackageName = "ProtonPass";

    private static readonly string _protonVpnUninstaller = @"C:\Program Files\Proton\VPN\unins000.exe";
    private static readonly string _protonMailUninstallerCommand = $"& \"{Path.Combine(TestConstants.LocalAppData, @"proton_mail\Update.exe")}\" --uninstall";
    private static readonly string _protonPassUninstallerCommand = $"Get-AppxPackage -Name '{_protonPassPackageName}' | Remove-AppxPackage";
    private static string? ProtonDriveUninstallerCode => GetMsiProductCode("Proton Drive");

    private static readonly string _protonVpnFullUninstallScript = Path.Combine(TestConstants.SolutionRoot, @"ci\test-scripts\uninstall-app.ps1");
    private static readonly string _protonVpnFullInstallScript = Path.Combine(TestConstants.SolutionRoot, @"ci\test-scripts\install-app.ps1");

    public static void CloseRunningInstallerProcesses()
    {
        foreach (Process process in Process.GetProcesses())
        {
            try
            {
                bool isInstaller = process.ProcessName.Contains("ProtonVPN_", StringComparison.OrdinalIgnoreCase);
                bool isUninstaller = process.ProcessName.Contains("unins", StringComparison.OrdinalIgnoreCase) || process.ProcessName.Contains("_iu", StringComparison.OrdinalIgnoreCase);

                if (!isInstaller && !isUninstaller)
                {
                    continue;
                }

                process.Kill(entireProcessTree: true);
                process.WaitForExit(TestConstants.ThirtySecondsTimeout);
            }
            catch (Exception exception)
            {
                TestContext.WriteLine($"Could not close process '{process.ProcessName}': {exception.Message}");
            }
            finally
            {
                process.Dispose();
            }
        }
    }

    public InstallerRobot StartInstaller()
    {
        InstallerHelper.StartInstaller();
        return this;
    }

    public InstallerRobot StartInstallerWithCustomPath(string installerPath)
    {
        InstallerHelper.StartInstaller(installerPath);
        return this;
    }

    public InstallerRobot RunSilentInstallation()
    {
        InstallerHelper.StartInstaller(arguments: "/SILENT");
        return this;
    }

    public InstallerRobot RunVerySilentInstallation()
    {
        WindowsUtils.RunPowerShellScript(_protonVpnFullInstallScript, timeout: TestConstants.FourMinutesTimeout);
        return this;
    }

    public InstallerRobot StartUninstaller()
    {
        InstallerHelper.StartInstaller(_protonVpnUninstaller);
        return this;
    }

    public InstallerRobot RunSilentUninstall(ProtonApp protonApp)
    {
        switch (protonApp)
        {
            case ProtonApp.Vpn:
                WindowsUtils.RunPowerShellScript(_protonVpnFullUninstallScript, timeout: TestConstants.FourMinutesTimeout);
                break;
            case ProtonApp.Mail:
                WindowsUtils.RunPowerShellScript(_protonMailUninstallerCommand, timeout: TestConstants.TwoMinutesTimeout);
                break;
            case ProtonApp.Drive:
                UninstallMsiProduct(ProtonDriveUninstallerCode);
                break;
            case ProtonApp.Pass:
                WindowsUtils.RunPowerShellScript(_protonPassUninstallerCommand, timeout: TestConstants.TwoMinutesTimeout);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(protonApp), protonApp, null);
        }
        return this;
    }

    public InstallerRobot SelectLanguage()
    {
        InstallerHelper.WaitForAndFocusWindow(INSTALLER_LANGUAGE_WINDOW);
        Keyboard.Type(VirtualKeyShort.ENTER);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        return this;
    }

    public InstallerRobot UncheckApps()
    {
        InstallerHelper.WaitForAndFocusWindow(INSTALLER_WINDOW);
        if (!TestEnvironment.AreTestsRunningLocally())
        {
            Thread.Sleep(TestConstants.UserInputSimulationDelay);
            Keyboard.Type(VirtualKeyShort.TAB);
            Thread.Sleep(TestConstants.UserInputSimulationDelay);
            Keyboard.Type(VirtualKeyShort.SPACE);
        }
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        Keyboard.Type(VirtualKeyShort.TAB);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        Keyboard.Type(VirtualKeyShort.SPACE);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        Keyboard.Type(VirtualKeyShort.TAB);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        Keyboard.Type(VirtualKeyShort.SPACE);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        return this;
    }

    public InstallerRobot CloseSameVersionWindow()
    {
        InstallerHelper.WaitForAndFocusWindow(INSTALLER_ALREADY_INSTALLED_WINDOW);
        Keyboard.Type(VirtualKeyShort.ENTER);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        return this;
    }

    public InstallerRobot ClickInstall()
    {
        InstallerHelper.WaitForAndFocusWindow(INSTALLER_WINDOW);
        Keyboard.Type(VirtualKeyShort.ENTER);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        return this;
    }

    public InstallerRobot ConfirmUninstall()
    {
        InstallerHelper.WaitForAndFocusWindow(UNINSTALLER_WINDOW);
        Keyboard.Type(VirtualKeyShort.TAB);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        Keyboard.Type(VirtualKeyShort.ENTER);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        return this;
    }

    public InstallerRobot CloseUninstallationCompletedWindow()
    {
        InstallerHelper.WaitForAndFocusWindow(UNINSTALLER_WINDOW);
        Keyboard.Type(VirtualKeyShort.ENTER);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        return this;
    }

    public InstallerRobot TrySkipFolderSelection()
    {
        bool isFolderSelectionDisplayed = InstallerHelper.IsWindowMatchingScreenshot(INSTALLER_WINDOW, FOLDER_SELECTION, out _, out _);

        if (isFolderSelectionDisplayed)
        {
            ClickNext();
        }
        else
        {
            Thread.Sleep(TestConstants.UserInputSimulationDelay);
            Keyboard.Type(VirtualKeyShort.TAB);
        }

        return this;
    }

    private InstallerRobot ClickNext()
    {
        InstallerHelper.WaitForAndFocusWindow(INSTALLER_WINDOW);
        Keyboard.Type(VirtualKeyShort.ENTER);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        return this;
    }

    private static string? GetMsiProductCode(string displayName)
    {
        using RegistryKey uninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")!;

        foreach (string subKeyName in uninstallKey.GetSubKeyNames())
        {
            using RegistryKey? key = uninstallKey.OpenSubKey(subKeyName);

            if ((key?.GetValue("DisplayName") as string) == displayName)
            {
                return subKeyName;
            }
        }
        return null;
    }

    private static void UninstallMsiProduct(string? productCode, bool isSilent = true)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            return;
        }

        StringBuilder arguments = new();

        arguments.Append($"/x {productCode}");

        if (isSilent)
        {
            arguments.Append(" /qn");
        }

        using Process process = Process.Start("msiexec.exe", arguments.ToString().Trim())!;

        if (!process.WaitForExit(TestConstants.ThirtySecondsTimeout))
        {
            TestContext.WriteLine("MSI uninstall timed out.");
            process.Kill(entireProcessTree: true);
        }
    }

    public class Verifications : InstallerRobot
    {
        public Verifications IsNewerVersionInstalledMessageDisplayed()
        {
            InstallerHelper.VerifyScreenshot(
                INSTALLER_ALREADY_INSTALLED_WINDOW,
                NEWER_VERSION_INSTALLED);
            return this;
        }

        public Verifications IsLanguageSelectionDisplayed()
        {
            InstallerHelper.VerifyScreenshot(
                INSTALLER_LANGUAGE_WINDOW,
                LANGUAGE_SELECTION,
                requiredSimilarity: 0.85);
            return this;
        }

        public Verifications IsUniversalInstallerDisplayed()
        {
            InstallerHelper.VerifyScreenshot(
                INSTALLER_WINDOW,
                UNIVERSAL_INSTALLER);
            return this;
        }

        public Verifications IsRegularInstallerDisplayed()
        {
            InstallerHelper.VerifyScreenshot(
                INSTALLER_WINDOW,
                REGULAR_INSTALLER);
            return this;
        }

        public Verifications IsInstallationInProgress()
        {
            InstallerHelper.VerifyScreenshotNonFatal(
                INSTALLER_WINDOW,
                IN_PROGRESS);
            return this;
        }

        public Verifications IsInstallationCompleted()
        {
            InstallerHelper.WaitForWindowToDisappear();
            return this;
        }

        public Verifications IsUninstallerDisplayed()
        {
            InstallerHelper.VerifyScreenshot(
                UNINSTALLER_WINDOW,
                UNINSTALL_CONFIRMATION,
                requiredSimilarity: 0.85);
            return this;
        }

        public Verifications IsUninstallationCompleted()
        {
            Thread.Sleep(TestConstants.TenSecondsTimeout);
            InstallerHelper.VerifyScreenshot(
                UNINSTALLER_WINDOW,
                UNINSTALL_COMPLETED,
                requiredSimilarity: 0.85);
            return this;
        }

        public Verifications IsAppInstalled(ProtonApp protonApp)
        {
            Retry.WhileFalse(
                () => IsInstalled(protonApp),
                TestConstants.TwoMinutesTimeout,
                throwOnTimeout: true,
                ignoreException: true,
                interval: TestConstants.EighteenSecondsTimeout,
                timeoutMessage: $"{protonApp} is not installed");

            return this;
        }

        public Verifications IsAppNotInstalled(ProtonApp protonApp)
        {
            Retry.WhileTrue(
                () => IsInstalled(protonApp),
                TestConstants.TwoMinutesTimeout,
                throwOnTimeout: true,
                ignoreException: true,
                interval: TestConstants.EighteenSecondsTimeout,
                timeoutMessage: $"{protonApp} is still installed");

            return this;
        }
    }

    public static bool IsInstalled(ProtonApp protonApp)
    {
        return protonApp switch
        {
            ProtonApp.Vpn => File.Exists(_protonVpnExePath),
            ProtonApp.Mail => Directory.Exists(_protonMailPath),
            ProtonApp.Drive => File.Exists(_protonDriveExePath),
            ProtonApp.Pass => IsMsixInstalled(_protonPassPackageName),
            _ => throw new ArgumentOutOfRangeException(nameof(protonApp), protonApp, null)
        };
    }

    private static bool IsMsixInstalled(string packageName)
    {
        string output = WindowsUtils.RunPowerShellScript($"Get-AppxPackage -Name '{packageName}' | Select-Object -ExpandProperty PackageFullName");
        return !string.IsNullOrWhiteSpace(output);
    }

    public Verifications Verify => new();
}