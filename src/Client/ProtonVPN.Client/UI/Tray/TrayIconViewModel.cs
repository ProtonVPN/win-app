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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Models.Activation;

namespace ProtonVPN.Client.UI.Tray;

public partial class TrayIconViewModel : ViewModelBase, IEventMessageReceiver<ConnectionStatusChanged>, IEventMessageReceiver<LoggedInMessage>, IEventMessageReceiver<LoggedOutMessage>
{
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IRecentConnectionsProvider _recentConnectionsProvider;
    private readonly IConnectionManager _connectionManager;
    private readonly IUserAuthenticator _userAuthenticator;

    public ImageSource IconSource => GetIconSource();

    public string OpenApplicationLabel => Localizer.GetFormat("Tray_Actions_OpenApplication", App.APPLICATION_NAME);

    public TrayIconViewModel(
        ILocalizationProvider localizationProvider,
        IMainWindowActivator mainWindowActivator,
        IRecentConnectionsProvider recentConnectionsProvider,
        IConnectionManager connectionManager,
        IUserAuthenticator userAuthenticator)
        : base(localizationProvider)
    {
        _mainWindowActivator = mainWindowActivator;
        _recentConnectionsProvider = recentConnectionsProvider;
        _connectionManager = connectionManager;
        _userAuthenticator = userAuthenticator;
    }

    [RelayCommand]
    public void ShowApplication()
    {
        _mainWindowActivator.Activate();
    }

    [RelayCommand]
    public void ExitApplication()
    {
        _mainWindowActivator.Exit();
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    public async Task ConnectAsync()
    {
        IRecentConnection? mostRecentConnection = _recentConnectionsProvider.GetMostRecentConnection();

        await _connectionManager.ConnectAsync(mostRecentConnection?.ConnectionIntent);
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    public async Task DisconnectAsync()
    {
        await _connectionManager.DisconnectAsync();
    }

    public void Receive(ConnectionStatusChanged message)
    {
        InvalidateTray();
    }

    public void Receive(LoggedInMessage message)
    {
        InvalidateTray();
    }

    public void Receive(LoggedOutMessage message)
    {
        InvalidateTray();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(OpenApplicationLabel));
    }

    private bool CanConnect()
    {
        return _userAuthenticator.IsLoggedIn
            && _connectionManager.IsDisconnected;
    }

    private bool CanDisconnect()
    {
        return _userAuthenticator.IsLoggedIn
            && _connectionManager.IsConnected;
    }

    private void InvalidateTray()
    {
        ConnectCommand.NotifyCanExecuteChanged();
        DisconnectCommand.NotifyCanExecuteChanged();

        OnPropertyChanged(nameof(IconSource));
    }

    private ImageSource GetIconSource()
    {
        return !_userAuthenticator.IsLoggedIn
            ? ResourceHelper.GetIcon("ProtonVpnLoggedOutTrayIcon")
            : _connectionManager.IsConnected
                ? ResourceHelper.GetIcon("ProtonVpnProtectedTrayIcon")
                : ResourceHelper.GetIcon("ProtonVpnUnprotectedTrayIcon");
    }
}