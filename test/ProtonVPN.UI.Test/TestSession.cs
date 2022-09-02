/*
 * Copyright (c) 2022 Proton
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
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.UIA3;
using NUnit.Framework;
using ProtonVPN.Common.Extensions;
using ProtonVPN.UI.Test.ApiClient;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test
{
    public class TestSession
    {
        protected static Application App;
        protected static Application Service;
        protected static Window Window;
        public static TestRailApiClient TestRailClient;
        public static ulong TestCaseId { get; set; }

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
            TestRailClient.MarkTestsByStatus();
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
            try
            {
                Window = App.GetMainWindow(new UIA3Automation(), TestConstants.MediumTimeout);
            }
            catch (Exception ex)
            {
                if(ex is COMException || ex is System.TimeoutException)
                {
                    //Sometimes UI might be locked and framework does not know how to handle it
                    Thread.Sleep(3000);
                    Window = App.GetMainWindow(new UIA3Automation(), TestConstants.MediumTimeout);
                    return;
                }
                throw;
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
        
    }
}
