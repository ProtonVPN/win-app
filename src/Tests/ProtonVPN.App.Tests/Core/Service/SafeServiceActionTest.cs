/* Copyright (c) 2023 Proton AG
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
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Core.Service;

namespace ProtonVPN.App.Tests.Core.Service
{
    [TestClass]
    public class SafeServiceActionTest
    {
        private ILogger _logger;
        private IService _service;
        private IServiceEnabler _serviceEnabler;

        [TestInitialize]
        public void Initialize()
        {
            _logger = Substitute.For<ILogger>();
            _service = Substitute.For<IService>();
            _serviceEnabler = Substitute.For<IServiceEnabler>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _logger = null;
            _service = null;
            _serviceEnabler = null;
        }

        [TestMethod]
        public async Task ItShouldNotExecuteActionIfServiceIsDisabled()
        {
            // Arrange
            _serviceEnabler.GetServiceEnabledResult(Arg.Any<IService>()).Returns(Result.Fail());
            SafeServiceAction sut = new SafeServiceAction(_service, _logger, _serviceEnabler);
            int timesCalled = 0;
            Func<Task<Result>> action = () =>
            {
                timesCalled++;
                return Task.FromResult(Result.Ok());
            };

            // Act
            await sut.InvokeServiceAction(action);

            // Assert
            timesCalled.Should().Be(0);
        }

        [TestMethod]
        public async Task ItShouldStartServiceWhenItIsNotRunning()
        {
            // Arrange
            _service.StartAsync(Arg.Any<CancellationToken>()).Returns(Result.Ok);
            _service.Exists().Returns(true);
            _serviceEnabler.GetServiceEnabledResult(Arg.Any<IService>()).Returns(Result.Ok);
            SafeServiceAction sut = new SafeServiceAction(_service, _logger, _serviceEnabler);
            int timesCalled = 0;

            // Act
            await sut.InvokeServiceAction(() =>
            {
                if (timesCalled == 0)
                {
                    timesCalled++;
                    throw new EndpointNotFoundException();
                }

                timesCalled++;

                return Task.FromResult(Result.Ok());
            });

            // Assert
            timesCalled.Should().Be(2);
            await _service.Received().StartAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        [DataRow(typeof(TimeoutException))]
        [DataRow(typeof(CommunicationException))]
        public async Task ItShouldHandleExceptions(Type e)
        {
            // Arrange
            Exception ex = (Exception)Activator.CreateInstance(e);
            SafeServiceAction sut = new(_service, _logger, _serviceEnabler);
            _serviceEnabler.GetServiceEnabledResult(Arg.Any<IService>()).Returns(Result.Ok);

            // Act
            Result result = await sut.InvokeServiceAction(() => throw ex);

            // Assert
            result.Failure.Should().BeTrue();
        }

        [TestMethod]
        public async Task ItShouldReturnFailureResultWhenServiceIsMissing()
        {
            // Arrange
            _service.Exists().Returns(false);
            SafeServiceAction sut = new SafeServiceAction(_service, _logger, _serviceEnabler);

            // Act
            Result result = await sut.InvokeServiceAction(() => Task.FromResult(Result.Ok()));

            // Assert
            result.Failure.Should().BeTrue();
        }
    }
}