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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using Microsoft.Win32;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ProtonVPN.UI.Tests.TestsHelper;
using TimeoutException = System.TimeoutException;

namespace ProtonVPN.UI.Tests;

public class TestSession
{
    protected static Application App;
    protected static Application Service;
    protected static Window Window;

    private const int MAX_APP_START_TRIES = 10;
    private const string CLIENT_NAME = "ProtonVPN.Client.exe";

    private static readonly bool _isDevelopmentModeEnabled = false;

    public static void RefreshWindow(TimeSpan? timeout = null)
    {
        Window = null;
        TimeSpan refreshTimeout = timeout ?? TestConstants.MediumTimeout;
        RetryResult<Window> retry = Retry.WhileNull(() =>
        {
            try
            {
                Window = App.GetMainWindow(new UIA3Automation(), refreshTimeout);
            }
            catch (TimeoutException)
            {
                //Ignore
            }
            return Window;
        },
        refreshTimeout, TestConstants.RetryInterval);

        if (!retry.Success)
        {
            Assert.Fail($"Failed to refresh window in {refreshTimeout.Seconds} seconds.");
        }
    }

    protected static void DeleteProtonData()
    {
        try
        {
            Directory.Delete(TestConstants.UserStoragePath, true);
            Directory.Delete(Path.Combine(GetProtonClientFolder(), "ServiceData", "Logs"), true);
        }
        catch
        {
        }
    }

    protected static void Cleanup()
    {
        try
        {
            SaveScreenshotAndLogsIfFailed();
        }
        catch
        {
            //Do nothing, since artifact collection shouldn't block cleanup.
        }
        App.Kill();
        App.Dispose();
        StopService("ProtonVPN Service");
        StopService("ProtonVPN Wireguard");
    }

    protected static void LaunchApp()
    {
        DeleteProtonData();

        if (_isDevelopmentModeEnabled)
        {
            LaunchDevelopmentApp();
            return;
        }
        
        string installedClientPath = Path.Combine(GetProtonClientFolder(), CLIENT_NAME);
        App = Application.Launch(installedClientPath);
        RetryResult<bool> result = WaitUntilAppIsRunning();
        if (!result.Success)
        {
            //Sometimes app fails to launch on first try due to CI issues.
            App = Application.Launch(installedClientPath);
        }

        RefreshWindow(TestConstants.LongTimeout);
        Window.WaitUntilClickable(TestConstants.ShortTimeout);
        Window.Focus();
    }

    protected static void KillProtonVpnProcess()
    {
        Process.GetProcesses()
            .Where(process => process.ProcessName.StartsWith("ProtonVPN"))
            .ToList()
            .ForEach(process => process.Kill());
    }

    protected static void RestartFileExplorer()
    {
        foreach (Process exe in Process.GetProcesses())
        {
            if (exe.ProcessName == "explorer")
            {
                exe.Kill();
            }
        }
    }

    protected static void StartFileExplorer()
    {
        Process[] explorrerProcess = Process.GetProcessesByName("explorer");
        if (explorrerProcess.Length == 0)
        {
            string explorer = string.Format("{0}\\{1}", Environment.GetEnvironmentVariable("WINDIR"), "explorer.exe");
            Process process = new();
            process.StartInfo.FileName = explorer;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
    }

    private static string GetProtonClientFolder()
    {
        string registryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Proton VPN_is1";
        RegistryKey localMachineRegistry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        RegistryKey key = localMachineRegistry.OpenSubKey(registryKeyPath);

        object displayVersionObject = key?.GetValue("DisplayVersion");
        displayVersionObject?.ToString();
        string versionFolder = $"v{displayVersionObject?.ToString()}";
        return Path.Combine(TestConstants.AppFolderPath, versionFolder);
    }

    private static void LaunchDevelopmentApp()
    {
        string executingAssemblyFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        string applicationPath = Path.Combine(executingAssemblyFolderPath, CLIENT_NAME);

        App = Application.Launch(applicationPath);
        RefreshWindow();
        Window.WaitUntilClickable();
        Window.Focus();
    }

    private static void SaveScreenshotAndLogsIfFailed()
    {
        if (!TestEnvironment.AreTestsRunningLocally())
        {
            TestStatus status = TestContext.CurrentContext.Result.Outcome.Status;
            string testName = TestContext.CurrentContext.Test.MethodName;
            if (status == TestStatus.Failed)
            {
                TestsRecorder.SaveScreenshotAndLogs(testName, GetProtonClientFolder());
            }
        }
    }

    private static RetryResult<bool> WaitUntilAppIsRunning()
    {
        RetryResult<bool> retry = Retry.WhileFalse(
            () => {
                Process[] pname = Process.GetProcessesByName("ProtonVPN.Client");
                return pname.Length > 0;
            },
            TimeSpan.FromSeconds(30), TestConstants.RetryInterval);

        return retry;
    }

    private static void StopService(string serviceName)
    {
        try
        {
            ServiceController service = new ServiceController(serviceName);
            service.Stop();
        }
        catch (InvalidOperationException)
        {
            //Ignore because service might not be started.
        }
    }
}