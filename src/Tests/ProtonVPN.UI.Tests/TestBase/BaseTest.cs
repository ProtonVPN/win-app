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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestsHelper;
using TimeoutException = System.TimeoutException;

namespace ProtonVPN.UI.Tests.TestBase;

// Shared methods related to test session.
public class BaseTest
{
    public static Application App;
    public static Window Window;

    protected static LoginRobot LoginRobot { get; } = new();
    protected static HomeRobot HomeRobot { get; } = new();
    protected static NavigationRobot NavigationRobot { get; } = new();
    protected static ProfileRobot ProfileRobot { get; } = new();
    protected static SidebarRobot SidebarRobot { get; } = new();
    protected static SettingRobot SettingRobot { get; } = new();
    protected static SupportRobot SupportRobot { get; } = new(() => Window);
    protected static AdvancedSettingsRobot AdvancedSettingsRobot { get; } = new();
    protected static UpsellCarrouselRobot UpsellCarrouselRobot { get; } = new();
    protected static SplitTunnelingRobot SplitTunnelingRobot { get; } = new();
    protected static ConfirmationRobot ConfirmationRobot { get; } = new();

    private const string CLIENT_NAME = "ProtonVPN.Client.exe";

    private static readonly bool _isDevelopmentModeEnabled = false;

    // Shared SetUp, TearDown actions that will be performed accross all the tests.
    [SetUp]
    public async Task GlobalSetUp()
    {
        string testName = TestContext.CurrentContext.Test.MethodName;
        ArtifactsHelper.ClearEventViewerLogs();
        ArtifactsHelper.DeleteArtifactFolder(testName);
        await ArtifactsHelper.StartVideoCaptureAsync(testName);
    }

    [TearDown]
    public void GlobalTeardown()
    {
        string testName = TestContext.CurrentContext.Test.MethodName;

        ArtifactsHelper.Recorder.Stop();
        ArtifactsHelper.Recorder.Dispose();
        ArtifactsHelper.SaveEventViewerLogs(testName);
        if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed)
        {
            ArtifactsHelper.DeleteArtifactFolder(testName);
        }
    }

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
            catch (Win32Exception)
            {
                // Ignore
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
        App.Dispose();
    }

    protected static void LaunchApp(bool isFreshStart = true)
    {
        if (isFreshStart)
        {
            DeleteProtonData();
        }

        string installedClientPath = Path.Combine(
            _isDevelopmentModeEnabled
                ? TestEnvironment.GetDevProtonClientFolder()
                : TestEnvironment.GetProtonClientFolder(),
            CLIENT_NAME);

        ProcessStartInfo startInfo = new ProcessStartInfo(installedClientPath)
        {
            Arguments = "-ExitAppOnClose"
        };
        App = Application.Launch(startInfo);

        RetryResult<bool> result = WaitUntilAppIsRunning();
        if (!result.Success)
        {
            //Sometimes app fails to launch on first try due to CI issues.
            App = Application.Launch(installedClientPath);
        }

        RefreshWindow(TestConstants.OneMinuteTimeout);
        Window.Focus();
    }

    protected static void SaveScreenshotAndLogsIfFailed()
    {
        if (!TestEnvironment.AreTestsRunningLocally() && TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
        {
            string testName = TestContext.CurrentContext.Test.MethodName;
            ArtifactsHelper.SaveScreenshotAndLogs(testName, TestEnvironment.GetServiceLogsPath());
        }
    }

    private static RetryResult<bool> WaitUntilAppIsRunning()
    {
        RetryResult<bool> retry = Retry.WhileFalse(
            () =>
            {
                Process[] pname = Process.GetProcessesByName("ProtonVPN.Client");
                return pname.Length > 0;
            },
            TimeSpan.FromSeconds(30), TestConstants.RetryInterval);

        return retry;
    }
}