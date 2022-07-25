/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.IO;
using NUnit.Framework;
using ProtonVPN.UI.Test.ApiClient;
using ProtonVPN.UI.Test.TestsHelper;
using System.Reflection;

namespace ProtonVPN.UI.Test.Tests
{
    [SetUpFixture]
    public class SetUp : TestSession
    {
        private readonly string _testRailUrl = "https://proton.testrail.io/";
        private Assembly asm = Assembly.GetExecutingAssembly();

        [OneTimeSetUp]
        public void TestInitialize()
        {
            KillProtonVpnProcess();
            string dir = Path.GetDirectoryName(TestConstants.AppFolderPath);
            Directory.SetCurrentDirectory(dir);
            CreateScreenshotFolder();
            TestRailClient = new TestRailApiClient(_testRailUrl,
                    TestUserData.GetTestrailUser().Username, TestUserData.GetTestrailUser().Password);
            if (!TestEnvironment.AreTestsRunningLocally())
            {
                CreateTestRailTestRun();
            }
        }

        private void CreateScreenshotFolder()
        {
            ScreenshotDir = Path.Combine(Path.GetDirectoryName(asm.Location), "TestScreenshots");
            try
            {
                Directory.Delete(ScreenshotDir, true);
            }
            catch (DirectoryNotFoundException)
            {
                //Ignore because directory could not exist
            }

            Directory.CreateDirectory(ScreenshotDir);
        }

        private void CreateTestRailTestRun()
        {
            string path = Path.Combine(Path.GetDirectoryName(asm.Location), "ProtonVPN.exe");
            string version = Assembly.LoadFile(path).GetName().Version.ToString();
            string branchName = Environment.GetEnvironmentVariable("CI_COMMIT_BRANCH");
            version = version.Substring(0, version.Length - 2);
            if (!TestRailClient.ShouldUpdateRun())
            {
                TestRailClient.CreateTestRun($"{branchName} {version} {DateTime.Now}");
            }
        }
    }
}