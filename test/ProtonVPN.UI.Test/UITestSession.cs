/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace ProtonVPN.UI.Test
{
    public class UITestSession
    {
        private const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        private const string AppPath = @"C:\Program Files (x86)\Proton Technologies\ProtonVPN\ProtonVPN.exe";

        protected static WindowsDriver<WindowsElement> Session;

        private static readonly List<WindowsDriver<WindowsElement>> _sessions = new List<WindowsDriver<WindowsElement>>();

        public static void CreateSession()
        {
            if (Session != null)
            {
                return;
            }

            DeleteUserConfig();
            CreateSessionInner();
        }

        public static void RefreshSession()
        {
            CreateSessionInner();
        }

        public static void TearDown()
        {
            foreach (var session in _sessions)
            {
                session.Quit();
                session.Dispose();
            }

            _sessions.Clear();
            Session = null;
        }

        private static void CreateSessionInner()
        {
            var options = new AppiumOptions();
            options.AddAdditionalCapability("app", AppPath);
            options.AddAdditionalCapability("deviceName", "WindowsPC");
            Session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), options);
            Session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            Assert.IsNotNull(Session);
            _sessions.Add(Session);
            Session.SwitchTo().Window(Session.WindowHandles.First());
        }

        private static void DeleteUserConfig()
        {
            var localAppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProtonVPN");
            if (Directory.Exists(localAppDataFolder))
            {
                try
                {
                    Directory.Delete(localAppDataFolder, true);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}
