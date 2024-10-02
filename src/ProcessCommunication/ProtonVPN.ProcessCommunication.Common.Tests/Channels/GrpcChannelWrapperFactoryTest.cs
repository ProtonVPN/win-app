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

namespace ProtonVPN.ProcessCommunication.Common.Tests.Channels
{
    [TestClass]
    public class GrpcChannelWrapperFactoryTest
    {
        private ILogger _logger;
        private GrpcChannelWrapperFactory _grpcChannelWrapperFactory;

        [TestInitialize]
        public void Initialize()
        {
            _logger = Substitute.For<ILogger>();
            _grpcChannelWrapperFactory = new GrpcChannelWrapperFactory(_logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _logger = null;
            _grpcChannelWrapperFactory = null;
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(80)]
        [DataRow(65535)]
        public void TestCreate(int serverPort)
        {
            IGrpcChannelWrapper result = _grpcChannelWrapperFactory.Create(serverPort);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ResolvedTarget);
            Assert.AreEqual(result.ResolvedTarget, $"localhost:{serverPort}");
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(65536)]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreate_Throws(int serverPort)
        {
            _grpcChannelWrapperFactory.Create(serverPort);
        }
    }
}