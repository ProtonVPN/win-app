/*
 * Copyright (c) 2024 Proton AG
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppUpdateLogs;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.Service.ProcessCommunication;
using ProtonVPN.Service.Update;
using ProtonVPN.Update;
using ProtonVPN.Update.Contracts;
using ProtonVPN.Update.Contracts.Config;
using Timer = System.Timers.Timer;

namespace ProtonVPN.Service
{
    public class UpdateController : IUpdateController
    {
        private readonly TimeSpan _retryIdCleanupTimerInterval = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _retryIdExpirationInterval = TimeSpan.FromHours(1);

        private readonly INotifyingAppUpdate _notifyingAppUpdate;
        private readonly IAppUpdates _appUpdates;
        private readonly IFeedUrlProvider _feedUrlProvider;
        private readonly IClientControllerSender _clientControllerSender;
        private readonly IEntityMapper _entityMapper;
        private readonly ICurrentAppVersionProvider _currentAppVersionProvider;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly Timer _timer;
        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<Guid, DateTimeOffset> _receivedRetryIds = [];

        private AppUpdateStateContract _lastUpdateState;
        private Version _lastInstalledVersion;

        public UpdateController(
            INotifyingAppUpdate notifyingAppUpdate,
            IAppUpdates appUpdates,
            IFeedUrlProvider feedUrlProvider,
            IClientControllerSender clientControllerSender,
            IEntityMapper entityMapper,
            ICurrentAppVersionProvider currentAppVersionProvider,
            ILogger logger)
        {
            _notifyingAppUpdate = notifyingAppUpdate;
            _appUpdates = appUpdates;
            _feedUrlProvider = feedUrlProvider;
            _clientControllerSender = clientControllerSender;
            _entityMapper = entityMapper;
            _currentAppVersionProvider = currentAppVersionProvider;
            _logger = logger;

            _notifyingAppUpdate.StateChanged += OnUpdateStateChanged;

            _timer = new(_retryIdCleanupTimerInterval);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void OnTimedEvent(object sender, EventArgs e)
        {
            lock (_lock)
            {
                List<KeyValuePair<Guid, DateTimeOffset>> list = _receivedRetryIds.ToList();
                foreach (KeyValuePair<Guid, DateTimeOffset> pair in list)
                {
                    if (pair.Value <= DateTimeOffset.UtcNow)
                    {
                        _receivedRetryIds.TryRemove(pair.Key, out _);
                    }
                }
            }
        }

        public async Task CheckForUpdate(UpdateSettingsIpcEntity updateSettingsIpcEntity)
        {
            CacheUpdateSettings(updateSettingsIpcEntity);
            _appUpdates.Cleanup();
            _notifyingAppUpdate.StartCheckingForUpdate(updateSettingsIpcEntity.IsEarlyAccess);
        }

        public async Task StartAutoUpdate(StartAutoUpdateIpcEntity startAutoUpdateIpcEntity)
        {
            EnforceRetryId(startAutoUpdateIpcEntity.RetryId);

            if (_lastUpdateState.IsReady)
            {
                Version lastRegistryVersion = _currentAppVersionProvider.GetVersion();

                if (_lastUpdateState.Version > lastRegistryVersion)
                {
                    await HandleAutoUpdate(_lastUpdateState);
                }
                else if (_lastUpdateState.Version == lastRegistryVersion)
                {
                    _lastUpdateState.Status = AppUpdateStatus.AutoUpdated;
                    await SendUpdateStateAsync(_lastUpdateState);
                }
            }
        }

        private void EnforceRetryId(Guid retryId, [CallerMemberName] string sourceMemberName = "")
        {
            lock (_lock)
            {
                if (_receivedRetryIds.ContainsKey(retryId))
                {
                    string message = $"{sourceMemberName} request dropped because the retry ID is repeated.";
                    _logger.Info<ConnectLog>(message);
                    throw new ArgumentException(message);
                }
                DateTimeOffset expirationDate = DateTimeOffset.UtcNow + _retryIdExpirationInterval;
                _receivedRetryIds.AddOrUpdate(retryId, _ => expirationDate, (_, _) => expirationDate);
            }
        }

        private void UpdateFeedType(FeedType feedType)
        {
            _feedUrlProvider.SetFeedType(feedType);
        }

        private async void OnUpdateStateChanged(object sender, AppUpdateStateContract e)
        {
            await SendUpdateStateAsync(e);
        }

        private async Task HandleAutoUpdate(AppUpdateStateContract appUpdateStateContract)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (_lastInstalledVersion is not null && _lastInstalledVersion >= appUpdateStateContract.Version)
                {
                    _logger.Info<AppUpdateLog>($"Ignoring request to update the app to version " +
                        $"{appUpdateStateContract.Version} because the last successfully installed " +
                        $"version by this running service was equal or higher ({_lastInstalledVersion}).");
                    return;
                }

                int exitCode = RunInstaller(appUpdateStateContract);
                if (exitCode == 0)
                {
                    _logger.Info<AppUpdateLog>($"The app was updated to version {appUpdateStateContract.Version}.");
                    CheckRegistryVersionVersusInstalledVersion(appUpdateStateContract);
                    _lastInstalledVersion = appUpdateStateContract.Version;
                    appUpdateStateContract.Status = AppUpdateStatus.AutoUpdated;
                }
                else
                {
                    _logger.Error<AppUpdateLog>(
                        $"Failed to install the update using file {appUpdateStateContract.FilePath}. " +
                        $"Process exited with code {exitCode}. Informing the user to update manually.");
                    appUpdateStateContract.Status = AppUpdateStatus.AutoUpdateFailed;
                }

                await SendUpdateStateAsync(appUpdateStateContract);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void CheckRegistryVersionVersusInstalledVersion(AppUpdateStateContract appUpdateStateContract)
        {
            Version lastInstalledVersion = _currentAppVersionProvider.GetVersion();
            if (appUpdateStateContract.Version != lastInstalledVersion)
            {
                _logger.Warn<AppUpdateLog>($"There is a mismatch between the version " +
                    $"just installed ({appUpdateStateContract.Version}) and the " +
                    $"version provided by the registry {lastInstalledVersion}.");
            }
        }

        private int RunInstaller(AppUpdateStateContract appUpdateStateContract)
        {
            try
            {
                Process process = new();
                process.StartInfo.FileName = appUpdateStateContract.FilePath;
                process.StartInfo.Arguments = "/VERYSILENT /SUPPRESSMSGBOXES";
                process.Start();
                process.WaitForExit();

                return process.ExitCode;
            }
            catch (Exception e)
            {
                _logger.Error<AppUpdateLog>($"Failed to start installer on path {appUpdateStateContract.FilePath}.", e);
                return -1;
            }
        }

        private async Task SendUpdateStateAsync(AppUpdateStateContract e)
        {
            _lastUpdateState = e;
            await _clientControllerSender.SendUpdateStateAsync(_entityMapper.Map<AppUpdateStateContract, UpdateStateIpcEntity>(e));
        }

        private void CacheUpdateSettings(UpdateSettingsIpcEntity updateSettingsIpcEntity)
        {
            UpdateFeedType((FeedType)updateSettingsIpcEntity.FeedType);
        }
    }
}