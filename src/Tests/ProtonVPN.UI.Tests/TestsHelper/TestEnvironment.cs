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
using Castle.Core.Internal;

namespace ProtonVPN.UI.Tests.TestsHelper
{
    public class TestEnvironment : TestSession
    {
        public static bool AreTestsRunningLocally()
        {
            bool isLocalEnvironment = false;
            string ciCommitHash = Environment.GetEnvironmentVariable("CI_COMMIT_SHA");
            if (ciCommitHash.IsNullOrEmpty())
            {
                isLocalEnvironment = true;
            }
            return isLocalEnvironment;
        }

        public static bool IsVideoRecorderPresent()
        {
            return File.Exists(TestConstants.PathToRecorder);
        }

        public static bool IsWindows11()
        {
            return Environment.OSVersion.Version.Build >= 22000;
        }
    }
}
