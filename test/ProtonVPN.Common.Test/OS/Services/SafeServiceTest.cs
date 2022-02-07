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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
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
        public void Running_ShouldBe_Origin_Running(bool value)
        {
            // Arrange
            _origin.Running().Returns(value);
            var subject = new SafeService(_origin);

            // Act
            var result = subject.Running();

            // Assert
            result.Should().Be(value);
        }

        [DataTestMethod]
        [DataRow(typeof(Win32Exception))]
        public void Running_ShouldBeFalse_WhenOriginThrows_ExpectedException(Type exceptionType)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.Running().Throws(exception);
            var subject = new SafeService(_origin);

            // Act
            var result = subject.Running();

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Running_ShouldPass_NotExpectedException()
        {
            // Arrange
            var exception = new Exception();
            _origin.Running().Throws(exception);
            var subject = new SafeService(_origin);

            // Act
            Action action = () => subject.Running();

            // Assert
            action.Should().ThrowExactly<Exception>();
        }

        [TestMethod]
        public async Task StartAsync_ShouldBe_Origin_StartAsync()
        {
            // Arrange
            var expected = Result.Ok();
            var cancellationToken = new CancellationToken();
            _origin.StartAsync(cancellationToken).Returns(expected);
            var subject = new SafeService(_origin);

            // Act
            var result = await subject.StartAsync(cancellationToken);

            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public async Task StartAsync_ShouldBeSuccess_WhenOriginThrows_ServiceAlreadyRunning()
        {
            // Arrange
            var exception = new InvalidOperationException("", new Win32Exception(ServiceAlreadyRunning));
            var cancellationToken = CancellationToken.None;
            _origin.StartAsync(cancellationToken).Throws(exception);
            var subject = new SafeService(_origin);

            // Act
            var result = await subject.StartAsync(cancellationToken);

            // Assert
            result.Success.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(typeof(InvalidOperationException))]
        [DataRow(typeof(System.ServiceProcess.TimeoutException))]
        [DataRow(typeof(TimeoutException))]
        public async Task StartAsync_ShouldBeFailure_WhenOriginThrows_ExpectedException(Type exceptionType)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            var cancellationToken = CancellationToken.None;
            _origin.StartAsync(cancellationToken).Throws(exception);
            var subject = new SafeService(_origin);

            // Act
            var result = await subject.StartAsync(cancellationToken);

            // Assert
            result.Failure.Should().BeTrue();
            result.Exception.Should().BeSameAs(exception);
        }

        [TestMethod]
        public async Task StopAsync_ShouldBe_Origin_StopAsync()
        {
            // Arrange
            var expected = Result.Ok();
            var cancellationToken = new CancellationToken();
            _origin.StopAsync(cancellationToken).Returns(expected);
            var subject = new SafeService(_origin);

            // Act
            var result = await subject.StopAsync(cancellationToken);

            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public async Task StopAsync_ShouldBeSuccess_WhenOriginThrows_ServiceNotRunning()
        {
            // Arrange
            var exception = new InvalidOperationException("", new Win32Exception(ServiceNotRunning));
            var cancellationToken = CancellationToken.None;
            _origin.StopAsync(cancellationToken).Throws(exception);
            var subject = new SafeService(_origin);

            // Act
            var result = await subject.StopAsync(cancellationToken);

            // Assert
            result.Success.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(typeof(InvalidOperationException))]
        [DataRow(typeof(System.ServiceProcess.TimeoutException))]
        [DataRow(typeof(TimeoutException))]
        public async Task StopAsync_ShouldBeFailure_WhenOriginThrows_ExpectedException(Type exceptionType)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            var cancellationToken = CancellationToken.None;
            _origin.StopAsync(cancellationToken).Throws(exception);
            var subject = new SafeService(_origin);

            // Act
            var result = await subject.StopAsync(cancellationToken);

            // Assert
            result.Failure.Should().BeTrue();
            result.Exception.Should().BeSameAs(exception);
        }
    }
}
