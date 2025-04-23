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

using ProtonVPN.Client.Common.Observers;
using ProtonVPN.Client.Contracts.Services.Lifecycle;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Extensions;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Legacy.OS.Processes;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppUpdateLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.Client.Logic.Updates;

public class UpdatesManager : PollingObserverBase, IUpdatesManager,
    IEventMessageReceiver<UpdateStateIpcEntity>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IConnectionManager _connectionManager;
    private readonly IConfiguration _config;
    private readonly IEntityMapper _entityMapper;
    private readonly ISettings _settings;
    private readonly IUpdateServiceCaller _updateServiceCaller;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IVpnServiceSettingsUpdater _vpnServiceSettingsUpdater;
    private readonly IOsProcesses _osProcesses;
    private readonly IAppExitInvoker _appExitInvoker;

    private bool _requestedManualCheck;
    private DateTime _lastCheckTime;
    private FeedType _feedType;

    private AppUpdateStateContract? _lastUpdateState;

    private bool IsToCheckForUpdate => DateTime.UtcNow - _lastCheckTime >= _config.UpdateCheckInterval;

    protected override TimeSpan PollingInterval => _config.UpdateCheckInterval;

    public bool IsAutoUpdated { get; private set; }

    public bool IsAutoUpdateInProgress { get; private set; }

    public bool IsUpdateAvailable => _lastUpdateState?.IsReady == true && (!_settings.AreAutomaticUpdatesEnabled || IsAutoUpdated);

    public UpdatesManager(
        ILogger logger,
        IIssueReporter issueReporter,
        IConnectionManager connectionManager,
        IConfiguration config,
        IEntityMapper entityMapper,
        ISettings settings,
        IUpdateServiceCaller updateServiceCaller,
        IEventMessageSender eventMessageSender,
        IVpnServiceSettingsUpdater vpnServiceSettingsUpdater,
        IOsProcesses osProcesses,
        IAppExitInvoker appExitInvoker) : base(logger, issueReporter)
    {
        _connectionManager = connectionManager;
        _config = config;
        _entityMapper = entityMapper;
        _settings = settings;
        _updateServiceCaller = updateServiceCaller;
        _eventMessageSender = eventMessageSender;
        _vpnServiceSettingsUpdater = vpnServiceSettingsUpdater;
        _osProcesses = osProcesses;
        _appExitInvoker = appExitInvoker;
    }

    protected override Task OnTriggerAsync()
    {
        CheckForUpdate(false);
        return Task.CompletedTask;
    }

    public void CheckForUpdate(bool isManualCheck)
    {
        _requestedManualCheck |= isManualCheck;

        if (isManualCheck || IsToCheckForUpdate)
        {
            _updateServiceCaller.CheckForUpdateAsync(new UpdateSettingsIpcEntity
            {
                FeedType = (FeedTypeIpcEntity)_feedType,
                IsEarlyAccess = _settings.IsBetaAccessEnabled,
            });

            _lastCheckTime = DateTime.UtcNow;
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsBetaAccessEnabled))
        {
            SendClientUpdateStateChangeMessage(new ClientUpdateStateChangedMessage());
            CheckForUpdate(true);
        }
    }

    private void SendClientUpdateStateChangeMessage(ClientUpdateStateChangedMessage message)
    {
        _eventMessageSender.Send(message);
    }

    public void Initialize()
    {
        StartTimerAndTriggerOnStart();
    }

    public void Receive(UpdateStateIpcEntity message)
    {
        AppUpdateStateContract state = _entityMapper.Map<UpdateStateIpcEntity, AppUpdateStateContract>(message);
        if (state.IsReady && _settings.AreAutomaticUpdatesEnabled && state.Status == AppUpdateStatus.Ready)
        {
            IsAutoUpdateInProgress = true;
            SendClientUpdateStateChangeMessage(new ClientUpdateStateChangedMessage());

            _updateServiceCaller.StartAutoUpdateAsync();
        }
        else
        {
            if (state.Status == AppUpdateStatus.AutoUpdated)
            {
                IsAutoUpdated = true;
                IsAutoUpdateInProgress = false;
            }

            if (IsAutoUpdated && state.IsReady)
            {
                state.Status = AppUpdateStatus.AutoUpdated;
            }

            OnUpdateStateChanged(state);
        }
    }

    private void OnUpdateStateChanged(AppUpdateStateContract state)
    {
        if (state.Status != _lastUpdateState?.Status || state.IsReady != _lastUpdateState?.IsReady || _requestedManualCheck)
        {
            if (state.Status == AppUpdateStatus.Checking)
            {
                _requestedManualCheck = false;
            }

            SendClientUpdateStateChangeMessage(new ClientUpdateStateChangedMessage
            {
                State = state
            });

            _lastUpdateState = state;
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ConnectionDetails? connectionDetails = _connectionManager.CurrentConnectionDetails;
        FeedType feedType = message.ConnectionStatus == ConnectionStatus.Connected &&
                            connectionDetails?.ServerTier == ServerTiers.Internal
            ? FeedType.Internal
            : FeedType.Public;

        if (_feedType != feedType)
        {
            _feedType = feedType;
            CheckForUpdate(true);
        }
    }

    public async Task UpdateAsync()
    {
        if (_lastUpdateState == null)
        {
            return;
        }

        if (_lastUpdateState.Status == AppUpdateStatus.AutoUpdated)
        {
            Logger.Info<AppUpdateLog>("Restarting app after auto update due to manual request.");
            await _appExitInvoker.RestartAsync();
        }
        else if (_lastUpdateState.IsReady)
        {
            await UpdateManuallyAsync();
        }
    }

    private async Task UpdateManuallyAsync()
    {
        if (_lastUpdateState == null)
        {
            return;
        }

        LogUpdateStartingMessage();

        if (_settings.IsAdvancedKillSwitchActive())
        {
            await _vpnServiceSettingsUpdater.SendAsync(KillSwitchModeIpcEntity.Off);
        }

        try
        {
            _osProcesses.ElevatedProcess(_lastUpdateState.FilePath, _lastUpdateState.FileArguments).Start();
            await _appExitInvoker.ForceExitAsync();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Privileges were not granted
            if (_settings.IsAdvancedKillSwitchActive())
            {
                await _vpnServiceSettingsUpdater.SendAsync(KillSwitchModeIpcEntity.Hard);
            }
        }
    }

    private void LogUpdateStartingMessage()
    {
        string fileName = GetUpdateFileName();
        string message = $"Closing the app and starting installer '{fileName}'. " +
                         $"Current app version: {_config.ClientVersion}, OS: {Environment.OSVersion.VersionString}";

        Logger.Info<AppUpdateStartLog>(message);
    }

    private string GetUpdateFileName()
    {
        string fileName;
        string filePath = _lastUpdateState?.FilePath ?? string.Empty;
        try
        {
            fileName = Path.GetFileNameWithoutExtension(filePath);
        }
        catch (Exception e)
        {
            Logger.Error<AppUpdateLog>($"Failed to parse file name of path '{filePath}'.", e);
            fileName = filePath;
        }

        return fileName;
    }
}