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
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Services;

namespace ProtonVPN.Common.Test.OS.Services
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
            var subject = new LoggingService(_logger, _origin);

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
            var subject = new LoggingService(_logger, _origin);

            // Act
            var result = subject.Running();

            // Assert
            result.Should().Be(value);
        }

        [TestMethod]
        public async Task StartAsync_ShouldBe_Origin_StartAsync()
        {
            // Arrange
            var expected = Result.Ok();
            var cancellationToken = new CancellationToken();
            _origin.StartAsync(cancellationToken).Returns(expected);
            var subject = new LoggingService(_logger, _origin);

            // Act
            var result = await subject.StartAsync(cancellationToken);
            
            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public async Task StartAsync_ShouldPass_Exception()
        {
            // Arrange
            var exception = new InvalidOperationException();
            var cancellationToken = CancellationToken.None;
            _origin.StartAsync(cancellationToken).Throws(exception);
            var subject = new LoggingService(_logger, _origin);

            // Act
            Func<Task> action = async () => await subject.StartAsync(cancellationToken);

            // Assert
            await action.Should().ThrowExactlyAsync<InvalidOperationException>();
        }

        [TestMethod]
        public async Task StartAsync_ShouldLog()
        {
            // Arrange
            var expected = Result.Ok();
            var cancellationToken = new CancellationToken();
            _origin.StartAsync(cancellationToken).Returns(expected);
            var subject = new LoggingService(_logger, _origin);

            // Act
            await subject.StartAsync(cancellationToken);

            // Assert
            _logger.Received(2).Info(Arg.Any<string>());
        }

        [DataTestMethod]
        [DataRow(typeof(InvalidOperationException), 1056)]
        [DataRow(typeof(InvalidOperationException), 1062)]
        [DataRow(typeof(OperationCanceledException), 0)]
        [DataRow(typeof(TimeoutException), 0)]
        public async Task StartAsync_ShouldLog_ExpectedException(Type exceptionType, int errorCode)
        {
            // Arrange
            var exception = exceptionType == typeof(InvalidOperationException)
                ? new InvalidOperationException("", new Win32Exception(errorCode))
                : (Exception)Activator.CreateInstance(exceptionType);
            var cancellationToken = CancellationToken.None;
            _origin.StartAsync(cancellationToken).Throws(exception);
            var subject = new LoggingService(_logger, _origin);

            // Act
            try
            {
                await subject.StartAsync(cancellationToken);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            { }

            // Assert
            _logger.Received(2).Info(Arg.Any<string>());
        }

        [TestMethod]
        public async Task StartAsync_ShouldLog_UnexpectedException()
        {
            // Arrange
            var exception = new InvalidOperationException("");
            var cancellationToken = CancellationToken.None;
            _origin.StartAsync(cancellationToken).Throws(exception);
            var subject = new LoggingService(_logger, _origin);

            // Act
            try
            {
                await subject.StartAsync(cancellationToken);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            { }

            // Assert
            _logger.Received(1).Info(Arg.Any<string>());
            _logger.Received(1).Error(Arg.Any<string>());
        }

        [TestMethod]
        public async Task StopAsync_ShouldBe_Origin_StopAsync()
        {
            // Arrange
            var expected = Result.Fail();
            var cancellationToken = new CancellationToken();
            _origin.StopAsync(cancellationToken).Returns(expected);
            var subject = new LoggingService(_logger, _origin);

            // Act
            var result = await subject.StopAsync(cancellationToken);

            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public async Task StopAsync_ShouldPass_Exception()
        {
            // Arrange
            var exception = new Win32Exception();
            var cancellationToken = CancellationToken.None;
            _origin.StopAsync(cancellationToken).Throws(exception);
            var subject = new LoggingService(_logger, _origin);

            // Act
            Func<Task> action = async () => await subject.StopAsync(cancellationToken);

            // Assert
            await action.Should().ThrowExactlyAsync<Win32Exception>();
        }

        [TestMethod]
        public async Task StopAsync_ShouldLog()
        {
            // Arrange
            var expected = Result.Fail();
            var cancellationToken = new CancellationToken();
            _origin.StopAsync(cancellationToken).Returns(expected);
            var subject = new LoggingService(_logger, _origin);

            // Act
            await subject.StopAsync(cancellationToken);

            // Assert
            _logger.Received(2).Info(Arg.Any<string>());
        }

        [DataTestMethod]
        [DataRow(typeof(InvalidOperationException), 1056)]
        [DataRow(typeof(InvalidOperationException), 1062)]
        [DataRow(typeof(OperationCanceledException), 0)]
        [DataRow(typeof(TimeoutException), 0)]
        public async Task StopAsync_ShouldLog_ExpectedException(Type exceptionType, int errorCode)
        {
            // Arrange
            var exception = exceptionType == typeof(InvalidOperationException) 
                ? new InvalidOperationException("", new Win32Exception(errorCode))
                : (Exception)Activator.CreateInstance(exceptionType);
            var cancellationToken = CancellationToken.None;
            _origin.StopAsync(cancellationToken).Throws(exception);
            var subject = new LoggingService(_logger, _origin);

            // Act
            try
            {
                await subject.StopAsync(cancellationToken);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            { }

            // Assert
            _logger.Received(2).Info(Arg.Any<string>());
        }

        [TestMethod]
        public async Task StopAsync_ShouldLog_UnexpectedException()
        {
            // Arrange
            var exception = new InvalidOperationException("");
            var cancellationToken = CancellationToken.None;
            _origin.StopAsync(cancellationToken).Throws(exception);
            var subject = new LoggingService(_logger, _origin);

            // Act
            try
            {
                await subject.StopAsync(cancellationToken);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            { }

            // Assert
            _logger.Received(1).Info(Arg.Any<string>());
            _logger.Received(1).Error(Arg.Any<string>());
        }
    }
}
