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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppUpdateLogs;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.Service.ControllerRetries;
using ProtonVPN.Service.ProcessCommunication;
using ProtonVPN.Service.Update;
using ProtonVPN.Update;
using ProtonVPN.Update.Contracts;
using ProtonVPN.Update.Contracts.Config;

namespace ProtonVPN.Service;

public class UpdateController : IUpdateController
{
    private readonly INotifyingAppUpdate _notifyingAppUpdate;
    private readonly IAppUpdates _appUpdates;
    private readonly IFeedUrlProvider _feedUrlProvider;
    private readonly IClientControllerSender _clientControllerSender;
    private readonly IEntityMapper _entityMapper;
    private readonly ICurrentAppVersionProvider _currentAppVersionProvider;
    private readonly ILogger _logger;
    private readonly IControllerRetryManager _controllerRetryManager;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private AppUpdateStateContract _lastUpdateState;
    private Version _lastInstalledVersion;

    public UpdateController(
        INotifyingAppUpdate notifyingAppUpdate,
        IAppUpdates appUpdates,
        IFeedUrlProvider feedUrlProvider,
        IClientControllerSender clientControllerSender,
        IEntityMapper entityMapper,
        ICurrentAppVersionProvider currentAppVersionProvider,
        ILogger logger,
        IControllerRetryManager controllerRetryManager)
    {
        _notifyingAppUpdate = notifyingAppUpdate;
        _appUpdates = appUpdates;
        _feedUrlProvider = feedUrlProvider;
        _clientControllerSender = clientControllerSender;
        _entityMapper = entityMapper;
        _currentAppVersionProvider = currentAppVersionProvider;
        _logger = logger;
        _controllerRetryManager = controllerRetryManager;

        _notifyingAppUpdate.StateChanged += OnUpdateStateChanged;
    }

    public async Task CheckForUpdate(UpdateSettingsIpcEntity updateSettingsIpcEntity, CancellationToken cancelToken)
    {
        CacheUpdateSettings(updateSettingsIpcEntity);
        _appUpdates.Cleanup();
        _notifyingAppUpdate.StartCheckingForUpdate(updateSettingsIpcEntity.IsEarlyAccess);
    }

    public async Task StartAutoUpdate(StartAutoUpdateIpcEntity startAutoUpdateIpcEntity, CancellationToken cancelToken)
    {
        Ensure.NotNull(startAutoUpdateIpcEntity, nameof(startAutoUpdateIpcEntity));
        _controllerRetryManager.EnforceRetryId(startAutoUpdateIpcEntity);

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