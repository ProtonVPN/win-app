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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.User;

namespace ProtonVPN.Core.Auth
{
    public class AuthCertificateUpdater : IAuthCertificateUpdater, ILoggedInAware, ILogoutAware, IVpnPlanAware
    {
        private readonly Common.Configuration.Config _appConfig;
        private readonly IAuthCertificateManager _authCertificateManager;
        private readonly ISchedulerTimer _timer;

        public AuthCertificateUpdater(IScheduler scheduler,
            Common.Configuration.Config appConfig,
            IAuthCertificateManager authCertificateManager)
        {
            _appConfig = appConfig;
            _authCertificateManager = authCertificateManager;

            _timer = scheduler.Timer();
            _timer.Tick += Timer_OnTick;
        }

        private async void Timer_OnTick(object sender, EventArgs eventArgs)
        {
            await _authCertificateManager.RequestNewCertificateAsync();
        }
        
        public void OnUserLoggedIn()
        {
            _timer.Interval = _appConfig.AuthCertificateUpdateInterval.RandomizedWithDeviation(0.2);
            _timer.Start();
        }

        public void OnUserLoggedOut()
        {
            _timer.Stop();
        }
        
        public async Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            await _authCertificateManager.ForceRequestNewCertificateAsync();
        }
    }
}
