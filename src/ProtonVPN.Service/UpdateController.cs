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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppUpdateLogs;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.Service.ProcessCommunication;
using ProtonVPN.Service.Update;
using ProtonVPN.Update;
using ProtonVPN.Update.Contracts;
using ProtonVPN.Update.Contracts.Config;

namespace ProtonVPN.Service
{
    public class UpdateController : IUpdateController
    {
        private readonly INotifyingAppUpdate _notifyingAppUpdate;
        private readonly IAppUpdates _appUpdates;
        private readonly IFeedUrlProvider _feedUrlProvider;
        private readonly IClientControllerSender _clientControllerSender;
        private readonly IEntityMapper _entityMapper;
        private readonly ICurrentAppVersionProvider _currentAppVersionProvider;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private AppUpdateStateContract _lastUpdateState;

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
        }

        public async Task CheckForUpdate(UpdateSettingsIpcEntity updateSettingsIpcEntity,
            CancellationToken cancelToken)
        {
            CacheUpdateSettings(updateSettingsIpcEntity);
            _appUpdates.Cleanup();
            _notifyingAppUpdate.StartCheckingForUpdate(updateSettingsIpcEntity.IsEarlyAccess);
        }

        public async Task StartAutoUpdate(CancellationToken cancelToken)
        {
            if (_lastUpdateState.IsReady)
            {
                if (_lastUpdateState.Version > _currentAppVersionProvider.GetVersion())
                {
                    await HandleAutoUpdate(_lastUpdateState);
                }
                else if (_lastUpdateState.Version == _currentAppVersionProvider.GetVersion())
                {
                    _lastUpdateState.Status = AppUpdateStatus.AutoUpdated;
                    await SendUpdateStateAsync(_lastUpdateState);
                }
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
                int exitCode = RunInstaller(appUpdateStateContract);
                if (exitCode == 0)
                {
                    _logger.Info<AppUpdateLog>($"The app was updated to version {appUpdateStateContract.Version}.");
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