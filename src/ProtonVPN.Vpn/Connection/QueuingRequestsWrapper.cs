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

using System;
using System.Collections.Generic;
using ProtonVPN.Common;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.Connection
{
    /// <summary>
    /// Queues <see cref="Connect"/>, <see cref="Disconnect"/>, and <see cref="UpdateServers"/>
    /// requests into sequence with events.
    /// A wrapper around <see cref="ISingleVpnConnection"/>.
    /// </summary>
    /// <remarks>
    /// Other wrappers behind <see cref="QueuingRequestsWrapper"/> will receive <see cref="Connect"/>,
    /// <see cref="Disconnect"/>, and <see cref="UpdateServers"/> requests queued into single queue.
    /// The next request will arrive only after previous one has passed the wrapper sequence behind
    /// <see cref="QueuingRequestsWrapper"/>.
    ///
    /// Requests and events should be processed fast without delays.
    /// </remarks>
    public class QueuingRequestsWrapper : IVpnConnection
    {
        private readonly IVpnConnection _origin;
        private readonly ITaskQueue _taskQueue;

        public QueuingRequestsWrapper(
            ITaskQueue taskQueue,
            IVpnConnection origin)
        {
            _taskQueue = taskQueue;
            _origin = origin;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged
        {
            add => _origin.StateChanged += value;
            remove => _origin.StateChanged -= value;
        }

        public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
        {
            add => _origin.ConnectionDetailsChanged += value;
            remove => _origin.ConnectionDetailsChanged -= value;
        }

        public InOutBytes Total => _origin.Total;

        public void Connect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnCredentials credentials)
        {
            _taskQueue.Enqueue(() => _origin.Connect(servers, config, credentials));
        }

        public void Disconnect(VpnError error = VpnError.None)
        {
            _origin.Disconnect(error);
        }

        public void UpdateAuthCertificate(string certificate)
        {
            _taskQueue.Enqueue(() => _origin.UpdateAuthCertificate(certificate));
        }

        public void SetFeatures(VpnFeatures vpnFeatures)
        {
            _origin.SetFeatures(vpnFeatures);
        }
    }
}