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
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Vpn;
using ProtonVPN.P2PDetection.Blocked;
using ProtonVPN.Translations;

namespace ProtonVPN.P2PDetection
{
    public class P2PDetector : IP2PDetector, IVpnStateAware
    {
        private readonly ILogger _logger;
        private readonly IDialogs _dialogs;
        private readonly IBlockedTraffic _blockedTraffic;
        private readonly ISchedulerTimer _timer;

        private VpnState _vpnState;

        public P2PDetector(ILogger logger,
            IConfiguration appConfig,
            IBlockedTraffic blockedTraffic,
            IScheduler scheduler,
            IDialogs dialogs)
        {
            _logger = logger;
            _blockedTraffic = blockedTraffic;
            _timer = scheduler.Timer();
            _dialogs = dialogs;

            _timer.Interval = appConfig.P2PCheckInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += OnTimerTick;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnState = e.State;

            if (e.State.Status.Equals(VpnStatus.Connected))
            {
                StartTimer();
            }
            else
            {
                StopTimer();
            }

            return Task.CompletedTask;
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            if (_vpnState.Server != null && _vpnState.Status == VpnStatus.Connected)
            {
                await CheckBlockedTraffic();
            }
        }

        private async Task CheckBlockedTraffic()
        {
            if (await _blockedTraffic.Detected())
            {
                _logger.Info<AppLog>("Blocked traffic detected");
                StopTimer();
                ShowBlockedTrafficModal();
            }
        }

        private void ShowBlockedTrafficModal()
        {
            _dialogs.ShowWarning(Translation.Get("Dialogs_P2PBlocked_msg_Blocked"));
        }

        private void StartTimer()
        {
            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
        }

        private void StopTimer()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }
        }
    }
}