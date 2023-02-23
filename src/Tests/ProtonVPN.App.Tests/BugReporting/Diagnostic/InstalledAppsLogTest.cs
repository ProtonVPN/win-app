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

using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.BugReporting.Diagnostic;

namespace ProtonVPN.App.Tests.BugReporting.Diagnostic
{
    [TestClass]
    public class InstalledAppsLogTest : BaseLogTest
    {
        [TestMethod]
        public void ItShouldCreateLogFile()
        {
            // Act
            new InstalledAppsLog(Config).Write();

            // Assert
            File.Exists(Path.Combine(TmpPath, "Apps.txt")).Should().BeTrue();
        }
    }
}
