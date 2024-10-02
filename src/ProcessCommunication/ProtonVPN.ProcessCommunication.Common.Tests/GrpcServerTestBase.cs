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
using ProtonVPN.ProcessCommunication.Contracts;

namespace ProtonVPN.ProcessCommunication.Common.Tests
{
    [TestClass]
    public abstract class GrpcServerTestBase
    {
        private bool _isOnStartEventCalled;
        private IGrpcServer _grpcServer;

        [TestInitialize]
        public virtual void Initialize()
        {
            _isOnStartEventCalled = false;
            _grpcServer = CreateGrpcServer();
        }

        protected abstract IGrpcServer CreateGrpcServer();

        [TestCleanup]
        public virtual void Cleanup()
        {
            _isOnStartEventCalled = false;
            _grpcServer = null;
        }

        [TestMethod]
        public virtual void TestCreateAndStart()
        {
            _grpcServer.OnStart += OnGrpcServerStart;

            _grpcServer.CreateAndStart();

            Assert.IsNotNull(_grpcServer.Port);
            Assert.IsTrue(_isOnStartEventCalled);
        }

        [TestMethod]
        public virtual void TestCreateAndStart_WithoutEventListener()
        {
            _grpcServer.CreateAndStart();

            Assert.IsNotNull(_grpcServer.Port);
            Assert.IsFalse(_isOnStartEventCalled);
        }

        private void OnGrpcServerStart(object sender, EventArgs e)
        {
            _isOnStartEventCalled = true;
        }

        [TestMethod]
        public virtual async Task ShutdownAsync_WhenServerIsNull()
        {
            await _grpcServer.ShutdownAsync();
        }

        [TestMethod]
        public virtual async Task ShutdownAsync_WhenServerIsNotNull()
        {
            _grpcServer.CreateAndStart();

            await _grpcServer.ShutdownAsync();
        }

        [TestMethod]
        public virtual async Task KillAsync_WhenServerIsNull()
        {
            await _grpcServer.KillAsync();
        }

        [TestMethod]
        public virtual async Task KillAsync_WhenServerIsNotNull()
        {
            _grpcServer.CreateAndStart();

            await _grpcServer.KillAsync();
        }
    }
}