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

using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Common.Attributes;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Preferences;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;
using ProtonVPN.Client.Logic.Searches.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Services.DefaultConnections;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Client.Services.LocationExclusion;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.ConnectionPreferences;

public partial class ConnectionPreferencesSettingsPageViewModel : SettingsPageViewModelBase,
    IEventMessageReceiver<RecentConnectionsChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IDefaultConnectionSelectionManager _defaultConnectionSelectionManager;
    private readonly IExcludeLocationsManager _excludeLocationsManager;
    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.DefaultConnection))]
    private object? _selectedDefaultConnection;

    public override string Title => Localizer.Get("Settings_Connection_ConnectionPreferences");

    public SmartObservableCollection<object> ConnectionsList => _defaultConnectionSelectionManager.Connections;

    public bool IsPaidUser => Settings.VpnPlan.IsPaid;

    public ConnectionPreferencesSettingsPageViewModel(
        IDefaultConnectionSelectionManager defaultConnectionSelectionManager,
        IExcludeLocationsManager excludeLocationsManager,
        IGlobalSearch globalSearch,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               viewModelHelper)
    {
        _defaultConnectionSelectionManager = defaultConnectionSelectionManager;
        _excludeLocationsManager = excludeLocationsManager;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;

        InvalidateAllConnections();

        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.DefaultConnection, () => _defaultConnectionSelectionManager.TryCreateDefaultConnection(SelectedDefaultConnection)),
            ChangedSettingArgs.Create(() => Settings.ExcludedLocationsList, () => GetExcludedLocations())
        ];

        Locations.CollectionChanged += OnLocationsCollectionChanged;
        Locations.ItemPropertyChanged += OnLocationItemPropertyChanged;
    }

    public void Receive(RecentConnectionsChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAllConnections);
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAllConnections);
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateAllConnections);
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        UpdateSelectedDefaultConnection();
        InvalidateExcludableLocations();
    }

    protected override void OnRetrieveSettings()
    {
        UpdateSelectedDefaultConnection();
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        base.OnSettingsChanged(propertyName);
        if (propertyName == nameof(ISettings.DefaultConnection))
        {
            UpdateSelectedDefaultConnection();
        }
    }

    private void UpdateSelectedDefaultConnection()
    {
        SelectedDefaultConnection = _defaultConnectionSelectionManager.FindSelectedItem(Settings.DefaultConnection);
    }

    [RelayCommand]
    private Task TriggerDefaultConnectionUpsellProcessAsync()
    {
        return _upsellCarouselWindowActivator.ActivateAsync(UpsellFeatureType.Profiles);
    }

    [RelayCommand]
    private Task TriggerExcludedLocationsUpsellAsync()
    {
        return _upsellCarouselWindowActivator.ActivateAsync(UpsellFeatureType.AdvancedSettings);
    }

    private void InvalidateAllConnections()
    {
        _defaultConnectionSelectionManager.Refresh();
        UpdateSelectedDefaultConnection();
    }
}