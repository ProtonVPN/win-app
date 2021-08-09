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
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using ProtonVPN.Common.Extensions;
using ProtonVPN.UI.Test.ApiClient;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test
{
    public class UITestSession
    {
        private const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        private const string AppPath = @"C:\Program Files (x86)\Proton Technologies\ProtonVPN\ProtonVPN.exe";
        protected const double ImplicitWaitTimeInSeconds = 5;
        private static readonly List<WindowsDriver<WindowsElement>> Sessions = new List<WindowsDriver<WindowsElement>>();
        protected static WindowsDriver<WindowsElement> Session;
        public static TestRailAPIClient TestRailClient;
        public static ulong TestCaseId { get; set; }

        public static void CreateSession()
        {
            TestCaseId = 0;
            if (Sessions.Count > 0)
            {
                foreach (var session in Sessions)
                {
                    session.Quit();
                }
                Session = null;
                Sessions.Clear();
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
            foreach (var session in Sessions)
            {
                session.Quit();
                session.Dispose();
            }

            Sessions.Clear();
            Session = null;
            TestRailClient.MarkTestsByStatus();
        }

        public static void KillProtonVpnProcess()
        {
            Process[] proc = Process.GetProcessesByName("ProtonVPN");
            proc.ForEach(p => p.Kill());
        }

        public static void KillProtonVPNProcessAndReopenIt()
        {
            KillProtonVpnProcess();
            RefreshSession();
            UIActions.WaitUntilElementIsNotVisible(By.ClassName("Loading"), 15);
            RefreshSession();
        }

        protected static void SetImplicitWait(double timeInSeconds)
        {
            Session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(timeInSeconds);
        }

        private static void CreateSessionInner()
        {
            try
            {
                Session = CreateNewDriver();
            }
            catch (WebDriverException)
            {
                Session = CreateNewDriver();
            }

            SetImplicitWait(ImplicitWaitTimeInSeconds);
            Assert.IsNotNull(Session);
            Sessions.Add(Session);
            Session.SwitchTo().Window(Session.WindowHandles.First());
        }

        private static WindowsDriver<WindowsElement> CreateNewDriver()
        {
            var options = new AppiumOptions();
            options.AddAdditionalCapability("app", AppPath);
            options.AddAdditionalCapability("deviceName", "WindowsPC");
            return new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), options);
        }

        private static void DeleteUserConfig()
        {
            var localAppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProtonVPN");
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
    }
}