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
using System.IO;
using System.Security;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Registry;

namespace ProtonVPN.Common.Test.OS.Registry
{
    [TestClass]
    public class SafeStartupRecordTest
    {
        private ILogger _logger;
        private IStartupRecord _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _origin = Substitute.For<IStartupRecord>();
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Exists_ShouldReturn_OriginExists(bool value)
        {
            _origin.Exists().Returns(value);
            var record = new SafeStartupRecord(_logger, _origin);

            var result = record.Exists();

            _origin.Received().Exists();
            result.Should().Be(value);
        }

        [DataTestMethod]
        [DataRow(typeof(SecurityException))]
        [DataRow(typeof(UnauthorizedAccessException))]
        [DataRow(typeof(IOException))]
        public void Exists_ShouldReturnFalse_WhenOriginThrows_RegistryAccessException(Type exceptionType)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.Exists().Throws(exception);
            var record = new SafeStartupRecord(_logger, _origin);

            var result = record.Exists();

            _origin.Received().Exists();
            result.Should().Be(false);
        }

        [TestMethod]
        public void Exists_ShouldThrow_WhenOriginThrows()
        {
            _origin.Exists().Throws(new Exception());
            var record = new SafeStartupRecord(_logger, _origin);

            Action action = () => record.Exists();

            action.Should().Throw<Exception>();
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Valid_ShouldReturn_OriginValid(bool value)
        {
            _origin.Valid().Returns(value);
            var record = new SafeStartupRecord(_logger, _origin);

            var result = record.Valid();

            _origin.Received().Valid();
            result.Should().Be(value);
        }

        [DataTestMethod]
        [DataRow(typeof(SecurityException))]
        [DataRow(typeof(UnauthorizedAccessException))]
        [DataRow(typeof(IOException))]
        public void Valid_ShouldReturnFalse_WhenOriginThrows_RegistryAccessException(Type exceptionType)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.Valid().Throws(exception);
            var record = new SafeStartupRecord(_logger, _origin);

            var result = record.Valid();

            result.Should().Be(false);
        }

        [TestMethod]
        public void Valid_ShouldPass_Exception()
        {
            _origin.Valid().Throws(new Exception());
            var record = new SafeStartupRecord(_logger, _origin);

            Action action = () => record.Valid();

            action.Should().Throw<Exception>();
        }

        [TestMethod]
        public void Create_ShouldCall_OriginCreate()
        {
            var record = new SafeStartupRecord(_logger, _origin);

            record.Create();

            _origin.Received().Create();
        }

        [DataTestMethod]
        [DataRow(typeof(SecurityException))]
        [DataRow(typeof(UnauthorizedAccessException))]
        [DataRow(typeof(IOException))]
        public void Create_ShouldSuppress_RegistryAccessException(Type exceptionType)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.When(x => x.Create()).Do(_ => throw exception);
            var record = new SafeStartupRecord(_logger, _origin);

            Action action = () => record.Create();

            action.Should().NotThrow();
        }

        [TestMethod]
        public void Create_ShouldPass_Exception()
        {
            _origin.When(x => x.Create()).Do(_ => throw new Exception());
            var record = new SafeStartupRecord(_logger, _origin);

            Action action = () => record.Create();

            action.Should().Throw<Exception>();
        }

        [TestMethod]
        public void Remove_ShouldCall_OriginRemove()
        {
            var record = new SafeStartupRecord(_logger, _origin);

            record.Remove();

            _origin.Received().Remove();
        }

        [DataTestMethod]
        [DataRow(typeof(SecurityException))]
        [DataRow(typeof(UnauthorizedAccessException))]
        [DataRow(typeof(IOException))]
        public void Remove_ShouldSuppress_RegistryAccessException(Type exceptionType)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.When(x => x.Remove()).Do(_ => throw exception);
            var record = new SafeStartupRecord(_logger, _origin);

            Action action = () => record.Remove();

            action.Should().NotThrow();
        }

        [TestMethod]
        public void Remove_ShouldPass_Exception()
        {
            _origin.When(x => x.Remove()).Do(_ => throw new Exception());
            var record = new SafeStartupRecord(_logger, _origin);

            Action action = () => record.Remove();

            action.Should().Throw<Exception>();
        }
    }
}
