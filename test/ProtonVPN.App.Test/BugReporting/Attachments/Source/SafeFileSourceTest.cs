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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.BugReporting.Attachments.Source;
using ProtonVPN.Common.Logging;

namespace ProtonVPN.App.Test.BugReporting.Attachments.Source
{
    [TestClass]
    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    public class SafeFileSourceTest
    {
        private ILogger _logger;
        private IEnumerable<string> _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _origin = Substitute.For<IEnumerable<string>>();
        }

        [TestMethod]
        public void Enumerable_ShouldBe_FromOrigin()
        {
            // Arrange
            var origin = new[] {"file-1", "file-2", "file-3"};
            var source = new SafeFileSource(_logger, origin);
            // Act
            var result = source.ToList();
            // Assert
            result.Should().BeEquivalentTo(origin);
        }

        [DataTestMethod]
        [DataRow(typeof(PathTooLongException))]
        [DataRow(typeof(SecurityException))]
        [DataRow(typeof(DirectoryNotFoundException))]
        [DataRow(typeof(IOException))]
        public void Enumerable_ShouldBeEmpty_WhenOriginThrows_ExpectedException(Type exceptionType)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.GetEnumerator().Throws(exception);
            var source = new SafeFileSource(_logger, _origin);
            // Act
            var result = source.ToList();
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Enumerable_ShouldLog_WhenOriginThrows_ExpectedException()
        {
            // Arrange
            _origin.GetEnumerator().Throws(new IOException());
            var source = new SafeFileSource(_logger, _origin);
            // Act
            source.ToList();
            // Assert
            _logger.ReceivedWithAnyArgs().Warn("");
        }

        [TestMethod]
        public void Enumerable_ShouldThrow_WhenOriginThrows_NotExpectedException()
        {
            // Arrange
            _origin.GetEnumerator().Throws(new Exception());
            var source = new SafeFileSource(_logger, _origin);
            // Act
            Action action = () => source.ToList();
            // Assert
            action.Should().Throw<Exception>();
        }
    }
}
