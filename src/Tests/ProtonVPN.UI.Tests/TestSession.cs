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
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using NUnit.Framework.Interfaces;
using NUnit.Framework;
using ProtonVPN.Common.Extensions;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests
{
    public class TestSession
    {
        protected static Application App;
        protected static Application Service;
        protected static Window Window;

        public static void DeleteProfiles()
        {
            string args = $"{TestUserData.GetPlusUser().Username} {TestUserData.GetPlusUser().Password}";
            Assembly asm = Assembly.GetExecutingAssembly();
            string pathToProfileCleaner = Path.Combine(Path.GetDirectoryName(asm.Location), "TestTools.ProfileCleaner.exe");
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(pathToProfileCleaner, args)
            };
            process.Start();
            process.WaitForExit();
        }

        protected static void DeleteUserConfig()
        {
            string localAppDataFolder = Path.Combine(TestConstants.AppLogsPath, @"..\..\");
            try
            {
                Directory.Delete(localAppDataFolder, true);
                Directory.Delete(TestConstants.ServiceLogsPath);
            }
            catch (Exception)
            {
                //ignored
            }
        }

        protected static void Cleanup()
        {
            SaveScreenshotAndLogsIfFailed();
            VPNServiceHelper serviceHelper = new VPNServiceHelper();
            serviceHelper.Disconnect().GetAwaiter().GetResult();
            App.Close();
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

        protected static void RefreshWindow()
        {
            Window = null;
            RetryResult<Window> retry = Retry.WhileNull(
                () => {
                    try
                    {
                        Window = App.GetMainWindow(new UIA3Automation(), TestConstants.MediumTimeout);
                    }
                    catch (System.TimeoutException)
                    {
                        //Ignore
                    }
                    return Window;
                },
                TestConstants.MediumTimeout, TestConstants.RetryInterval);

            if (!retry.Success)
            {
                Assert.Fail($"Failed to refresh window in {TestConstants.MediumTimeout.Seconds} seconds.");
            }
        }

        protected static void LaunchApp()
        {
            App = Application.Launch(TestConstants.AppFolderPath + @"\ProtonVPN.exe");
        }

        protected static void KillProtonVpnProcess()
        {
            Process[] proc = Process.GetProcessesByName("ProtonVPN");
            proc.ForEach(p => p.Kill());
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

        private static void SaveScreenshotAndLogsIfFailed()
        {
            if (!TestEnvironment.AreTestsRunningLocally() && !TestEnvironment.IsWindows11())
            {
                TestStatus status = TestContext.CurrentContext.Result.Outcome.Status;
                string testName = TestContext.CurrentContext.Test.MethodName;
                if (status == TestStatus.Failed)
                {
                    TestsRecorder.SaveScreenshotAndLogs(testName);
                }
            }
        }
    }
}
