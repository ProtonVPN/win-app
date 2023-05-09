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
using System.Threading.Tasks;
using System.Windows.Threading;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Service.Update;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.Update
{
    public class UpdateService : ISettingsAware, IVpnStateAware, ILogoutAware
    {
        private readonly IScheduler _scheduler;
        private readonly IConfiguration _appConfig;
        private readonly IAppSettings _appSettings;
        private readonly IEntityMapper _entityMapper;
        private readonly UpdateServiceCaller _updateServiceCaller;
        private readonly DispatcherTimer _timer;

        private bool _manualCheck;
        private bool _requestedManualCheck;
        private bool _isAutoUpdated;
        private DateTime _lastCheckTime;
        private FeedType _feedType;
        private AppUpdateStatus _status = AppUpdateStatus.None;

        public UpdateService(
            IScheduler scheduler,
            IConfiguration appConfig,
            IAppSettings appSettings,
            IEntityMapper entityMapper,
            UpdateServiceCaller updateServiceCaller,
            IAppController appController)
        {
            _scheduler = scheduler;
            _appConfig = appConfig;
            _appSettings = appSettings;
            _entityMapper = entityMapper;
            _updateServiceCaller = updateServiceCaller;

            _timer = new DispatcherTimer();
            _timer.Tick += TimerTick;
            _timer.Interval = appConfig.UpdateCheckInterval;

            appController.OnUpdateStateChanged += OnUpdateStateChanged;
        }

        public event EventHandler<UpdateStateChangedEventArgs> UpdateStateChanged;

        public void StartCheckingForUpdate()
        {
            StartCheckingForUpdate(true);
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(IAppSettings.EarlyAccess))
            {
                StartCheckingForUpdate(true);
            }
        }

        public void Initialize()
        {
            StartCheckingForUpdate(true);
            _timer.Start();
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            FeedType feedType = e.State.Status == VpnStatus.Connected && e.State.Server.Tier == ServerTiers.Internal
                ? FeedType.Internal
                : FeedType.Public;

            if (_feedType != feedType)
            {
                _feedType = feedType;
                StartCheckingForUpdate(true);
            }
        }

        public void OnUserLoggedOut()
        {
            _status = AppUpdateStatus.None;
            StartCheckingForUpdate(true);
        }

        private UpdateSettingsIpcEntity CreateSettingsIpcEntity()
        {
            return new UpdateSettingsIpcEntity
            {
                FeedType = (FeedTypeIpcEntity)_feedType,
                IsEarlyAccess = _appSettings.EarlyAccess,
            };
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

            _updateServiceCaller.CheckForUpdates(CreateSettingsIpcEntity());

            _lastCheckTime = DateTime.UtcNow;
        }

        private void OnUpdateStateChanged(object sender, UpdateStateIpcEntity e)
        {
            AppUpdateStateContract state = _entityMapper.Map<UpdateStateIpcEntity, AppUpdateStateContract>(e);
            if (state.IsReady && _appSettings.IsToAutoUpdate && state.Status == AppUpdateStatus.Ready)
            {
                _updateServiceCaller.StartAutoUpdate();
            }
            else
            {
                if (state.Status == AppUpdateStatus.AutoUpdated)
                {
                    _isAutoUpdated = true;
                }

                if (_isAutoUpdated && state.IsReady)
                {
                    state.Status = AppUpdateStatus.AutoUpdated;
                }

                OnUpdateStateChanged(state);
            }
        }

        private void OnUpdateStateChanged(AppUpdateStateContract state)
        {
            if (state.Status != _status)
            {
                if (state.Status == AppUpdateStatus.Checking)
                {
                    _manualCheck = _requestedManualCheck;
                    _requestedManualCheck = false;
                }

                _scheduler.Schedule(() =>
                {
                    UpdateStateChanged?.Invoke(this, new UpdateStateChangedEventArgs(state, _manualCheck));
                });
            }

            _status = state.Status;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            StartCheckingForUpdate(false);
        }
    }
}