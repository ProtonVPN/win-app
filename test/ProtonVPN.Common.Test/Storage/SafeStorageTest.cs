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

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Storage;

namespace ProtonVPN.Common.Test.Storage
{
    [TestClass]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class SafeStorageTest
    {
        private IStorage<string> _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _origin = Substitute.For<IStorage<string>, IThrowsExpectedExceptions>();
        }

        [TestMethod]
        public void SafeStorage_ShouldThrow_WhenOriginIsNull()
        {
            // Act
            Action action = () => new SafeStorage<string>(null);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void SafeStorage_ShouldThrow_WhenOriginDoesNotImplement_IThrowsExpectedExceptions()
        {
            // Arrange
            var origin = Substitute.For<IStorage<string>>();

            // Act
            Action action = () => new SafeStorage<string>(origin);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Get_ShouldBe_OriginGet()
        {
            // Arrange
            const string expected = "John Doe";
            _origin.Get().Returns(expected);
            var storage = new SafeStorage<string>(_origin);

            // Act
            var result = storage.Get();
            
            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void Get_ShouldBeNull_WhenOriginThrows_ExpectedException()
        {
            // Arrange
            var exception = new Exception();
            _origin.When(x => x.Get()).Do(_ => throw exception);
            ((IThrowsExpectedExceptions)_origin).IsExpectedException(exception).Returns(true);

            var storage = new SafeStorage<string>(_origin);

            // Act
            var result = storage.Get();
            
            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void Get_ShouldPass_UnexpectedException()
        {
            // Arrange
            _origin.When(x => x.Get()).Do(_ => throw new Exception());
            ((IThrowsExpectedExceptions)_origin).IsExpectedException(Arg.Any<Exception>()).Returns(false);
            var storage = new SafeStorage<string>(_origin);

            // Act
            Action action = () => storage.Get();
            
            // Assert
            action.Should().Throw<Exception>();
        }

        [TestMethod]
        public void Set_ShouldCall_OriginSet()
        {
            // Arrange
            const string value = "John Doe Set";
            var storage = new SafeStorage<string>(_origin);

            // Act
            storage.Set(value);

            // Assert
            _origin.Received().Set(value);
        }

        [TestMethod]
        public void Set_ShouldIgnore_ExpectedException()
        {
            // Arrange
            var exception = new Exception();
            _origin.When(x => x.Set(Arg.Any<string>())).Do(_ => throw exception);
            ((IThrowsExpectedExceptions)_origin).IsExpectedException(exception).Returns(true);

            var storage = new SafeStorage<string>(_origin);

            // Act
            Action action = () => storage.Set("ABC");

            // Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void Set_ShouldPass_UnexpectedException()
        {
            // Arrange
            var exception = new Exception();
            _origin.When(x => x.Set(Arg.Any<string>())).Do(_ => throw exception);
            ((IThrowsExpectedExceptions)_origin).IsExpectedException(Arg.Any<Exception>()).Returns(false);

            var storage = new SafeStorage<string>(_origin);

            // Act
            Action action = () => storage.Set("ABC");

            // Assert
            action.Should().Throw<Exception>();
        }
    }
}
