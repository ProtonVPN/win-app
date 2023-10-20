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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Windows;

namespace ProtonVPN.UI.Tests.Tests
{
    [TestFixture]
    [Category("Performance")]
    public class PerformanceTests : TestSession
    {
        private HomeWindow _homeWindow = new HomeWindow();
        private LoginWindow _loginWindow = new LoginWindow();
        private string _statusMetricName;
        private string _statusMetricDescription;

        [OneTimeSetUp]
        public async Task OneTimeSetUpAsync()
        {
            DeleteUserConfig();
            LaunchApp();
            await TestMonitorHelper.IncrementMetricAsync(
                "windows_pipeline_id",
                "Pipeline ID for alerts",
                double.Parse(Environment.GetEnvironmentVariable("CI_JOB_ID"))
            );
        }

        [Test, Order(1)]
        public async Task LoginPerformanceAsync()
        {
            _statusMetricName = "windows_login_status";
            _statusMetricDescription = "Indicates if login passed.";

            _loginWindow.EnterCredentials(TestUserData.GetPlusUser());

            TestMonitorHelper.Start();
            _loginWindow.WaitUntilLoggedIn();
            TestMonitorHelper.Stop();

            await TestMonitorHelper.ReportDurationAsync("windows_login_duration_seconds", "Login time");
        }

        [Test, Order(2)]
        public async Task QuickConnectPerformanceAsync()
        {
            _statusMetricName = "windows_connection_status";
            _statusMetricDescription = "Indicates if connection passed.";

            _homeWindow.PressQuickConnectButton();
            _homeWindow.WaitUntilConnected();
            _homeWindow.PressQuickConnectButton();

            //Cooldown for another connection
            Thread.Sleep(2000);

            _homeWindow.PressQuickConnectButton();
            TestMonitorHelper.Start();
            _homeWindow.WaitUntilConnected();
            TestMonitorHelper.Stop();
            _homeWindow.PressQuickConnectButton();

            // Sometimes API calls break due to VPN connection disconnection, so timeout is needed.
            Thread.Sleep(5000);

            await TestMonitorHelper.ReportDurationAsync("windows_connection_duration_seconds", "Connection time");
        }

        [TearDown]
        public async Task TestCleanupAsync()
        {
            string testName = TestContext.CurrentContext.Test.MethodName;
            await TestMonitorHelper.ReportTestStatusAsync(_statusMetricName, _statusMetricDescription);
            TestsRecorder.SaveLogs(testName);
            TestMonitorHelper.Reset();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Cleanup();
        }
    }
}
