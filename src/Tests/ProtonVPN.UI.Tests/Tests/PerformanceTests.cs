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

        private static readonly Stopwatch _timer = new Stopwatch();
        private readonly PrometheusHelper _prometheusHelper = new PrometheusHelper();

        [SetUp]
        public void TestInitialize()
        {
            DeleteUserConfig();
            LaunchApp();
        }

        [Test]
        public async Task QuickConnectPerformanceAsync()
        {
            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.PressQuickConnectButton();
            _timer.Start();
            _homeWindow.WaitUntilConnected();
            _timer.Stop();
            _homeWindow.PressQuickConnectButton();
            //Sometimes API calls break due to VPN connection disconnection, so timeout is needed.
            Thread.Sleep(3000);
            await _prometheusHelper.PushMetricAsync("windows_connection_duration_seconds", "Windows client connection duration.", _timer.Elapsed.TotalSeconds);
            Console.WriteLine($"Connection time: {_timer.Elapsed.Seconds} seconds.");
        }

        [Test]
        public async Task LoginPerformanceAsync()
        {
            _loginWindow.EnterCredentials(TestUserData.GetPlusUser());
            _timer.Start();
            _loginWindow.WaitUntilLoggedIn();
            _timer.Stop();
            await _prometheusHelper.PushMetricAsync("windows_login_duration_seconds", "Windows client login duration.", _timer.Elapsed.TotalSeconds);
            Console.WriteLine($"Login time: {_timer.Elapsed.Seconds} seconds.");
        }

        [TearDown]
        public void TestCleanup()
        {
            Cleanup();
            string testName = TestContext.CurrentContext.Test.MethodName;
            TestsRecorder.SaveLogs(testName);
            _timer.Reset();
        }
    }
}
