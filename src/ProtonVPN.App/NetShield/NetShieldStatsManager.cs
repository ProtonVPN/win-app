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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Core.Windows;

namespace ProtonVPN.NetShield
{
    public class NetShieldStatsManager : IVpnStateAware, ISettingsAware, IHandle<WindowStateMessage>
    {
        private const int TIMER_INTERVAL_IN_SECONDS = 20;
        private const int MINIMUM_REQUEST_TIMEOUT_IN_SECONDS = 20;
        
        private readonly ILogger _logger;
        private readonly VpnServiceCaller _vpnServiceCaller;
        private readonly IAppSettings _appSettings;
        private readonly ISchedulerTimer _timer;
        private readonly TimeSpan _requestTimeout;
        private readonly object _lock = new();

        private bool _isConnected;
        private bool _isMainAppWindowFocused = true;
        private DateTime _nextRequestDateUtc = DateTime.MinValue;

        public NetShieldStatsManager(ILogger logger,
            VpnServiceCaller vpnServiceCaller,
            IAppSettings appSettings,
            IScheduler scheduler,
            IEventAggregator eventAggregator,
            IConfiguration configuration)
        {
            eventAggregator.Subscribe(this);

            _logger = logger;
            _vpnServiceCaller = vpnServiceCaller;
            _appSettings = appSettings;

            _timer = scheduler.Timer();
            _timer.Interval = TimeSpan.FromSeconds(TIMER_INTERVAL_IN_SECONDS);
            _timer.Tick += OnTimerTick;

            TimeSpan requestInterval = configuration.NetShieldStatisticRequestInterval.RandomizedWithDeviation(0.2);
            TimeSpan minimumRequestTimeout = TimeSpan.FromSeconds(MINIMUM_REQUEST_TIMEOUT_IN_SECONDS);
            _requestTimeout = requestInterval > minimumRequestTimeout ? requestInterval : minimumRequestTimeout;
            _logger.Info<AppLog>($"NetShield Stats - Request timeout set to {_requestTimeout}.");
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            RequestIfAllowed();
        }

        private void RequestIfAllowed()
        {
            bool isToRequest;
            lock (_lock)
            {
                isToRequest = AreNetShieldMode2AndFeatureNetShieldStatsEnabled() && _isConnected && _isMainAppWindowFocused &&
                    _nextRequestDateUtc < DateTime.UtcNow;
                if (isToRequest)
                {
                    SetNextRequestDateUtc();
                }
            }
            if (isToRequest)
            {
                _logger.Debug<AppLog>("NetShield Stats - Request made");
                _vpnServiceCaller.RequestNetShieldStats();
            }
        }

        private void SetNextRequestDateUtc()
        {
            _nextRequestDateUtc = DateTime.UtcNow + _requestTimeout;
        }

        private bool AreNetShieldMode2AndFeatureNetShieldStatsEnabled()
        {
            return IsNetShieldMode2Enabled() && _appSettings.FeatureNetShieldStatsEnabled;
        }

        private bool IsNetShieldMode2Enabled()
        {
            return _appSettings.IsNetShieldEnabled() && _appSettings.NetShieldMode == 2;
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            lock (_lock)
            {
                if (!_isConnected && e.State.Status == VpnStatus.Connected)
                {
                    _logger.Debug<AppLog>("NetShield Stats - Connection established, resetting next request date");
                    SetNextRequestDateUtc();
                }
                _isConnected = e.State.Status == VpnStatus.Connected;
                SwitchTimerIfNeeded();
            }
        }

        private void SwitchTimerIfNeeded()
        {
            bool isNetShieldMode2Enabled = AreNetShieldMode2AndFeatureNetShieldStatsEnabled();
            if (isNetShieldMode2Enabled && _isConnected && _isMainAppWindowFocused && !_timer.IsEnabled)
            {
                _timer.Start();
            }
            else if ((!isNetShieldMode2Enabled || !_isConnected || !_isMainAppWindowFocused) && _timer.IsEnabled)
            {
                _timer.Stop();
            }
        }

        public async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e?.PropertyName is not null &&
                (e.PropertyName == nameof(IAppSettings.NetShieldEnabled) ||
                 e.PropertyName == nameof(IAppSettings.NetShieldMode) ||
                 e.PropertyName == nameof(IAppSettings.FeatureNetShieldEnabled) ||
                 e.PropertyName == nameof(IAppSettings.FeatureNetShieldStatsEnabled)))
            {
                lock (_lock)
                {
                    SwitchTimerIfNeeded();
                }
            }
        }

        public async Task HandleAsync(WindowStateMessage message, CancellationToken cancellationToken)
        {
            bool isToRequestImmediatly;
            lock (_lock)
            {
                isToRequestImmediatly = !_isMainAppWindowFocused && message.IsActive;
                _isMainAppWindowFocused = message.IsActive;
                SwitchTimerIfNeeded();
            }
            if (isToRequestImmediatly)
            {
                RequestIfAllowed();
            }
        }
    }
}