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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Storage;
using ProtonVPN.Common.Text.Serialization;
using ProtonVPN.Tests.Common;

namespace ProtonVPN.Common.Tests.Storage
{
    [TestClass]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class FileStorageTest
    {
        private ITextSerializer<int> _serializer;
        private ITextSerializerFactory _serializerFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            _serializer = Substitute.For<ITextSerializer<int>, IThrowsExpectedExceptions>();
            _serializer
                .Deserialize(Arg.Any<TextReader>())
                .Returns(args => int.Parse(args.Arg<TextReader>().ReadToEnd()));
            _serializer
                .When(x => x.Serialize(Arg.Any<int>(), Arg.Any<TextWriter>()))
                .Do(args => args.Arg<TextWriter>().Write(args.Arg<int>().ToString()));

            _serializerFactory = Substitute.For<ITextSerializerFactory>();
            _serializerFactory.Serializer<int>().Returns(_serializer);
        }

        [TestMethod]
        public void FileStorage_ShouldThrow_WhenSerializerFactory_IsNull()
        { 
            // Act
            Action action = () => new FileStorage<int>(null, "FileName");

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void FileStorage_ShouldThrow_WhenFilename_IsNull()
        {
            // Act
            Action action = () => new FileStorage<int>(_serializerFactory, null);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void FileStorage_ShouldThrow_WhenFilename_IsEmpty()
        {
            // Act
            Action action = () => new FileStorage<int>(_serializerFactory, "");

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void FileStorage_ShouldThrow_WhenSerializerFactory_Serializer_DoesNotImplement_IThrowsExpectedExceptions()
        {
            // Arrange
            ITextSerializer<int> serializer = Substitute.For<ITextSerializer<int>>();
            _serializerFactory.Serializer<int>().Returns(serializer);

            // Act
            Action action = () => new FileStorage<int>(_serializerFactory, "ABC");

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Get_ShouldBe_NotZero_WhenFileExists()
        {
            // Arrange
            string fileName = FilePath("Test.json");
            FileStorage<int> storage = new FileStorage<int>(_serializerFactory, fileName);

            // Act
            int result = storage.Get();
            
            // Assert
            result.Should().NotBe(0);
        }

        [TestMethod]
        public void Get_ShouldThrow_FileAccessException_WhenFileDoesNotExist()
        {
            // Arrange
            FileStorage<int> storage = new FileStorage<int>(_serializerFactory, "Does-not-exist.json");

            // Act
            Action action = () => storage.Get();
            
            // Assert
            action.Should().Throw<Exception>()
                .And.IsFileAccessException().Should().BeTrue();
        }

        [TestMethod]
        public void Get_ShouldThrow_FileAccessException_WhenFolderDoesNotExist()
        {
            // Arrange
            FileStorage<int> storage = new FileStorage<int>(_serializerFactory, "Does-not-exist\\Test.json");
            
            // Act
            Action action = () => storage.Get();
            
            // Assert
            action.Should().Throw<Exception>()
                .And.IsFileAccessException().Should().BeTrue();
        }

        [TestMethod]
        public void Set_ShouldSave_ToFile()
        {
            // Arrange
            string fileName = FilePath("Saved-data.json");
            File.Delete(fileName);
            FileStorage<int> storage = new FileStorage<int>(_serializerFactory, fileName);

            // Act
            storage.Set(348);

            // Assert
            File.Exists(fileName).Should().BeTrue();
        }

        [TestMethod]
        public void Set_ShouldThrow_FileAccessException_WhenFolderDoesNotExist()
        {
            // Arrange
            FileStorage<int> storage = new FileStorage<int>(_serializerFactory, "Does-not-exist\\Saved.json");

            // Act
            Action action = () => storage.Set(7896);

            // Assert
            action.Should().Throw<Exception>()
                .And.IsFileAccessException().Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(typeof(IOException))]
        [DataRow(typeof(UnauthorizedAccessException))]
        public void IsExpectedException_ShouldBeTrue_WhenFileAccessException(Type exceptionType)
        {
            // Arrange
            Exception exception = (Exception)Activator.CreateInstance(exceptionType);
            FileStorage<int> storage = new FileStorage<int>(_serializerFactory, "kmb");

            // Act
            bool result = storage.IsExpectedException(exception);
            
            // Assert
            result.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void IsExpectedException_ShouldBe_SerializerFactory_Serializer_IsExpectedException(bool expected)
        {
            // Arrange
            Exception exception = new();
            ((IThrowsExpectedExceptions)_serializer).IsExpectedException(exception).Returns(expected);
            FileStorage<int> storage = new FileStorage<int>(_serializerFactory, "kmb");

            // Act
            bool result = storage.IsExpectedException(exception);

            // Assert
            result.Should().Be(expected);
        }

        #region Helpers

        private string FilePath(string fileName)
        {
            return TestConfig.GetFolderPath(fileName);
        }

        #endregion
    }
}