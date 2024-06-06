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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.UI.Login.Forms;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Login;

public partial class DisableKillSwitchBannerViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionStatusChanged>
{
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;
    private readonly LoginShellViewModel _loginShellViewModel;

    [ObservableProperty]
    private bool _isKillSwitchNotificationVisible;

    public DisableKillSwitchBannerViewModel(
        ISettings settings,
        ILoginViewNavigator loginViewNavigator,
        IConnectionManager connectionManager,
        LoginShellViewModel loginShellViewModel,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter) : base(localizationProvider, logger, issueReporter)
    {
        _settings = settings;
        _connectionManager = connectionManager;
        _loginShellViewModel = loginShellViewModel;

        loginViewNavigator.Navigated += OnLoginViewNavigated;
    }

    private void OnLoginViewNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        InvalidateKillSwitchNotification();
    }

    public void Receive(ConnectionStatusChanged message)
    {
        InvalidateKillSwitchNotification();
    }

    private void InvalidateKillSwitchNotification()
    {
        IsKillSwitchNotificationVisible = _loginShellViewModel.CurrentPage is LoginFormViewModel or TwoFactorFormViewModel &&
                                          _settings.IsKillSwitchEnabled &&
                                          _settings.KillSwitchMode == KillSwitchMode.Advanced &&
                                          _connectionManager.IsNetworkBlocked;
    }

    [RelayCommand]
    private void DisableKillSwitch()
    {
        _settings.IsKillSwitchEnabled = false;
    }
}