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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Common.Channels;
using ProtonVPN.ProcessCommunication.Common.Tests;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Registration;

namespace ProtonVPN.ProcessCommunication.App.Tests
{
    [TestClass]
    public class AppGrpcClientTest
    {
        private const int SERVICE_SERVER_PORT = 1;
        private const int APP_SERVER_PORT = 2;

        private ILogger _logger;
        private IServiceServerPortRegister _serviceServerPortRegister;
        private IGrpcServer _grpcServer;
        private IGrpcChannelWrapper _grpcChannelWrapper;
        private IGrpcChannelWrapperFactory _grpcChannelWrapperFactory;
        private IAppGrpcClient _appGrpcClient;

        [TestInitialize]
        public void Initialize()
        {
            _logger = Substitute.For<ILogger>();
            _serviceServerPortRegister = Substitute.For<IServiceServerPortRegister>();
            _grpcServer = Substitute.For<IGrpcServer>();
            _grpcChannelWrapper = Substitute.For<IGrpcChannelWrapper>();
            _grpcChannelWrapper.CreateService<IVpnController>().Returns(c => Substitute.For<IVpnController>());
            _grpcChannelWrapper.CreateService<IUpdateController>().Returns(c => Substitute.For<IUpdateController>());
            _grpcChannelWrapperFactory = Substitute.For<IGrpcChannelWrapperFactory>();
            _grpcChannelWrapperFactory.Create(Arg.Any<int>()).Returns(_grpcChannelWrapper);
            _appGrpcClient = new AppGrpcClient(_logger, _serviceServerPortRegister, _grpcServer, _grpcChannelWrapperFactory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _logger = null;
            _serviceServerPortRegister = null;
            _grpcServer = null;
            _grpcChannelWrapper = null;
            _grpcChannelWrapperFactory = null;
            _appGrpcClient = null;
        }

        [TestMethod]
        public async Task TestCreateAsync()
        {
            _serviceServerPortRegister.ReadAsync(Arg.Any<CancellationToken>()).Returns(SERVICE_SERVER_PORT);
            _grpcServer.Port.Returns(APP_SERVER_PORT);

            await _appGrpcClient.CreateAsync();

            Assert.IsNotNull(_appGrpcClient.VpnController);
        }

        [TestMethod]
        public async Task TestCreateAsync_NoRaceConditionOccurs()
        {
            _serviceServerPortRegister.ReadAsync(Arg.Any<CancellationToken>()).Returns(SERVICE_SERVER_PORT);
            _grpcServer.Port.Returns(APP_SERVER_PORT);
            int numOfCalls = 64;

            ParallelTestRunner.ExecuteInParallel(numOfCalls,
                (int _) => _appGrpcClient.CreateAsync());

            Assert.IsNotNull(_appGrpcClient.VpnController);
            await _serviceServerPortRegister.Received(1).ReadAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task TestCreateAsync_WhenServiceControllerIsNotNull()
        {
            _serviceServerPortRegister.ReadAsync(Arg.Any<CancellationToken>()).Returns(SERVICE_SERVER_PORT);
            _grpcServer.Port.Returns(APP_SERVER_PORT);

            await _appGrpcClient.CreateAsync();

            Assert.IsNotNull(_appGrpcClient.VpnController);
            await _serviceServerPortRegister.Received(1).ReadAsync(Arg.Any<CancellationToken>());
            IVpnController vpnController = _appGrpcClient.VpnController;

            await _appGrpcClient.CreateAsync();

            Assert.AreEqual(vpnController, _appGrpcClient.VpnController);
            await _serviceServerPortRegister.Received(1).ReadAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task TestRecreateAsync()
        {
            _serviceServerPortRegister.ReadAsync(Arg.Any<CancellationToken>()).Returns(SERVICE_SERVER_PORT);
            _grpcServer.Port.Returns(APP_SERVER_PORT);

            await _appGrpcClient.RecreateAsync();

            Assert.IsNotNull(_appGrpcClient.VpnController);
        }

        [TestMethod]
        public async Task TestRecreateAsync_NoRaceConditionOccurs()
        {
            _serviceServerPortRegister.ReadAsync(Arg.Any<CancellationToken>()).Returns(SERVICE_SERVER_PORT);
            _grpcServer.Port.Returns(APP_SERVER_PORT);
            int numOfCalls = 64;

            ParallelTestRunner.ExecuteInParallel(numOfCalls,
                (int _) => _appGrpcClient.RecreateAsync());

            Assert.IsNotNull(_appGrpcClient.VpnController);
            await _serviceServerPortRegister.Received(numOfCalls).ReadAsync(Arg.Any<CancellationToken>());
            await _grpcChannelWrapper.Received(numOfCalls - 1).ShutdownAsync();
        }

        [TestMethod]
        public async Task TestRecreateAsync_WhenServiceControllerIsNotNull()
        {
            _serviceServerPortRegister.ReadAsync(Arg.Any<CancellationToken>()).Returns(SERVICE_SERVER_PORT);
            _grpcServer.Port.Returns(APP_SERVER_PORT);

            await _appGrpcClient.RecreateAsync();

            Assert.IsNotNull(_appGrpcClient.VpnController);
            await _serviceServerPortRegister.Received(1).ReadAsync(Arg.Any<CancellationToken>());
            IVpnController vpnController = _appGrpcClient.VpnController;

            await _appGrpcClient.RecreateAsync();

            Assert.IsNotNull(_appGrpcClient.VpnController);
            await _serviceServerPortRegister.Received(2).ReadAsync(Arg.Any<CancellationToken>());
            Assert.AreNotEqual(vpnController, _appGrpcClient.VpnController);
            await _grpcChannelWrapper.Received(1).ShutdownAsync();
        }

        [TestMethod]
        public async Task TestGetServiceControllerOrThrowAsync()
        {
            _serviceServerPortRegister.ReadAsync(Arg.Any<CancellationToken>()).Returns(SERVICE_SERVER_PORT);
            _grpcServer.Port.Returns(APP_SERVER_PORT);
            TimeSpan timeout = TimeSpan.FromSeconds(0);
            await _appGrpcClient.CreateAsync();

            await _appGrpcClient.GetServiceControllerOrThrowAsync<IVpnController>(timeout);
            await _appGrpcClient.GetServiceControllerOrThrowAsync<IUpdateController>(timeout);
        }

        [TestMethod]
        public async Task TestGetServiceControllerOrThrowAsync_WhenServiceControllerIsCreatedAfterTheCall()
        {
            _serviceServerPortRegister.ReadAsync(Arg.Any<CancellationToken>()).Returns(SERVICE_SERVER_PORT);
            _grpcServer.Port.Returns(APP_SERVER_PORT);
            TimeSpan timeout = TimeSpan.FromSeconds(5);

            Task<IVpnController> task = _appGrpcClient.GetServiceControllerOrThrowAsync<IVpnController>(timeout);
            await Task.Delay(TimeSpan.FromSeconds(1));
            await _appGrpcClient.CreateAsync();
            IVpnController vpnController = await task;

            Assert.IsNotNull(vpnController);
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task TestGetServiceControllerOrThrowAsync_WhenServiceControllerIsNullAndTimeoutZero()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(0);

            await _appGrpcClient.GetServiceControllerOrThrowAsync<IVpnController>(timeout);
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task TestGetServiceControllerOrThrowAsync_WhenServiceControllerIsNullAndTimeoutThreeSeconds()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(3);

            await _appGrpcClient.GetServiceControllerOrThrowAsync<IVpnController>(timeout);
        }
    }
}