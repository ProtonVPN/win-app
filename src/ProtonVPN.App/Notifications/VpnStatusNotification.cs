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
using System.Threading.Tasks;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Translations;

namespace ProtonVPN.Notifications
{
    public class VpnStateNotification : IVpnStateAware
    {
        private VpnStatus _lastVpnStatus;
        private readonly ISystemNotification _systemNotification;

        public VpnStateNotification(ISystemNotification systemNotification)
        {
            _systemNotification = systemNotification;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            var server = e.State.Server;
            switch (e.State.Status)
            {
                case VpnStatus.Connected:
                    _systemNotification.Show(StringResources.Format("Notifications_VpnState_msg_Connected", server?.Name, Environment.NewLine, server?.ExitIp));
                    break;
                case VpnStatus.Disconnecting when _lastVpnStatus == VpnStatus.Connected:
                case VpnStatus.Disconnected when _lastVpnStatus == VpnStatus.Connected:
                    _systemNotification.Show(StringResources.Get("Notifications_VpnState_msg_Disconnected"));
                    break;
            }

            _lastVpnStatus = e.State.Status;

            return Task.CompletedTask;
        }
    }
}
