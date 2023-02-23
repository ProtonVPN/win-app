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
using System.IO;
using System.Reflection;
using NUnit.Framework;
using ProtonVPN.UI.Tests.ApiClient;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests
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
            TestsRecorder.StartVideoCapture();
        }

        [OneTimeTearDown]
        public void TestFinalTearDown()
        {
            TestsRecorder.StopRecording();
        }
    }
}