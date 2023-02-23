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
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Login.ViewModels;

namespace ProtonVPN.Core
{
    internal class ExpiredSessionHandler : IVpnStateAware
    {
        private VpnStatus _vpnStatus;
        private readonly IScheduler _scheduler;
        private readonly IVpnServiceManager _vpnServiceManager;
        private readonly LoginViewModel _loginViewModel;
        private readonly IUserAuthenticator _userAuthenticator;

        public ExpiredSessionHandler(
            IScheduler scheduler,
            IVpnServiceManager vpnServiceManager,
            LoginViewModel loginViewModel,
            IUserAuthenticator userAuthenticator)
        {
            _userAuthenticator = userAuthenticator;
            _loginViewModel = loginViewModel;
            _vpnServiceManager = vpnServiceManager;
            _scheduler = scheduler;
        }

        public async void Execute()
        {
            await _scheduler.Schedule(async () =>
            {
                if (_vpnStatus != VpnStatus.Disconnected)
                {
                    await _vpnServiceManager.Disconnect(VpnError.NoneKeepEnabledKillSwitch);
                }

                _loginViewModel.OnSessionExpired();
                await _userAuthenticator.LogoutAsync();
            });
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            return Task.CompletedTask;
        }
    }
}
