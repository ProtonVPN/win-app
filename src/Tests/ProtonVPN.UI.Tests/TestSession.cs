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
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
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

    private const string CLIENT_NAME = "ProtonVPN.Client.exe";

    private static readonly bool _isDevelopmentModeEnabled = false;

    public static void RefreshWindow(TimeSpan? timeout = null)
    {
        Window = null;
        TimeSpan refreshTimeout = timeout ?? TestConstants.ThirtySecondsTimeout;
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
            File.Delete(TestEnvironment.GetServiceLogsPath());
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
        App.Close();
    }

    protected static void LaunchApp(bool isFreshStart = true)
    {
        if(isFreshStart)
        {
            DeleteProtonData();
        }

        if (_isDevelopmentModeEnabled)
        {
            LaunchDevelopmentApp();
            return;
        }

        string installedClientPath = Path.Combine(TestEnvironment.GetProtonClientFolder(), CLIENT_NAME);
        ProcessStartInfo startInfo = new ProcessStartInfo(installedClientPath)
        {
            Arguments = "-ExitAppOnClose"
        };
        App = Application.Launch(startInfo);

        RetryResult<bool> result = WaitUntilAppIsRunning();
        if (!result.Success)
        {
            //Sometimes app fails to launch on first try due to CI issues.
            App = Application.Launch(startInfo);
        }

        RefreshWindow(TestConstants.OneMinuteTimeout);
        Window.WaitUntilClickable(TestConstants.TenSecondsTimeout);
        Window.Focus();
    }

    protected static void CloseProtonVPN()
    {
        Process.GetProcesses()
            .Where(process => process.ProcessName.StartsWith("ProtonVPN"))
            .ToList()
            .ForEach(process => process.Kill());
    }

    protected static void SaveScreenshotAndLogsIfFailed()
    {
        if (!TestEnvironment.AreTestsRunningLocally() && TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
        {
            string testName = TestContext.CurrentContext.Test.MethodName;
            ArtifactsHelper.SaveScreenshotAndLogs(testName, TestEnvironment.GetServiceLogsPath());
        }
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
}