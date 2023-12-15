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
using System.ServiceProcess;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using NUnit.Framework.Interfaces;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;
using System.Security.Principal;

namespace ProtonVPN.UI.Tests
{
    public class TestSession
    {
        protected static Application App;
        protected static Application Service;
        protected static Window Window;

        protected static void DeleteUserConfig()
        {
            string localAppDataFolder = Path.Combine(TestData.AppLogsPath, @"..\..\");
            try
            {
                Directory.Delete(localAppDataFolder, true);
                Directory.Delete(TestData.ServiceLogsPath);
                File.Delete(TestData.TestConfigPath);
                Directory.Delete(TestData.ServiceLogsFolder, true);
            }
            catch (Exception)
            {
                //ignored
            }
        }

        protected static void ClientCleanup()
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
            try
            {
                ServiceController service = new ServiceController("ProtonVPN Service");
                service.Stop();
            }
            catch (InvalidOperationException)
            {
                //Ignore because service might not be started.
            }
        }

        protected static void RefreshWindow(TimeSpan? timeout = null)
        {
            Window = null;
            TimeSpan refreshTimeout = timeout ?? TestData.MediumTimeout;
            RetryResult<Window> retry = Retry.WhileNull(
                () => {
                    try
                    {
                        Window = App.GetMainWindow(new UIA3Automation(), refreshTimeout);
                    }
                    catch (System.TimeoutException)
                    {
                        //Ignore
                    }
                    return Window;
                },
                refreshTimeout, TestData.RetryInterval);

            if (!retry.Success)
            {
                Assert.Fail($"Failed to refresh window in {refreshTimeout.TotalSeconds} seconds.");
            }
        }

        protected static void LaunchApp()
        {
            string appExecutable = TestData.AppVersionFolder + @"\ProtonVPN.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo(appExecutable)
            {
                Arguments = "/DisableAutoUpdate"
            };

            App = Application.Launch(startInfo);
            RetryResult<bool> result = WaitUntilAppIsRunning();
            if (!result.Success)
            {
                //Sometimes app fails to launch on first try due to CI issues.
                App = Application.Launch(startInfo);
            }
            RefreshWindow(TestData.LongTimeout);
        }


        protected static void KillProtonVpnProcess()
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/C taskkill /F /im protonvpn.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            process.StartInfo = startInfo;
            process.Start();
            process.Close();
            //Give some time to properly exit the app
            Thread.Sleep(2000);
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
            if(explorrerProcess.Length == 0)
            {
                string explorer = string.Format("{0}\\{1}", Environment.GetEnvironmentVariable("WINDIR"), "explorer.exe");
                Process process = new Process();
                process.StartInfo.FileName = explorer;
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
        }
        
        protected static void GenerateAppConfig()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal currentPrincipal = new WindowsPrincipal(currentIdentity);
            bool isAdmin = currentPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            Assert.That(isAdmin, Is.True, "User does not have Admin permissions, to generate Configuration file.");

            LaunchApp();
            App.Close();
        }

        private static void SaveScreenshotAndLogsIfFailed()
        {
            if (!TestEnvironment.AreTestsRunningLocally() && !TestEnvironment.IsWindows11())
            {
                TestStatus status = TestContext.CurrentContext.Result.Outcome.Status;
                string testName = TestContext.CurrentContext.Test.MethodName;
                if (status == TestStatus.Failed)
                {
                    TestsRecorder.SaveLogs(testName);
                    TestsRecorder.SaveScreenshot(testName);
                }
            }
        }

        private static RetryResult<bool> WaitUntilAppIsRunning()
        {
            RetryResult<bool> retry = Retry.WhileFalse(
                () => {
                    Process[] pname = Process.GetProcessesByName("ProtonVPN");
                    return pname.Length > 0;
                },
                TimeSpan.FromSeconds(30), TestData.RetryInterval);

            return retry;
        }
    }
}
