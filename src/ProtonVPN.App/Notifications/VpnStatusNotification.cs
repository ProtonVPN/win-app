/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Translations;

namespace ProtonVPN.Notifications
{
    public class VpnStateNotification : IVpnStateAware, IConnectionDetailsAware
    {
        private VpnStatus _lastVpnStatus;
        private Server _lastServer;
        private readonly INotificationSender _notificationSender;
        private bool _isToNotifyOfConnectedState;

        public VpnStateNotification(INotificationSender notificationSender)
        {
            _notificationSender = notificationSender;
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            Server server = e.State.Server;
            switch (e.State.Status)
            {
                case VpnStatus.Connected when server is not null && !server.IsEmpty():
                    _lastServer = server;
                    _isToNotifyOfConnectedState = true;
                    break;
                case VpnStatus.Disconnecting when _lastVpnStatus == VpnStatus.Connected:
                case VpnStatus.Disconnected when _lastVpnStatus == VpnStatus.Connected:
                    _notificationSender.Send(Translation.Get("Notifications_VpnState_msg_Disconnected"));
                    break;
            }

            _lastVpnStatus = e.State.Status;
        }

        public async Task OnConnectionDetailsChanged(ConnectionDetails connectionDetails)
        {
            if (_lastServer != null && _isToNotifyOfConnectedState)
            {
                _isToNotifyOfConnectedState = false;
                _notificationSender.Send(Translation.Format("Notifications_VpnState_msg_Connected",
                    _lastServer.Name, Environment.NewLine,
                    connectionDetails.ServerIpAddress.IsNullOrEmpty()
                        ? _lastServer.ExitIp
                        : connectionDetails.ServerIpAddress));
            }
        }
    }
}