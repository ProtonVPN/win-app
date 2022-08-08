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
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using ProtonVPN.Common.Extensions;
using ProtonVPN.UI.Test.FlaUI.Utils;

namespace ProtonVPN.UI.Test.FlaUI
{
    public class TestSession
    {
        protected static Application App;
        protected static Application Service;
        protected static Window Window;

        protected static void DeleteUserConfig()
        {
            string localAppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProtonVPN");
            if (!Directory.Exists(localAppDataFolder))
            {
                return;
            }

            try
            {
                Directory.Delete(localAppDataFolder, true);
            }
            catch (Exception)
            {
                //ignored
            }
        }

        protected static void Cleanup()
        {
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
            UITestSession.TestRailClient.MarkTestsByStatus();
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
    }
}
