/*
 * Copyright (c) 2026 Proton AG
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
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.Handlers;

public class ExcludedLocationsSmartDiscoveryPromptHandler : IHandler,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private static readonly TimeSpan _quickDisconnectThreshold = TimeSpan.FromMinutes(1);

    private readonly ISettings _settings;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IConnectionManager _connectionManager;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly ISettingsViewNavigator _settingsViewNavigator;
    private readonly IProductPromptDisplayReporter _productPromptDisplayReporter;
    private readonly IProductPromptActionReporter _productPromptActionReporter;
    private readonly IUserAuthenticator _userAuthenticator;

    private DateTime? _connectionStartTimeUtc;
    private bool _wasConnectedToFastestCountry;

    public ExcludedLocationsSmartDiscoveryPromptHandler(
        ISettings settings,
        IUIThreadDispatcher uiThreadDispatcher,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IProductPromptDisplayReporter productPromptDisplayReporter,
        IProductPromptActionReporter productPromptActionReporter,
        IUserAuthenticator userAuthenticator)
    {
        _settings = settings;
        _uiThreadDispatcher = uiThreadDispatcher;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _connectionManager = connectionManager;
        _mainViewNavigator = mainViewNavigator;
        _settingsViewNavigator = settingsViewNavigator;
        _productPromptDisplayReporter = productPromptDisplayReporter;
        _productPromptActionReporter = productPromptActionReporter;
        _userAuthenticator = userAuthenticator;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (!ShouldShowPrompt())
        {
            return;
        }

        switch (message.ConnectionStatus)
        {
            case ConnectionStatus.Connected:
                HandleConnected();
                break;

            case ConnectionStatus.Disconnected:
                HandleDisconnected();
                break;

            case ConnectionStatus.Connecting:
                if (_wasConnectedToFastestCountry)
                {
                    ResetTracking();
                }
                break;
        }
    }

    private void HandleConnected()
    {
        IConnectionIntent? currentIntent = _connectionManager.CurrentConnectionIntent;

        if (IsFastestCountryConnection(currentIntent))
        {
            _connectionStartTimeUtc = DateTime.UtcNow;
            _wasConnectedToFastestCountry = true;
        }
        else
        {
            ResetTracking();
        }
    }

    private void HandleDisconnected()
    {
        bool wasQuickDisconnect = _connectionStartTimeUtc.HasValue &&
                                   (DateTime.UtcNow - _connectionStartTimeUtc.Value) < _quickDisconnectThreshold;

        if (_wasConnectedToFastestCountry && wasQuickDisconnect)
        {
            _uiThreadDispatcher.TryEnqueue(ShowExcludedLocationsSmartDiscoveryPromptAsync);
        }

        ResetTracking();
    }

    private bool ShouldShowPrompt()
    {
        return _settings.VpnPlan.IsPaid 
            && !_settings.WasExcludedLocationsSmartDiscoveryPromptDisplayed
            && _userAuthenticator.IsLoggedIn;
    }

    private bool IsFastestCountryConnection(IConnectionIntent? connectionIntent)
    {
        if (connectionIntent == null)
        {
            return false;
        }

        if (connectionIntent is IConnectionProfile)
        {
            return false;
        }

        return connectionIntent.Location is MultiCountryLocationIntent intent
            && intent.IsSelectionEmpty;
    }

    private async void ShowExcludedLocationsSmartDiscoveryPromptAsync()
    {
        _settings.WasExcludedLocationsSmartDiscoveryPromptDisplayed = true;
        _productPromptDisplayReporter.Report(PromptType.FeatureDiscovery, PromptContext.ConnectionPreferencesFirstConnection);

        ContentDialogResult result = await _mainWindowOverlayActivator.ShowExcludedLocationsSmartDiscoveryPromptAsync();

        if (result == ContentDialogResult.Primary)
        {
            _productPromptActionReporter.Report(PromptType.FeatureDiscovery, PromptContext.ConnectionPreferencesFirstConnection, PromptAction.Configure);

            await _mainViewNavigator.NavigateToSettingsViewAsync();
            await _settingsViewNavigator.NavigateToConnectionPreferencesSettingsViewAsync();
        }
        else
        {
            _productPromptActionReporter.Report(PromptType.FeatureDiscovery, PromptContext.ConnectionPreferencesFirstConnection, PromptAction.Dismiss);
        }
    }

    private void ResetTracking()
    {
        _connectionStartTimeUtc = null;
        _wasConnectedToFastestCountry = false;
    }
}