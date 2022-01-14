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
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Storage;

namespace ProtonVPN.Common.Test.Storage
{
    [TestClass]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
    public class LoggingStorageTest
    {
        private ILogger _logger;
        private IStorage<string> _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _origin = Substitute.For<IStorage<string>, IThrowsExpectedExceptions>();
        }

        [TestMethod]
        public void LoggingStorage_ShouldThrow_WhenLoggerIsNull()
        {
            // Act
            Action action = () => new LoggingStorage<string>(null, _origin);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void LoggingStorage_ShouldThrow_WhenOriginIsNull()
        {
            // Act
            Action action = () => new LoggingStorage<string>(_logger, null);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void LoggingStorage_ShouldThrow_WhenOriginDoesNotImplement_IThrowsExpectedExceptions()
        {
            // Arrange
            var origin = Substitute.For<IStorage<string>>();

            // Act
            Action action = () => new LoggingStorage<string>(_logger, origin);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Get_ShouldBe_OriginGet()
        {
            // Arrange
            const string expected = "John Doe";
            _origin.Get().Returns(expected);
            var storage = new LoggingStorage<string>(_logger, _origin);

            // Act
            string value = storage.Get();
            
            // Assert
            value.Should().Be(expected);
        }

        [TestMethod]
        public void Get_ShouldLog_WhenOriginThrows_ExpectedException()
        {
            // Arrange
            var exception = new Exception();
            _origin.When(x => x.Get()).Do(_ => throw exception);
            ((IThrowsExpectedExceptions)_origin).IsExpectedException(exception).Returns(true);

            var storage = new LoggingStorage<string>(_logger, _origin);

            // Act
            try
            {
                storage.Get();
            }
            catch
            {
            }
            
            // Assert
            _logger.Received().Error<AppLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        [TestMethod]
        public void Get_ShouldPass_ExpectedException()
        {
            // Arrange
            var exception = new Exception();
            _origin.When(x => x.Get()).Do(_ => throw exception);
            ((IThrowsExpectedExceptions)_origin).IsExpectedException(exception).Returns(true);
            var storage = new LoggingStorage<string>(_logger, _origin);

            // Act
            Action action = () => storage.Get();
            
            // Assert
            action.Should().Throw<Exception>();
        }

        [TestMethod]
        public void Get_ShouldPass_UnexpectedException()
        {
            // Arrange
            _origin.When(x => x.Get()).Do(_ => throw new Exception());
            ((IThrowsExpectedExceptions)_origin).IsExpectedException(Arg.Any<Exception>()).Returns(false);
            var storage = new LoggingStorage<string>(_logger, _origin);

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
            var storage = new LoggingStorage<string>(_logger, _origin);

            // Act
            storage.Set(value);

            // Assert
            _origin.Received().Set(value);
        }

        [TestMethod]
        public void Set_ShouldLog_UnexpectedException()
        {
            // Arrange
            _origin.When(x => x.Set(Arg.Any<string>())).Do(_ => throw new Exception());
            ((IThrowsExpectedExceptions)_origin).IsExpectedException(Arg.Any<Exception>()).Returns(false);

            var storage = new LoggingStorage<string>(_logger, _origin);

            // Act
            try
            {
                storage.Set("ABC");
            }
            catch
            {
            }

            // Assert
            _logger.Received().Error<AppLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        [TestMethod]
        public void Set_ShouldPass_ExpectedException()
        {
            // Arrange
            var exception = new Exception();
            _origin.When(x => x.Set(Arg.Any<string>())).Do(_ => throw exception);
            ((IThrowsExpectedExceptions)_origin).IsExpectedException(exception).Returns(true);

            var storage = new LoggingStorage<string>(_logger, _origin);

            // Act
            Action action = () => storage.Set("ABC");

            // Assert
            action.Should().Throw<Exception>();
        }

        [TestMethod]
        public void Set_ShouldPass_UnexpectedException()
        {
            // Arrange
            _origin.When(x => x.Set(Arg.Any<string>())).Do(_ => throw new Exception());
            ((IThrowsExpectedExceptions)_origin).IsExpectedException(Arg.Any<Exception>()).Returns(false);
            var storage = new LoggingStorage<string>(_logger, _origin);

            // Act
            Action action = () => storage.Set("ABC");

            // Assert
            action.Should().Throw<Exception>();
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void IsExpectedException_ShouldBe_Origin_IsExpectedException(bool expected)
        {
            // Arrange
            var exception = new Exception();
            ((IThrowsExpectedExceptions)_origin).IsExpectedException(exception).Returns(expected);

            var storage = new LoggingStorage<string>(_logger, _origin);

            // Act
            bool result = storage.IsExpectedException(exception);

            // Assert
            result.Should().Be(expected);
        }
    }
}
