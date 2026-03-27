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

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Services.DefaultConnections;

namespace ProtonVPN.Client.UI.Main.Home.Card.DefaultConnections;

public partial class DefaultConnectionSelectorViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<RecentConnectionsChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>
{
    private readonly ISettings _settings;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly ISettingsViewNavigator _settingsViewNavigator;
    private readonly IDefaultConnectionSelectionManager _defaultConnectionSelectionManager;
    private bool _isSelectionUpdateInProgress;

    public SmartObservableCollection<object> ConnectionsList => _defaultConnectionSelectionManager.Connections;

    [ObservableProperty]
    private object? _selectedDefaultConnection;

    public DefaultConnectionSelectorViewModel(
        IViewModelHelper viewModelHelper,
        ISettings settings,
        IMainWindowActivator mainWindowActivator,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IDefaultConnectionSelectionManager defaultConnectionSelectionManager)
        : base(viewModelHelper)
    {
        _settings = settings;
        _mainWindowActivator = mainWindowActivator;
        _mainViewNavigator = mainViewNavigator;
        _settingsViewNavigator = settingsViewNavigator;
        _defaultConnectionSelectionManager = defaultConnectionSelectionManager;
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.DefaultConnection))
        {
            ExecuteOnUIThread(UpdateSelectedDefaultConnection);
        }
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(RefreshConnections);
    }

    public void Receive(RecentConnectionsChangedMessage message)
    {
        ExecuteOnUIThread(RefreshConnections);
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(RefreshConnections);
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        RefreshConnections();
    }

    private void RefreshConnections()
    {
        _defaultConnectionSelectionManager.Refresh();
        UpdateSelectedDefaultConnection();
    }

    private void UpdateSelectedDefaultConnection()
    {
        if (!IsActive)
        {
            return;
        }

        _isSelectionUpdateInProgress = true;
        SelectedDefaultConnection = _defaultConnectionSelectionManager.FindSelectedItem(_settings.DefaultConnection);
        _isSelectionUpdateInProgress = false;
    }

    partial void OnSelectedDefaultConnectionChanged(object? value)
    {
        if (_isSelectionUpdateInProgress || !IsActive)
        {
            return;
        }

        DefaultConnection? selected = _defaultConnectionSelectionManager.TryCreateDefaultConnection(value);
        if (selected is null)
        {
            return;
        }

        if (_settings.DefaultConnection != selected)
        {
            _settings.DefaultConnection = selected.Value;
        }
    }
}