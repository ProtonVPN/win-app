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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Notifications.Contracts;
using ProtonVPN.Client.Services.Upselling;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.Handlers;

public class VpnPlanChangedHandler : IHandler,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly ILogger _logger;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly ILocalizationProvider _localizer;
    private readonly IConnectionManager _connectionManager;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly ISettingsViewNavigator _settingsViewNavigator;
    private readonly IMainWindowOverlayActivator _overlayActivator;
    private readonly ISubscriptionExpiredNotificationSender _subscriptionExpiredNotificationSender;
    private readonly IAccountUpgradeUrlLauncher _accountUpgradeUrlLauncher;

    private bool _notifyOnNextConnection;

    public VpnPlanChangedHandler(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        ILocalizationProvider localizer,
        IConnectionManager connectionManager,
        IUserAuthenticator userAuthenticator,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator overlayActivator,
        ISubscriptionExpiredNotificationSender subscriptionExpiredNotificationSender,
        IAccountUpgradeUrlLauncher accountUpgradeUrlLauncher)
    {
        _logger = logger;
        _uiThreadDispatcher = uiThreadDispatcher;
        _localizer = localizer;
        _connectionManager = connectionManager;
        _userAuthenticator = userAuthenticator;
        _mainViewNavigator = mainViewNavigator;
        _settingsViewNavigator = settingsViewNavigator;
        _overlayActivator = overlayActivator;
        _subscriptionExpiredNotificationSender = subscriptionExpiredNotificationSender;
        _accountUpgradeUrlLauncher = accountUpgradeUrlLauncher;
    }

    public async void Receive(VpnPlanChangedMessage message)
    {
        if (!_userAuthenticator.IsLoggedIn)
        {
            return;
        }

        if (message.HasMaxTierChanged())
        {
            _logger.Info<AppLog>("Navigating to Home page due to max tier change.");
            await _uiThreadDispatcher.TryEnqueueAsync(ResetNavigationAsync);
        }

        if (message.IsDowngrade())
        {
            await HandlePlanDowngradeAsync();
        }
    }

    public async void Receive(ConnectionStatusChangedMessage message)
    {
        if (_notifyOnNextConnection && message.ConnectionStatus == ConnectionStatus.Connected)
        {
            _notifyOnNextConnection = false;
            await NotifyUserOfSubscriptionExpirationAsync();
        }
    }

    private async Task HandlePlanDowngradeAsync()
    {
        if (_connectionManager.IsDisconnected)
        {
            await NotifyUserOfSubscriptionExpirationAsync();
        }
        else
        {
            _notifyOnNextConnection = true;
        }
    }

    private async Task NotifyUserOfSubscriptionExpirationAsync()
    {
        _subscriptionExpiredNotificationSender.Send();
        await _uiThreadDispatcher.TryEnqueueAsync(ShowSubscriptionExpiredDialogAsync);
    }

    private async Task ShowSubscriptionExpiredDialogAsync()
    {
        ContentDialogResult result = await _overlayActivator.ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = _localizer.Get("Dialogs_Common_SubscriptionExpired"),
                Message = _localizer.Get("Dialogs_Common_UpgradeToGetPlusFeatures"),
                PrimaryButtonText = _localizer.Get("Common_Actions_Upgrade"),
                CloseButtonText = _localizer.Get("Dialogs_SubscriptionExpired_MaybeLater"),
            });

        if (result is not ContentDialogResult.Primary) // Cancel exit
        {
            return;
        }

        await _accountUpgradeUrlLauncher.OpenAsync(ModalSource.Downgrade);
    }

    private async Task ResetNavigationAsync()
    {
        await _settingsViewNavigator.NavigateToCommonSettingsViewAsync(forceNavigation: true);
        await _mainViewNavigator.NavigateToDefaultAsync();
    }
}