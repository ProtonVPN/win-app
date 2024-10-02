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

using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Legacy.Models.Activation;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.Models.Urls;
using ProtonVPN.Client.Notifications.Contracts;
using ProtonVPN.Client.Legacy.UI.Home;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Legacy.Handlers;

public class VpnPlanChangedHandler : IHandler,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChanged>
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private readonly ILogger _logger;
    private readonly ILocalizationProvider _localizer;
    private readonly IConnectionManager _connectionManager;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly IOverlayActivator _overlayActivator;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IUrls _urls;
    private readonly ISubscriptionExpiredNotificationSender _subscriptionExpiredNotificationSender;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    private bool _notifyOnNextConnection;

    public VpnPlanChangedHandler(ILogger logger,
        ILocalizationProvider localizer,
        IConnectionManager connectionManager,
        IUserAuthenticator userAuthenticator,
        IMainViewNavigator mainViewNavigator,
        IOverlayActivator overlayActivator,
        IWebAuthenticator webAuthenticator,
        IUrls urls,
        ISubscriptionExpiredNotificationSender subscriptionExpiredNotificationSender,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _logger = logger;
        _localizer = localizer;
        _connectionManager = connectionManager;
        _userAuthenticator = userAuthenticator;
        _mainViewNavigator = mainViewNavigator;
        _overlayActivator = overlayActivator;
        _webAuthenticator = webAuthenticator;
        _urls = urls;
        _subscriptionExpiredNotificationSender = subscriptionExpiredNotificationSender;
        _uiThreadDispatcher = uiThreadDispatcher;
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
            _uiThreadDispatcher.TryEnqueue(() => _mainViewNavigator.NavigateToAsync<HomeViewModel>(null, false, true));
        }

        if (message.IsDowngrade())
        {
            await HandlePlanDowngradeAsync();
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
        await _dispatcherQueue.EnqueueAsync(async () => ShowSubscriptionExpiredDialogAsync());
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

        _urls.NavigateTo(await _webAuthenticator.GetUpgradeAccountUrlAsync(ModalSources.Downgrade));
    }

    public async void Receive(ConnectionStatusChanged message)
    {
        if (_notifyOnNextConnection && message.ConnectionStatus == ConnectionStatus.Connected)
        {
            _notifyOnNextConnection = false;
            await NotifyUserOfSubscriptionExpirationAsync();
        }
    }
}