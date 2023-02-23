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

using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectionLogs;
using ProtonVPN.Vpn.SynchronizationEvent;

namespace ProtonVPN.Vpn.OpenVpn
{
    /// <summary>
    /// Signals OpenVPN process to exit using synchronization event.
    /// </summary>
    internal class OpenVpnExitEvent
    {
        private readonly ILogger _logger;
        private readonly ISynchronizationEvents _synchronizationEvents;
        private readonly string _exitEventName;

        public OpenVpnExitEvent(ILogger logger, ISynchronizationEvents synchronizationEvents, string exitEventName)
        {
            _logger = logger;
            _synchronizationEvents = synchronizationEvents;
            _exitEventName = exitEventName;
        }

        public void Signal()
        {
            _logger.Info<ConnectionLog>($"OpenVPN <- Signaling {_exitEventName}");
            using (ISynchronizationEvent exitEvent = _synchronizationEvents.SynchronizationEvent(_exitEventName))
            {
                exitEvent.Set();
                exitEvent.Reset();
            }
        }
    }
}
