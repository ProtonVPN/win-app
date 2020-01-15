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
using System.ComponentModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.OS.Services;

namespace ProtonVPN.Common.Test.OS.Services
{
    [TestClass]
    public class SafeServiceTest
    {
        private const int ServiceAlreadyRunning = 1056;
        private const int ServiceNotRunning = 1062;

        private IService _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _origin = Substitute.For<IService>();
        }

        [TestMethod]
        public void Name_ShouldBe_Origin_Name()
        {
            // Arrange
            const string name = "My service";
            _origin.Name.Returns(name);
            var subject = new SafeService(_origin);
            // Act
            var result = subject.Name;
            // Assert
            result.Should().Be(name);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void IsRunning_ShouldBe_Origin_IsRunning(bool value)
        {
            // Arrange
            _origin.IsRunning().Returns(value);
            var subject = new SafeService(_origin);
            // Act
            var result = subject.IsRunning();
            // Assert
            result.Should().Be(value);
        }

        [DataTestMethod]
        [DataRow(typeof(Win32Exception))]
        public void IsRunning_ShouldBeFalse_WhenOriginThrows_ExpectedException(Type exceptionType)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.IsRunning().Throws(exception);
            var subject = new SafeService(_origin);
            // Act
            var result = subject.IsRunning();
            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsRunning_ShouldPass_NotExpectedException()
        {
            // Arrange
            var exception = new Exception();
            _origin.IsRunning().Throws(exception);
            var subject = new SafeService(_origin);
            // Act
            Action action = () => subject.IsRunning();
            // Assert
            action.Should().ThrowExactly<Exception>();
        }

        [TestMethod]
        public void Start_ShouldBe_Origin_Start()
        {
            // Arrange
            var expected = Result.Ok();
            _origin.Start().Returns(expected);
            var subject = new SafeService(_origin);
            // Act
            var result = subject.Start();
            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public void Start_ShouldBeSuccess_WhenOriginThrows_ServiceAlreadyRunning()
        {
            // Arrange
            var exception = new InvalidOperationException("", new Win32Exception(ServiceAlreadyRunning));
            _origin.Start().Throws(exception);
            var subject = new SafeService(_origin);
            // Act
            var result = subject.Start();
            // Assert
            result.Success.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(typeof(InvalidOperationException))]
        [DataRow(typeof(System.ServiceProcess.TimeoutException))]
        [DataRow(typeof(TimeoutException))]
        public void Start_ShouldBeFailure_WhenOriginThrows_ExpectedException(Type exceptionType)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.Start().Throws(exception);
            var subject = new SafeService(_origin);
            // Act
            var result = subject.Start();
            // Assert
            result.Failure.Should().BeTrue();
            result.Exception.Should().BeSameAs(exception);
        }

        [TestMethod]
        public void Stop_ShouldBe_Origin_Stop()
        {
            // Arrange
            var expected = Result.Ok();
            _origin.Stop().Returns(expected);
            var subject = new SafeService(_origin);
            // Act
            var result = subject.Stop();
            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public void Stop_ShouldBeSuccess_WhenOriginThrows_ServiceNotRunning()
        {
            // Arrange
            var exception = new InvalidOperationException("", new Win32Exception(ServiceNotRunning));
            _origin.Stop().Throws(exception);
            var subject = new SafeService(_origin);
            // Act
            var result = subject.Stop();
            // Assert
            result.Success.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(typeof(InvalidOperationException))]
        [DataRow(typeof(System.ServiceProcess.TimeoutException))]
        [DataRow(typeof(TimeoutException))]
        public void Stop_ShouldBeFailure_WhenOriginThrows_ExpectedException(Type exceptionType)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.Stop().Throws(exception);
            var subject = new SafeService(_origin);
            // Act
            var result = subject.Stop();
            // Assert
            result.Failure.Should().BeTrue();
            result.Exception.Should().BeSameAs(exception);
        }
    }
}
