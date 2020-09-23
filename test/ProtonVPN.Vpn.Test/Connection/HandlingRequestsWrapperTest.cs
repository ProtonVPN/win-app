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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Connection;

namespace ProtonVPN.Vpn.Test.Connection
{
    [TestClass]
    public class HandlingRequestsWrapperTest
    {
        private ILogger _logger;
        private TaskQueue _taskQueue;
        private ISingleVpnConnection _origin;

        private VpnEndpoint _endpoint;
        private VpnCredentials _credentials;
        private VpnConfig _config;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _taskQueue = new TaskQueue();
            _origin = Substitute.For<ISingleVpnConnection>();

            _endpoint = new VpnEndpoint(new VpnHost("proton.vpn", "135.27.46.203"), VpnProtocol.OpenVpnTcp, 777);
            _credentials = new VpnCredentials("username", "password");
            _config = new VpnConfig(new Dictionary<VpnProtocol, IReadOnlyCollection<int>>(), new List<string>(), false);
        }

        [TestMethod]
        public async Task Connect_ShouldCall_OriginDisconnect()
        {
            // Arrange
            var subject = new HandlingRequestsWrapper(_logger, _taskQueue, _origin);

            // Act
            subject.Connect(_endpoint, _credentials, _config);
            await _taskQueue.WaitForLastTask();

            // Assert
            _origin.Received().Disconnect(VpnError.None);
        }

        [TestMethod]
        public async Task Connect_ShouldCall_OriginConnect()
        {
            // Arrange
            SetupDisconnect();
            var subject = new HandlingRequestsWrapper(_logger, _taskQueue, _origin);

            // Act
            subject.Connect(_endpoint, _credentials, _config);
            await _taskQueue.WaitForLastTask();

            // Assert
            _origin.Received().Connect(_endpoint, _credentials, _config);
        }

        [TestMethod]
        public async Task DisconnectError_ShouldBe_Unknown_WhenUnexpectedlyDisconnected_WithoutAnError()
        {
            // Arrange
            var disconnectError = VpnError.None;

            SetupDisconnect();
            SetupConnect();
            SetupUnexpectedDisconnect(VpnError.None);

            var subject = new HandlingRequestsWrapper(_logger, _taskQueue, _origin);
            subject.StateChanged += (s, e) =>
            {
                if (e.Data.Status == VpnStatus.Disconnected)
                {
                    disconnectError = e.Data.Error;
                }
            };

            // Act
            subject.Connect(_endpoint, _credentials, _config);
            await _taskQueue.WaitForLastTask();

            // Assert
            disconnectError.Should().Be(VpnError.Unknown);
        }

        #region Helpers

        private void SetupDisconnect()
        {
            _origin
                .When(x => x.Disconnect(Arg.Any<VpnError>()))
                .Do(x => RaiseStateChangedEvents(
                    new[]
                    {
                        new VpnState(VpnStatus.Disconnecting, x.Arg<VpnError>()),
                        new VpnState(VpnStatus.Disconnected, x.Arg<VpnError>())
                    }));
        }

        private void SetupConnect()
        {
            _origin
                .When(x => x.Connect(Arg.Any<VpnEndpoint>(), Arg.Any<VpnCredentials>(), Arg.Any<VpnConfig>()))
                .Do(x => RaiseStateChangedEvents(
                    new[]
                    {
                        new VpnState(VpnStatus.Connecting),
                        new VpnState(VpnStatus.Connected)
                    }));
        }

        private void SetupUnexpectedDisconnect(VpnError error)
        {
            _origin.StateChanged += (s, e) =>
            {
                if (e.Data.Status == VpnStatus.Connected)
                {
                    _taskQueue.Enqueue(() => RaiseStateChangedEvents(
                        new []
                        {
                            new VpnState(VpnStatus.Disconnecting, error),
                            new VpnState(VpnStatus.Disconnected, error)
                        }));
                }
            };
        }

        private void RaiseStateChangedEvents(IEnumerable<VpnState> states)
        {
            var list = states.ToList();
            var state = list.First();
            _origin.StateChanged += Raise.EventWith(new object(), new EventArgs<VpnState>(state));

            if (list.Count > 1)
            {
                _taskQueue.Enqueue(() => RaiseStateChangedEvents(list.Skip(1)));
            }
        }

        private class TaskQueue : ITaskQueue
        {
            private readonly ITaskQueue _origin = new SerialTaskQueue();
            private volatile Task _lastTask = Task.CompletedTask;

            public Task<T> Enqueue<T>(Func<Task<T>> function)
            {
                var task = _origin.Enqueue(function);
                _lastTask = task;
                return task;
            }

            public async Task WaitForLastTask()
            {
                Task task;
                do
                {
                    task = _lastTask;
                    await task;

                } while (task != _lastTask);
            }
        }

        #endregion
    }
}
