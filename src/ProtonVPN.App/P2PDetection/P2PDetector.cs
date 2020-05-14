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

using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals;
using ProtonVPN.P2PDetection.Blocked;
using ProtonVPN.P2PDetection.Forwarded;
using ProtonVPN.Resources;
using System;
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.P2PDetection
{
    /// <summary>
    /// While connected to VPN periodically checks if P2P activity is detected on VPN server.
    /// </summary>
    /// <remarks>
    /// If P2P traffic is detected, traffic is blocked on free VPN servers or might be blocked
    /// or forwarded on not free VPN server. Displays modal dialog with corresponding message
    /// if P2P activity is detected.
    /// </remarks>
    public class P2PDetector : IVpnStateAware
    {
        private readonly ILogger _logger;
        private readonly IModals _modals;
        private readonly IDialogs _dialogs;
        private readonly IBlockedTraffic _blockedTraffic;
        private readonly IForwardedTraffic _forwardedTraffic;
        private readonly ISchedulerTimer _timer;

        private VpnState _vpnState;
        private bool _trafficForwarded;

        public P2PDetector(
            ILogger logger,
            Common.Configuration.Config appConfig,
            IBlockedTraffic blockedTraffic,
            IForwardedTraffic forwardedTraffic,
            IScheduler scheduler,
            IModals modals,
            IDialogs dialogs) :
            this(logger, blockedTraffic, forwardedTraffic, scheduler.Timer(), modals, dialogs, appConfig.P2PCheckInterval.RandomizedWithDeviation(0.2))
        { }

        public event EventHandler<string> TrafficForwarded;

        private P2PDetector(
            ILogger logger,
            IBlockedTraffic blockedTraffic,
            IForwardedTraffic forwardedTraffic,
            ISchedulerTimer timer,
            IModals modals,
            IDialogs dialogs,
            TimeSpan checkInterval)
        {
            _logger = logger;
            _blockedTraffic = blockedTraffic;
            _forwardedTraffic = forwardedTraffic;
            _timer = timer;
            _modals = modals;
            _dialogs = dialogs;

            _timer.Interval = checkInterval;
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
                _trafficForwarded = false;
            }

            return Task.CompletedTask;
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            if (_vpnState.Server == null || !_vpnState.Status.Equals(VpnStatus.Connected))
            {
                return;
            }

            if (_vpnState.Server.IsPhysicalFree())
            {
                await CheckBlockedTraffic();
            }
            else
            {
                await CheckForwardedOrBlockedTraffic();
            }
        }

        private async Task CheckBlockedTraffic()
        {
            if (await _blockedTraffic.Detected())
            {
                _logger.Info("Blocked traffic detected");
                StopTimer();
                ShowBlockedTrafficModal();
            }
        }

        private async Task CheckForwardedOrBlockedTraffic()
        {
            var value = await _forwardedTraffic.Value();
            if (!value.Result)
            {
                return;
            }

            if (_trafficForwarded != value.Forwarded)
            {
                _trafficForwarded = value.Forwarded;
                TrafficForwarded?.Invoke(this, value.Ip);

                if (_trafficForwarded)
                {
                    _logger.Info("Forwarded traffic detected");
                    ShowForwardedTrafficModal();
                }
                else
                {
                    _logger.Info("Not forwarded traffic detected");
                }
            }

            if (!value.Forwarded)
            {
                await CheckBlockedTraffic();
            }
        }

        private void ShowBlockedTrafficModal()
        {
            _dialogs.ShowWarning(StringResources.Get("Dialogs_P2PBlocked_msg_Blocked"));
        }

        private void ShowForwardedTrafficModal()
        {
            _modals.Show<P2PForwardModalViewModel>();
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
