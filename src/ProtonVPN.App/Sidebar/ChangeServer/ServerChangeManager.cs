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
using Caliburn.Micro;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals;

namespace ProtonVPN.Sidebar.ChangeServer
{
    public class ServerChangeManager : IVpnStateAware
    {
        private readonly IVpnManager _vpnManager;
        private readonly IAppSettings _appSettings;
        private readonly IUserStorage _userStorage;
        private readonly IModals _modals;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISchedulerTimer _changeServerTimer;
        private bool _isToChangeServer;

        public ServerChangeManager(
            IVpnManager vpnManager,
            IAppSettings appSettings,
            IUserStorage userStorage,
            IModals modals,
            IEventAggregator eventAggregator,
            IScheduler scheduler)
        {
            _vpnManager = vpnManager;
            _appSettings = appSettings;
            _userStorage = userStorage;
            _modals = modals;
            _eventAggregator = eventAggregator;

            _changeServerTimer = scheduler.Timer();
            _changeServerTimer.IsEnabled = false;
            _changeServerTimer.Interval = TimeSpan.FromSeconds(1);
            _changeServerTimer.Tick += OnChangeServerTimerSecondPassed;
        }

        public async Task ChangeServerAsync()
        {
            if (DateTime.UtcNow >= _appSettings.NextChangeServerTimeUtc)
            {
                _isToChangeServer = true;
                await _vpnManager.ReconnectToFreeRandomServerAsync();
            }
            else
            {
                await _modals.ShowAsync<ChangeServerModalViewModel>();
            }
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (e.State.Status != VpnStatus.Connected)
            {
                return;
            }

            if (_isToChangeServer)
            {
                _isToChangeServer = false;
                if (!_userStorage.GetUser().Paid() && _appSettings.FeatureFreeRescopeEnabled)
                {
                    if (_appSettings.ChangeServerAttempts <= 0)
                    {
                        _appSettings.ChangeServerAttempts = _appSettings.ChangeServerAttemptLimit;
                    }
                    else
                    {
                        _appSettings.ChangeServerAttempts--;
                    }

                    int delayInSeconds = GetDelayInSeconds();
                    _appSettings.NextChangeServerTimeUtc = DateTime.UtcNow.AddSeconds(delayInSeconds);
                }
            }

            if ((DateTime.UtcNow < _appSettings.NextChangeServerTimeUtc && !_changeServerTimer.IsEnabled) || _isToChangeServer)
            {
                _changeServerTimer.Start();
            }
        }

        private int GetDelayInSeconds()
        {
            return _appSettings.ChangeServerAttempts <= 0
                ? _appSettings.ChangeServerLongDelayInSeconds
                : _appSettings.ChangeServerShortDelayInSeconds;
        }

        private void OnChangeServerTimerSecondPassed(object sender, EventArgs e)
        {
            double progress;
            double secondsLeft = _appSettings.NextChangeServerTimeUtc.Subtract(DateTime.UtcNow).TotalSeconds;
            bool isLongDelay = _appSettings.ChangeServerAttempts == 0;
            string timeLeftFormatted = "00:00";

            if (secondsLeft > 0)
            {
                int minutes = (int)secondsLeft / 60;
                int seconds = (int)secondsLeft % 60;

                timeLeftFormatted = $"{minutes:D2}:{seconds:D2}";
                progress = 100 - secondsLeft * 100 / GetDelayInSeconds();
            }
            else
            {
                _changeServerTimer.Stop();
                progress = 100;
            }

            _eventAggregator.PublishOnUIThreadAsync(
                new ChangeServerTimeLeftMessage
                {
                    IsLongDelay = isLongDelay,
                    Progress = progress,
                    TimeLeftFormatted = timeLeftFormatted,
                    TimeLeftInSeconds = secondsLeft,
                });
        }
    }
}