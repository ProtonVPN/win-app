/*
 * Copyright (c) 2025 Proton AG
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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Models;

namespace ProtonVPN.Client.UI.Main.Home.Card.DefaultConnections;

public partial class DefaultConnectionSelectorViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly ISettingsViewNavigator _settingsViewNavigator;

    public DefaultConnection DefaultConnection => _settings.DefaultConnection;

    public bool IsFastestDefaultConnection => DefaultConnection.Type == DefaultConnectionType.Fastest;

    public bool IsRandomDefaultConnection => DefaultConnection.Type == DefaultConnectionType.Random;

    public bool IsLastDefaultConnection => DefaultConnection.Type == DefaultConnectionType.Last;

    public DefaultConnectionSelectorViewModel(
        IViewModelHelper viewModelHelper,
        ISettings settings,
        IMainWindowActivator mainWindowActivator,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator)
        : base(viewModelHelper)
    {
        _settings = settings;
        _mainWindowActivator = mainWindowActivator;
        _mainViewNavigator = mainViewNavigator;
        _settingsViewNavigator = settingsViewNavigator;
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.DefaultConnection))
        {
            ExecuteOnUIThread(InvalidateDefaultConnection);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateDefaultConnection();
    }

    private void InvalidateDefaultConnection()
    {
        if (IsActive)
        {
            OnPropertyChanged(nameof(DefaultConnection));
            OnPropertyChanged(nameof(IsFastestDefaultConnection));
            OnPropertyChanged(nameof(IsRandomDefaultConnection));
            OnPropertyChanged(nameof(IsLastDefaultConnection));
        }
    }

    [RelayCommand]
    private void SwitchToFastestDefaultConnection()
    {
        _settings.DefaultConnection = DefaultConnection.Fastest;
    }

    [RelayCommand]
    private void SwitchToRandomDefaultConnection()
    {
        _settings.DefaultConnection = DefaultConnection.Random;
    }

    [RelayCommand]
    private void SwitchToLastDefaultConnection()
    {
        _settings.DefaultConnection = DefaultConnection.Last;
    }

    [RelayCommand]
    private async Task<bool> NavigateToDefaultConnectionSettingsAsync()
    {
        _mainWindowActivator.Activate();

        return await _mainViewNavigator.NavigateToSettingsViewAsync()
            && await _settingsViewNavigator.NavigateToDefaultConnectionSettingsViewAsync(isDirectNavigation: true);
    }
}