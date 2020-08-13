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

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Service.Vpn;

namespace ProtonVPN.App.Test.Core.Service.Vpn
{
    [TestClass]
    public class ServiceStartDecoratorTest
    {
        private ILogger _logger;
        private IVpnServiceManager _decorated;
        private IModals _modals;
        private IService _baseFilteringEngineService;

        [TestInitialize]
        public void Initialize()
        {
            _logger = Substitute.For<ILogger>();
            _decorated = Substitute.For<IVpnServiceManager>();
            _modals = Substitute.For<IModals>();
            _baseFilteringEngineService = Substitute.For<IService>();
        }

        [TestMethod]
        public async Task Connect_ShouldNotBeExecuted_WhenBfeIsNotRunning()
        {
            // Arrange
            _baseFilteringEngineService.Running().Returns(false);
            var sut = new ServiceStartDecorator(_logger, _decorated, _modals, _baseFilteringEngineService);

            // Act
            await sut.Connect(default);

            // Assert
            await _decorated.DidNotReceive().Connect(default);
        }

        [TestMethod]
        public async Task UpdateServers_ShouldNotBeExecuted_WhenBfeIsNotRunning()
        {
            // Arrange
            _baseFilteringEngineService.Running().Returns(false);
            var sut = new ServiceStartDecorator(_logger, _decorated, _modals, _baseFilteringEngineService);

            // Act
            await sut.UpdateServers(default, default);

            // Assert
            await _decorated.DidNotReceive().UpdateServers(default, default);
        }

        [TestMethod]
        public async Task RepeatState_ShouldNotBeExecuted_WhenBfeIsNotRunning()
        {
            // Arrange
            _baseFilteringEngineService.Running().Returns(false);
            var sut = new ServiceStartDecorator(_logger, _decorated, _modals, _baseFilteringEngineService);

            // Act
            await sut.RepeatState();

            // Assert
            await _decorated.DidNotReceive().RepeatState();
        }
    }
}
