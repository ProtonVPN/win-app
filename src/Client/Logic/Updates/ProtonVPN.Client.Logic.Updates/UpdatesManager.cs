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

using Microsoft.UI.Xaml;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.Client.Logic.Updates;

public class UpdatesManager : IUpdatesManager,
    IEventMessageReceiver<UpdateStateIpcEntity>,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IConnectionManager _connectionManager;
    private readonly IConfiguration _config;
    private readonly DispatcherTimer _refreshTimer;
    private readonly IEntityMapper _entityMapper;
    private readonly ISettings _settings;
    private readonly IUpdateServiceCaller _updateServiceCaller;
    private readonly IEventMessageSender _eventMessageSender;

    private bool _manualCheck;
    private bool _requestedManualCheck;
    private bool _isAutoUpdated;
    private DateTime _lastCheckTime;
    private FeedType _feedType;

    private AppUpdateStatus _status = AppUpdateStatus.None;
    private DateTime _lastNotifiedAt = DateTime.MinValue;

    private bool IsToCheckForUpdate => DateTime.UtcNow - _lastCheckTime >= _config.UpdateCheckInterval;
    private bool IsToRemindAboutUpdate => _manualCheck || DateTime.UtcNow - _lastNotifiedAt >= _config.UpdateRemindInterval;

    public UpdatesManager(
        IConnectionManager connectionManager,
        IConfiguration config,
        IEntityMapper entityMapper,
        ISettings settings,
        IUpdateServiceCaller updateServiceCaller,
        IEventMessageSender eventMessageSender)
    {
        _connectionManager = connectionManager;
        _config = config;
        _entityMapper = entityMapper;
        _settings = settings;
        _updateServiceCaller = updateServiceCaller;
        _eventMessageSender = eventMessageSender;

        _refreshTimer = new() { Interval = config.UpdateCheckInterval };
        _refreshTimer.Tick += OnRefreshTimerTick;
    }

    private void OnRefreshTimerTick(object? sender, object e)
    {
        StartCheckingForUpdate(false);
    }

    public void StartCheckingForUpdate(bool isManualCheck)
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
            SendClientUpdateStateChangeMessage(new ClientUpdateStateChangedMessage
            {
                IsUpdateAvailable = false,
            });
            StartCheckingForUpdate(true);
        }
    }

    public void Initialize()
    {
        StartCheckingForUpdate(true);
        _refreshTimer.Start();
    }

    public void Receive(UpdateStateIpcEntity message)
    {
        AppUpdateStateContract state = _entityMapper.Map<UpdateStateIpcEntity, AppUpdateStateContract>(message);
        if (state.IsReady && _settings.AreAutomaticUpdatesEnabled && state.Status == AppUpdateStatus.Ready)
        {
            _updateServiceCaller.StartAutoUpdateAsync();
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

            if (IsToSendAppUpdateStateChangeMessage(state))
            {
                _lastNotifiedAt = DateTime.UtcNow;
                SendClientUpdateStateChangeMessage(new ClientUpdateStateChangedMessage
                {
                    State = state,
                    IsUpdateAvailable = true,
                });
            }

            _status = state.Status;
        }
    }

    private bool IsToSendAppUpdateStateChangeMessage(AppUpdateStateContract state)
    {
        return IsAppReadyForUpdateOrUpdated(state.Status) && IsToRemindAboutUpdate;
    }

    private bool IsAppReadyForUpdateOrUpdated(AppUpdateStatus status)
    {
        return status is AppUpdateStatus.Ready or AppUpdateStatus.AutoUpdated or AppUpdateStatus.AutoUpdateFailed;
    }

    private void SendClientUpdateStateChangeMessage(ClientUpdateStateChangedMessage message)
    {
        _eventMessageSender.Send(message);
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ConnectionDetails? connectionDetails = _connectionManager.CurrentConnectionDetails;
        FeedType feedType = message.ConnectionStatus == ConnectionStatus.Connected &&
                            connectionDetails?.ServerTier == ServerTiers.Internal
            ? FeedType.Internal
            : FeedType.Public;

        if (_feedType != feedType)
        {
            _feedType = feedType;
            StartCheckingForUpdate(true);
        }
    }
}