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
using System.Threading.Tasks;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.UserCertificateLogs;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Users;

namespace ProtonVPN.Core.Auth
{
    public class AuthCertificateUpdater : IAuthCertificateUpdater, ILoggedInAware, ILogoutAware, IVpnPlanAware
    {
        private readonly IConfiguration _appConfig;
        private readonly IAuthCertificateManager _authCertificateManager;
        private readonly ILogger _logger;
        private readonly ISchedulerTimer _timer;

        public AuthCertificateUpdater(IScheduler scheduler,
            IConfiguration appConfig,
            IAuthCertificateManager authCertificateManager,
            ILogger logger)
        {
            _appConfig = appConfig;
            _authCertificateManager = authCertificateManager;
            _logger = logger;

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
            _logger.Info<UserCertificateScheduleRefreshLog>(
                $"User certificate refresh scheduled for every '{_timer.Interval}'.");
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