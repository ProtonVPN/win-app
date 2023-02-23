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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Config;

namespace ProtonVPN.App.Tests.Core.Config
{
    [TestClass]
    public class ConfigDirectoriesTest
    {
        private const string MainFolder = "ProtonVPN";
        private const string DiagnosticFolder = "Diagnostics";

        [TestMethod]
        public void Prepare_ShouldCreate_LocalAppDataFolder()
        {
            // Arrange
            if (Directory.Exists(MainFolder))
            {
                Directory.Delete(MainFolder);
            }
            ConfigDirectories subject = new(GetConfig());

            // Act
            subject.Prepare();

            // Assert
            Directory.Exists(MainFolder).Should().BeTrue();
            Directory.Exists(DiagnosticFolder).Should().BeTrue();
        }

        [TestMethod]
        public void Prepare_ShouldSucceed_When_LocalAppDataFolder_Exists()
        {
            // Arrange
            ConfigDirectories subject = new(GetConfig());

            // Act
            Action action = () => subject.Prepare();

            // Assert
            action.Should().NotThrow();
        }

        private IConfiguration GetConfig()
        {
            return new Common.Configuration.Config
            {
                LocalAppDataFolder = MainFolder,
                DiagnosticsLogFolder = DiagnosticFolder
            };
        }
    }
}
