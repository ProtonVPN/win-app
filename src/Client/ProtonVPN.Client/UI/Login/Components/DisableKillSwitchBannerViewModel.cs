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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Extensions;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.Login.Pages;

namespace ProtonVPN.Client.UI.Login.Components;

public partial class DisableKillSwitchBannerViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;
    private readonly ILoginViewNavigator _loginViewNavigator;
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    private bool _isKillSwitchNotificationVisible;

    public DisableKillSwitchBannerViewModel(
        ISettings settings,
        ILoginViewNavigator loginViewNavigator,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _settings = settings;
        _loginViewNavigator = loginViewNavigator;
        _connectionManager = connectionManager;

        loginViewNavigator.Navigated += OnLoginViewNavigated;
    }

    private void OnLoginViewNavigated(object sender, NavigationEventArgs e)
    {
        InvalidateKillSwitchNotification();
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateKillSwitchNotification);
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateKillSwitchNotification);
    }

    private void InvalidateKillSwitchNotification()
    {
        IsKillSwitchNotificationVisible = _loginViewNavigator.GetCurrentPageContext() is SignInPageViewModel or TwoFactorPageViewModel &&
                                          _settings.IsAdvancedKillSwitchActive() &&
                                          _connectionManager.IsNetworkBlocked;
    }

    [RelayCommand]
    private void DisableKillSwitch()
    {
        _settings.IsKillSwitchEnabled = false;
    }
}