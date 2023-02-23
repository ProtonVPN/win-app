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
using NSubstitute;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Configuration.Source;
using ProtonVPN.Common.Configuration.Storage;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Tests.Common;

namespace ProtonVPN.Common.Tests.Configuration.Storage
{
    [TestClass]
    public class FileConfigStorageTest
    {
        [TestMethod]
        public void Value_ShouldBe_NotNull_WhenFileExists()
        {
            // Arrange
            FileConfigStorage storage = new(ConfigFile("Config.json"));
            // Act
            IConfiguration result = storage.Value();
            // Assert
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void Value_ShouldThrow_FileAccessException_WhenFileDoesNotExist()
        {
            // Arrange
            FileConfigStorage storage = new(ConfigFile("Does-not-exist.json"));
            // Act
            Action action = () => storage.Value();
            // Assert
            action.Should().Throw<Exception>()
                .And.IsFileAccessException().Should().BeTrue();
        }

        [TestMethod]
        public void Value_ShouldThrow_FileAccessException_WhenFolderDoesNotExist()
        {
            // Arrange
            FileConfigStorage storage = new(ConfigFile("Does-not-exist\\Config.json"));
            // Act
            Action action = () => storage.Value();
            // Assert
            action.Should().Throw<Exception>()
                .And.IsFileAccessException().Should().BeTrue();
        }

        [TestMethod]
        public void Save_ShouldSave_ToFile()
        {
            // Arrange
            const string filename = "Saved-app-config.json";
            FileConfigStorage storage = new(ConfigFile(filename));
            IConfiguration config = new DefaultConfig().Value();
            // Act
            storage.Save(config);
            // Assert
            File.Exists($"TestData\\{filename}").Should().BeTrue();
        }

        private IStorageFile ConfigFile(string filename)
        {
            IStorageFile location = Substitute.For<IStorageFile>();
            location.Path().Returns(Path.Combine(TestConfig.TEST_DATA_FOLDER, filename));
            return location;
        }
    }
}