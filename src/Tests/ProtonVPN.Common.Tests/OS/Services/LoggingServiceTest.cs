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
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppServiceLogs;
using ProtonVPN.Common.OS.Services;

namespace ProtonVPN.Common.Tests.OS.Services
{
    [TestClass]
    public class LoggingServiceTest
    {
        private ILogger _logger;
        private IService _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _origin = Substitute.For<IService>();
        }

        [TestMethod]
        public void Name_ShouldBe_Origin_Name()
        {
            // Arrange
            const string name = "Our service";
            _origin.Name.Returns(name);
            LoggingService subject = new(_logger, _origin);

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
            LoggingService subject = new(_logger, _origin);

            // Act
            bool result = subject.Running();

            // Assert
            result.Should().Be(value);
        }

        [TestMethod]
        public async Task StartAsync_ShouldBe_Origin_StartAsync()
        {
            // Arrange
            Result expected = Result.Ok();
            CancellationToken cancellationToken = new();
            _origin.StartAsync(cancellationToken).Returns(expected);
            LoggingService subject = new(_logger, _origin);

            // Act
            Result result = await subject.StartAsync(cancellationToken);
            
            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public async Task StartAsync_ShouldPass_Exception()
        {
            // Arrange
            InvalidOperationException exception = new();
            CancellationToken cancellationToken = CancellationToken.None;
            _origin.StartAsync(cancellationToken).Throws(exception);
            LoggingService subject = new(_logger, _origin);

            // Act
            Func<Task> action = async () => await subject.StartAsync(cancellationToken);

            // Assert
            await action.Should().ThrowExactlyAsync<InvalidOperationException>();
        }

        [TestMethod]
        public async Task StartAsync_ShouldLog()
        {
            // Arrange
            Result expected = Result.Ok();
            CancellationToken cancellationToken = new();
            _origin.StartAsync(cancellationToken).Returns(expected);
            LoggingService subject = new(_logger, _origin);

            // Act
            await subject.StartAsync(cancellationToken);

            // Assert
            _logger.Received(2).Info<AppServiceStartLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        [DataTestMethod]
        [DataRow(typeof(InvalidOperationException), 1056)]
        [DataRow(typeof(InvalidOperationException), 1062)]
        [DataRow(typeof(OperationCanceledException), 0)]
        [DataRow(typeof(TimeoutException), 0)]
        public async Task StartAsync_ShouldLog_ExpectedException(Type exceptionType, int errorCode)
        {
            // Arrange
            Exception exception = exceptionType == typeof(InvalidOperationException)
                ? new InvalidOperationException("", new Win32Exception(errorCode))
                : (Exception)Activator.CreateInstance(exceptionType);
            CancellationToken cancellationToken = CancellationToken.None;
            _origin.StartAsync(cancellationToken).Throws(exception);
            LoggingService subject = new(_logger, _origin);

            // Act
            try
            {
                await subject.StartAsync(cancellationToken);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            { }

            // Assert
            _logger.Received(1).Info<AppServiceStartLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
            _logger.Received(1).Warn<AppServiceStartFailedLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        [TestMethod]
        public async Task StartAsync_ShouldLog_UnexpectedException()
        {
            // Arrange
            InvalidOperationException exception = new("");
            CancellationToken cancellationToken = CancellationToken.None;
            _origin.StartAsync(cancellationToken).Throws(exception);
            LoggingService subject = new(_logger, _origin);

            // Act
            try
            {
                await subject.StartAsync(cancellationToken);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            { }

            // Assert
            _logger.Received(1).Info<AppServiceStartLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
            _logger.Received(1).Error<AppServiceStartFailedLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        [TestMethod]
        public async Task StopAsync_ShouldBe_Origin_StopAsync()
        {
            // Arrange
            Result expected = Result.Fail();
            CancellationToken cancellationToken = new();
            _origin.StopAsync(cancellationToken).Returns(expected);
            LoggingService subject = new(_logger, _origin);

            // Act
            Result result = await subject.StopAsync(cancellationToken);

            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public async Task StopAsync_ShouldPass_Exception()
        {
            // Arrange
            Win32Exception exception = new();
            CancellationToken cancellationToken = CancellationToken.None;
            _origin.StopAsync(cancellationToken).Throws(exception);
            LoggingService subject = new(_logger, _origin);

            // Act
            Func<Task> action = async () => await subject.StopAsync(cancellationToken);

            // Assert
            await action.Should().ThrowExactlyAsync<Win32Exception>();
        }

        [TestMethod]
        public async Task StopAsync_ShouldLog()
        {
            // Arrange
            Result expected = Result.Fail();
            CancellationToken cancellationToken = new();
            _origin.StopAsync(cancellationToken).Returns(expected);
            LoggingService subject = new(_logger, _origin);

            // Act
            await subject.StopAsync(cancellationToken);

            // Assert
            _logger.Received(1).Info<AppServiceStopLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
            _logger.Received(1).Warn<AppServiceStopFailedLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        [DataTestMethod]
        [DataRow(typeof(InvalidOperationException), 1056)]
        [DataRow(typeof(InvalidOperationException), 1062)]
        [DataRow(typeof(OperationCanceledException), 0)]
        [DataRow(typeof(TimeoutException), 0)]
        public async Task StopAsync_ShouldLog_ExpectedException(Type exceptionType, int errorCode)
        {
            // Arrange
            Exception exception = exceptionType == typeof(InvalidOperationException) 
                ? new InvalidOperationException("", new Win32Exception(errorCode))
                : (Exception)Activator.CreateInstance(exceptionType);
            CancellationToken cancellationToken = CancellationToken.None;
            _origin.StopAsync(cancellationToken).Throws(exception);
            LoggingService subject = new(_logger, _origin);

            // Act
            try
            {
                await subject.StopAsync(cancellationToken);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            { }

            // Assert
            _logger.Received(1).Info<AppServiceStopLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
            _logger.Received(1).Warn<AppServiceStopFailedLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        [TestMethod]
        public async Task StopAsync_ShouldLog_UnexpectedException()
        {
            // Arrange
            InvalidOperationException exception = new("");
            CancellationToken cancellationToken = CancellationToken.None;
            _origin.StopAsync(cancellationToken).Throws(exception);
            LoggingService subject = new(_logger, _origin);

            // Act
            try
            {
                await subject.StopAsync(cancellationToken);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            { }

            // Assert
            _logger.Received(1).Info<AppServiceStopLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
            _logger.Received(1).Error<AppServiceStopFailedLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }
    }
}
