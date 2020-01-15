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

using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Service;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Resources;
using ProtonVPN.Windows;
using System.Threading.Tasks;
using System.Windows;

namespace ProtonVPN.Core
{
    internal class AppExitHandler : IVpnStateAware
    {
        private readonly IDialogs _dialogs;
        private readonly IService _vpnService;
        private readonly IService _appUpdateService;
        private VpnStatus _vpnStatus;
        private readonly TrayIcon _trayIcon;

        public bool PendingExit { get; private set; }

        public AppExitHandler(IDialogs dialogs, VpnServiceWrapper vpnService, TrayIcon trayIcon, AppUpdateServiceWrapper appUpdateService)
        {
            _trayIcon = trayIcon;
            _vpnService = vpnService;
            _appUpdateService = appUpdateService;
            _dialogs = dialogs;
        }

        public void RequestExit()
        {
            if (_vpnStatus != VpnStatus.Disconnected &&
                _vpnStatus != VpnStatus.Disconnecting)
            {
                var result = _dialogs.ShowQuestion(StringResources.Get("App_msg_ExitConnectedConfirm"));
                if (result == false)
                    return;
            }

            PendingExit = true;
            _vpnService.Stop();
            _appUpdateService.Stop();
            _trayIcon.Hide();

            Application.Current.Shutdown();
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;
            return Task.CompletedTask;
        }
    }
}
