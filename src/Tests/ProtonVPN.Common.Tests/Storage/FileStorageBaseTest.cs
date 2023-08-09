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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.FileStoraging;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Serialization.Contracts;
using ProtonVPN.Tests.Common;

namespace ProtonVPN.Common.Tests.Storage
{
    [TestClass]
    public abstract class FileStorageBaseTest<TFileStorage, TEntity>
        where TFileStorage : FileStorageBase<TEntity>
    {
        private ILogger _logger;
        private IConfiguration _appConfig;
        private IJsonSerializer _jsonSerializer;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _appConfig = Substitute.For<IConfiguration>();
            _jsonSerializer = Substitute.For<IJsonSerializer>();
            _jsonSerializer
                .Deserialize<int>(Arg.Any<TextReader>())
                .Returns(args => int.Parse(args.Arg<TextReader>().ReadToEnd()));
            _jsonSerializer
                .When(x => x.Serialize(Arg.Any<int>(), Arg.Any<TextWriter>()))
                .Do(args => args.Arg<TextWriter>().Write(args.Arg<int>().ToString()));
        }

        protected abstract TFileStorage Construct(ILogger logger, IJsonSerializer jsonSerializer,
            IConfiguration appConfig, string fileName);

        [TestMethod]
        public void FileStorage_ShouldThrow_WhenFilename_IsNull()
        {
            // Act
            Action action = () => Construct(_logger, _jsonSerializer, _appConfig, null);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void FileStorage_ShouldThrow_WhenFilename_IsEmpty()
        {
            // Act
            Action action = () => Construct(_logger, _jsonSerializer, _appConfig, "");

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Get_ShouldBe_NotZero_WhenFileExists()
        {
            // Arrange
            string fileName = GetFolderPath("Test.json");
            TFileStorage storage = Construct(_logger, _jsonSerializer, _appConfig, fileName);

            // Act
            TEntity result = storage.Get();

            // Assert
            result.Should().NotBe(default(TEntity));
        }

        private string GetFolderPath(string fileName)
        {
            return TestConfig.GetFolderPath(fileName);
        }

        [TestMethod]
        public void Get_ShouldThrow_FileAccessException_WhenFileDoesNotExist()
        {
            // Arrange
            TFileStorage storage = Construct(_logger, _jsonSerializer, _appConfig, "Does-not-exist.json");

            // Act
            Action action = () => storage.Get();

            // Assert
            action.Should().Throw<Exception>().And.IsFileAccessException().Should().BeTrue();
        }

        [TestMethod]
        public void Get_ShouldThrow_FileAccessException_WhenFolderDoesNotExist()
        {
            // Arrange
            TFileStorage storage = Construct(_logger, _jsonSerializer, _appConfig, "Does-not-exist\\Test.json");

            // Act
            Action action = () => storage.Get();

            // Assert
            action.Should().Throw<Exception>().And.IsFileAccessException().Should().BeTrue();
        }

        [TestMethod]
        public void Set_ShouldSave_ToFile()
        {
            // Arrange
            string fileName = GetFolderPath("Saved-data.json");
            File.Delete(fileName);
            TFileStorage storage = Construct(_logger, _jsonSerializer, _appConfig, fileName);

            // Act
            storage.Set(CreateEntity());

            // Assert
            File.Exists(fileName).Should().BeTrue();
        }

        protected abstract TEntity CreateEntity();

        [TestMethod]
        public void Set_ShouldThrow_FileAccessException_WhenFolderDoesNotExist()
        {
            // Arrange
            TFileStorage storage = Construct(_logger, _jsonSerializer, _appConfig, "Does-not-exist\\Saved.json");

            // Act
            Action action = () => storage.Set(CreateEntity());

            // Assert
            action.Should().Throw<Exception>().And.IsFileAccessException().Should().BeTrue();
        }
    }
}