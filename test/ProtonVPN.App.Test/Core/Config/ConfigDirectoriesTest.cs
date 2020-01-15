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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Config;
using System;
using System.IO;

namespace ProtonVPN.App.Test.Core.Config
{
    [TestClass]
    public class ConfigDirectoriesTest
    {
        [TestMethod]
        public void Prepare_ShouldCreate_LocalAppDataFolder()
        {
            // Arrange
            const string path = nameof(Prepare_ShouldCreate_LocalAppDataFolder);
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }

            var config = new Common.Configuration.Config { LocalAppDataFolder = path };
            var subject = new ConfigDirectories(config);
            // Act
            subject.Prepare();
            // Assert
            Directory.Exists(path).Should().BeTrue();
        }

        [TestMethod]
        public void Prepare_ShouldSucceed_When_LocalAppDataFolder_Exists()
        {
            // Arrange
            const string path = nameof(Prepare_ShouldSucceed_When_LocalAppDataFolder_Exists);
            Directory.CreateDirectory(path);

            var config = new Common.Configuration.Config { LocalAppDataFolder = path };
            var subject = new ConfigDirectories(config);
            // Act
            Action action = () => subject.Prepare();
            // Assert
            action.Should().NotThrow();
        }
    }
}
