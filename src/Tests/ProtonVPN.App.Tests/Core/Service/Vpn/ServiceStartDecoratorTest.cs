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

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Service;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.App.Tests.Core.Service.Vpn
{
    [TestClass]
    public class VpnServiceActionDecoratorTest
    {
        private ISafeServiceAction _service;
        private IVpnServiceManager _decorated;
        private IModals _modals;
        private IService _baseFilteringEngineService;
        private IServiceEnabler _serviceEnabler;
        private ILogger _logger;
        private VpnSystemService _vpnService;

        [TestInitialize]
        public void Initialize()
        {
            _service = Substitute.For<ISafeServiceAction>();
            _decorated = Substitute.For<IVpnServiceManager>();
            _modals = Substitute.For<IModals>();
            _baseFilteringEngineService = Substitute.For<IService>();
            _serviceEnabler = Substitute.For<IServiceEnabler>();
            _logger = Substitute.For<ILogger>();
            
            _vpnService = new VpnSystemService(Substitute.For<IService>(), _logger, _serviceEnabler);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _service = null;
            _decorated = null;
            _modals = null;
            _baseFilteringEngineService = null;
            _serviceEnabler = null;
            _logger = null;
            _vpnService = null;
        }

        [TestMethod]
        public async Task Connect_ShouldNotBeExecuted_WhenBfeIsNotRunning()
        {
            // Arrange
            _baseFilteringEngineService.Running().Returns(false);
            VpnServiceActionDecorator sut = new(_service, _decorated, _modals, _baseFilteringEngineService);
        
            // Act
            await sut.Connect(default);
        
            // Assert
            await _decorated.DidNotReceive().Connect(default);
        }
        
        [TestMethod]
        public async Task RepeatState_ShouldNotBeExecuted_WhenBfeIsNotRunning()
        {
            // Arrange
            _baseFilteringEngineService.Running().Returns(false);
            VpnServiceActionDecorator sut = new(_service, _decorated, _modals, _baseFilteringEngineService);
        
            // Act
            await sut.RepeatState();
        
            // Assert
            await _decorated.DidNotReceive().RepeatState();
        }
        
        [TestMethod]
        public async Task Connect_ShouldNotBeExecuted_WhenServiceIsDisabled()
        {
            // Arrange
            _vpnService.Enabled().Returns(false);
            _baseFilteringEngineService.Running().Returns(true);
            VpnServiceActionDecorator sut = new(_service, _decorated, _modals, _baseFilteringEngineService);
        
            // Act
            await sut.Connect(default);
        
            // Assert
            await _decorated.DidNotReceive().Connect(default);
        }
    }
}