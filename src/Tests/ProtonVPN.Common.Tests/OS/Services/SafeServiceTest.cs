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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.OS.Services;

namespace ProtonVPN.Common.Tests.OS.Services
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
            SafeService subject = new(_origin);

            // Act
            string result = subject.Name;

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
            SafeService subject = new(_origin);

            // Act
            bool result = subject.Running();

            // Assert
            result.Should().Be(value);
        }

        [DataTestMethod]
        [DataRow(typeof(Win32Exception))]
        public void Running_ShouldBeFalse_WhenOriginThrows_ExpectedException(Type exceptionType)
        {
            // Arrange
            Exception exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.Running().Throws(exception);
            SafeService subject = new(_origin);

            // Act
            bool result = subject.Running();

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Running_ShouldPass_NotExpectedException()
        {
            // Arrange
            Exception exception = new();
            _origin.Running().Throws(exception);
            SafeService subject = new(_origin);

            // Act
            Action action = () => subject.Running();

            // Assert
            action.Should().ThrowExactly<Exception>();
        }

        [TestMethod]
        public async Task StartAsync_ShouldBe_Origin_StartAsync()
        {
            // Arrange
            Result expected = Result.Ok();
            CancellationToken cancellationToken = new();
            _origin.StartAsync(cancellationToken).Returns(expected);
            SafeService subject = new(_origin);

            // Act
            Result result = await subject.StartAsync(cancellationToken);

            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public async Task StartAsync_ShouldBeSuccess_WhenOriginThrows_ServiceAlreadyRunning()
        {
            // Arrange
            InvalidOperationException exception = new("", new Win32Exception(ServiceAlreadyRunning));
            CancellationToken cancellationToken = CancellationToken.None;
            _origin.StartAsync(cancellationToken).Throws(exception);
            SafeService subject = new(_origin);

            // Act
            Result result = await subject.StartAsync(cancellationToken);

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
            Exception exception = (Exception)Activator.CreateInstance(exceptionType);
            CancellationToken cancellationToken = CancellationToken.None;
            _origin.StartAsync(cancellationToken).Throws(exception);
            SafeService subject = new(_origin);

            // Act
            Result result = await subject.StartAsync(cancellationToken);

            // Assert
            result.Failure.Should().BeTrue();
            result.Exception.Should().BeSameAs(exception);
        }

        [TestMethod]
        public async Task StopAsync_ShouldBe_Origin_StopAsync()
        {
            // Arrange
            Result expected = Result.Ok();
            CancellationToken cancellationToken = new();
            _origin.StopAsync(cancellationToken).Returns(expected);
            SafeService subject = new(_origin);

            // Act
            Result result = await subject.StopAsync(cancellationToken);

            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public async Task StopAsync_ShouldBeSuccess_WhenOriginThrows_ServiceNotRunning()
        {
            // Arrange
            InvalidOperationException exception = new("", new Win32Exception(ServiceNotRunning));
            CancellationToken cancellationToken = CancellationToken.None;
            _origin.StopAsync(cancellationToken).Throws(exception);
            SafeService subject = new(_origin);

            // Act
            Result result = await subject.StopAsync(cancellationToken);

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
            Exception exception = (Exception)Activator.CreateInstance(exceptionType);
            CancellationToken cancellationToken = CancellationToken.None;
            _origin.StopAsync(cancellationToken).Throws(exception);
            SafeService subject = new(_origin);

            // Act
            Result result = await subject.StopAsync(cancellationToken);

            // Assert
            result.Failure.Should().BeTrue();
            result.Exception.Should().BeSameAs(exception);
        }
    }
}
