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

using System.Threading.Tasks;
using ProtonVPN.Common.OS.Registry;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Translations;

namespace ProtonVPN.Notifications
{
    internal class SystemProxyNotification : IVpnStateAware
    {
        private bool _modalShown;
        private readonly ISystemProxy _proxy;
        private readonly IDialogs _dialogs;

        public SystemProxyNotification(ISystemProxy proxy, IDialogs dialogs)
        {
            _proxy = proxy;
            _dialogs = dialogs;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (e.State.Status == VpnStatus.Connected && _proxy.Enabled() && !_modalShown)
            {
                _modalShown = true;
                _dialogs.ShowWarning(Translation.Get("Dialogs_Proxy_msg_ProxyDetected"));
            }

            if (e.State.Status == VpnStatus.Disconnected)
            {
                _modalShown = false;
            }

            return Task.CompletedTask;
        }
    }
}
