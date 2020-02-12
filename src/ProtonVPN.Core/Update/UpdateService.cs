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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.UpdateServiceContract;

namespace ProtonVPN.Core.Update
{
    public class UpdateService : ISettingsAware, ILoggedInAware, ILogoutAware
    {
        private readonly IAppSettings _appSettings;
        private readonly Config _appConfig;
        private readonly ServiceClient _serviceClient;
        private readonly TimeSpan _updateInterval;
        private DateTime _lastCheckTime;

        private readonly DispatcherTimer _timer;

        private bool _firstCheck;
        private bool _manualCheck;
        private bool _requestedManualCheck;
        private UpdateStatus _status = UpdateStatus.None;

        public UpdateService(
            Config appConfig,
            IAppSettings appSettings,
            ServiceClient serviceClient)
        {
            _updateInterval = appConfig.UpdateCheckInterval;
            _appConfig = appConfig;
            _serviceClient = serviceClient;
            _appSettings = appSettings;

            _timer = new DispatcherTimer();
            _timer.Tick += TimerTick;

            _serviceClient.UpdateStateChanged += OnUpdateStateChanged;
        }

        public event EventHandler<UpdateStateChangedEventArgs> UpdateStateChanged;

        public void StartCheckingForUpdate() => StartCheckingForUpdate(true);

        public Task Update(bool auto) => _serviceClient.StartUpdating(auto);

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(IAppSettings.EarlyAccess)))
            {
                StartCheckingForUpdate(true);
            }
        }

        public void OnUserLoggedIn()
        {
            _firstCheck = true;
            _timer.Interval = _appConfig.UpdateFirstCheckDelay;
            _timer.Start();
        }

        public void OnUserLoggedOut()
        {
            _timer.Stop();
        }

        private void StartCheckingForUpdate(bool manualCheck)
        {
            _requestedManualCheck |= manualCheck;

            if (!manualCheck)
            {
                if (DateTime.UtcNow - _lastCheckTime <= _appConfig.UpdateCheckInterval)
                {
                    return;
                }
            }

            _serviceClient.CheckForUpdate(_appSettings.EarlyAccess);
            _lastCheckTime = DateTime.UtcNow;
        }

        private void OnUpdateStateChanged(object sender, UpdateStateContract e)
        {
            var state = Map(e);
            OnUpdateStateChanged(state, _manualCheck);
            HandleManualCheck(state.Status);
            HandleUpdating(state.Status);
        }

        private UpdateState Map(UpdateStateContract e)
        {
            var releaseHistory = e.ReleaseHistory
                .Select(release => new Release(
                    release.Version,
                    release.EarlyAccess,
                    release.New,
                    release.ChangeLog.ToList()))
                .ToList();

            return new UpdateState(releaseHistory, e.Available, e.Ready, (UpdateStatus)e.Status, e.FilePath, e.FileArguments);
        }

        private void HandleManualCheck(UpdateStatus status)
        {
            if (status != _status && status == UpdateStatus.Checking)
            {
                _manualCheck = _requestedManualCheck;
                _requestedManualCheck = false;
            }

            _status = status;
        }

        private void HandleUpdating(UpdateStatus status)
        {
            if (status != UpdateStatus.Updating)
                return;

            Application.Current.Shutdown();
        }

        private void OnUpdateStateChanged(UpdateState state, bool manualCheck)
        {
            UpdateStateChanged?.Invoke(this, new UpdateStateChangedEventArgs(state, manualCheck));
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (_firstCheck)
            {
                _firstCheck = false;
                _timer.Interval = _updateInterval;
            }

            StartCheckingForUpdate(false);
        }
    }
}
