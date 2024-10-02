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
using ProtonVPN.ProcessCommunication.Common.Tests;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Registration;

namespace ProtonVPN.ProcessCommunication.Service.Tests
{
    [TestClass]
    public class GrpcServerTest : GrpcServerTestBase
    {
        private ILogger _logger;
        private IVpnController _vpnController;
        private IUpdateController _updateController;
        private IServiceServerPortRegister _serviceServerPortRegister;
        private IAppServerPortRegister _appServerPortRegister;

        [TestInitialize]
        public override void Initialize()
        {
            _logger = Substitute.For<ILogger>();
            _vpnController = Substitute.For<IVpnController>();
            _updateController = Substitute.For<IUpdateController>();
            _serviceServerPortRegister = Substitute.For<IServiceServerPortRegister>();
            _appServerPortRegister = Substitute.For<IAppServerPortRegister>();
            base.Initialize();
        }

        protected override IGrpcServer CreateGrpcServer()
        {
            return new GrpcServer(_logger, _vpnController, _updateController, _serviceServerPortRegister, _appServerPortRegister);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            _logger = null;
            _vpnController = null;
            _updateController = null;
            _serviceServerPortRegister = null;
            _appServerPortRegister = null;
            base.Initialize();
        }

        [TestMethod]
        public override void TestCreateAndStart()
        {
            base.TestCreateAndStart();
            _serviceServerPortRegister.Received(1).Write(Arg.Any<int>());
        }

        [TestMethod]
        public override void TestCreateAndStart_WithoutEventListener()
        {
            base.TestCreateAndStart_WithoutEventListener();
            _serviceServerPortRegister.Received(1).Write(Arg.Any<int>());
        }
    }
}