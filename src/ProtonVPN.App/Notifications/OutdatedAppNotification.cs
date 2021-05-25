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

using System.Threading.Tasks;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Login.Views;
using ProtonVPN.Modals;

namespace ProtonVPN.Notifications
{
    internal class OutdatedAppNotification : IVpnStateAware
    {
        private readonly IModals _modals;
        private readonly UserAuth _userAuth;

        private bool _notified;
        private readonly LoginWindow _loginWindow;
        private readonly IScheduler _scheduler;
        private readonly IVpnServiceManager _vpnServiceManager;

        private VpnStatus _vpnStatus;

        public OutdatedAppNotification(
            IModals modals,
            UserAuth userAuth,
            LoginWindow loginWindow,
            IScheduler scheduler,
            IVpnServiceManager vpnServiceManager)
        {
            _modals = modals;
            _userAuth = userAuth;
            _loginWindow = loginWindow;
            _scheduler = scheduler;
            _vpnServiceManager = vpnServiceManager;
        }

        public async void OnAppOutdated(object sender, BaseResponse e)
        {
            if (_notified)
            {
                return;
            }

            _notified = true;
            await _scheduler.Schedule(async () =>
            {
                if (_vpnStatus != VpnStatus.Disconnected)
                {
                    await _vpnServiceManager.Disconnect(VpnError.Unknown);
                }

                await _userAuth.LogoutAsync();
                _loginWindow.Hide();
                _modals.Show<OutdatedAppModalViewModel>();
            });
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            return Task.CompletedTask;
        }
    }
}
