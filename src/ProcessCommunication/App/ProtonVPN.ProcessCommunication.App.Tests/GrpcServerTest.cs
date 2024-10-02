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

namespace ProtonVPN.ProcessCommunication.App.Tests
{
    [TestClass]
    public class GrpcServerTest : GrpcServerTestBase
    {
        private ILogger _logger;
        private IAppController _appController;

        [TestInitialize]
        public override void Initialize()
        {
            _logger = Substitute.For<ILogger>();
            _appController = Substitute.For<IAppController>();
            base.Initialize();
        }

        protected override IGrpcServer CreateGrpcServer()
        {
            return new GrpcServer(_logger, _appController);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            _logger = null;
            _appController = null;
            base.Initialize();
        }
    }
}