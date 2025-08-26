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

using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.DefaultConnections;

public partial class DefaultConnectionSettingsPageViewModel : SettingsPageViewModelBase,
    IEventMessageReceiver<RecentConnectionsChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IServersLoader _serversLoader;
    private readonly IRecentConnectionsManager _recentConnectionsManager;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.DefaultConnection))]
    [NotifyPropertyChangedFor(nameof(IsFastestConnection))]
    [NotifyPropertyChangedFor(nameof(IsLastConnection))]
    private DefaultConnection _currentDefaultConnection;

    public override string Title => Localizer.Get("Settings_Connection_Default");

    public bool IsFastestConnection
    {
        get => IsDefaultConnectionType(DefaultConnectionType.Fastest);
        set => SetDefaultConnectionType(value, DefaultConnectionType.Fastest);
    }

    public bool IsRandomConnection
    {
        get => IsDefaultConnectionType(DefaultConnectionType.Random);
        set => SetDefaultConnectionType(value, DefaultConnectionType.Random);
    }

    public bool IsLastConnection
    {
        get => IsDefaultConnectionType(DefaultConnectionType.Last);
        set => SetDefaultConnectionType(value, DefaultConnectionType.Last);
    }

    public bool IsFastestAndRandomConnectionVisible => _serversLoader.HasAnyCountries();

    public SmartObservableCollection<RecentDefaultConnectionObservable> Recents { get; } = [];

    public bool HasRecents => Recents.Any();

    public DefaultConnectionSettingsPageViewModel(
        IServersLoader serversLoader,
        IRecentConnectionsManager recentConnectionsManager,
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
        _serversLoader = serversLoader;
        _recentConnectionsManager = recentConnectionsManager;
        InvalidateRecents();

        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.DefaultConnection, () => CurrentDefaultConnection)
        ];
    }

    protected override void OnRetrieveSettings()
    {
        CurrentDefaultConnection = Settings.DefaultConnection;
    }

    private bool IsDefaultConnectionType(DefaultConnectionType defaultConnectionType)
    {
        return CurrentDefaultConnection.Type == defaultConnectionType;
    }

    private void SetDefaultConnectionType(bool value, DefaultConnectionType defaultConnectionType, Guid? recentId = null)
    {
        if (value)
        {
            CurrentDefaultConnection = defaultConnectionType switch
            {
                DefaultConnectionType.Fastest => DefaultConnection.Fastest,
                DefaultConnectionType.Random => DefaultConnection.Random,
                DefaultConnectionType.Last => DefaultConnection.Last,
                DefaultConnectionType.Recent when recentId.HasValue => new DefaultConnection(recentId.Value),
                _ => DefaultSettings.DefaultConnection
            };
        }
    }

    public void Receive(RecentConnectionsChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateRecents);
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(IsFastestAndRandomConnectionVisible));
        });
    }

    private void InvalidateRecents()
    {
        List<IRecentConnection> recents = _recentConnectionsManager.GetRecentConnections().ToList();
        DefaultConnection defaultConnection = Settings.DefaultConnection;

        Recents.Reset(recents.Select(r => new RecentDefaultConnectionObservable(Localizer, defaultConnection, this, r)));

        OnPropertyChanged(nameof(HasRecents));
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        base.OnSettingsChanged(propertyName);
        if (propertyName == nameof(ISettings.DefaultConnection))
        {
            InvalidateRecentDefaultConnection();
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateRecentDefaultConnection();
    }

    private void InvalidateRecentDefaultConnection()
    {
        DefaultConnection defaultConnection = Settings.DefaultConnection;
        List<RecentDefaultConnectionObservable> recents = Recents.ToList();
        foreach (RecentDefaultConnectionObservable recent in recents)
        {
            recent.OnDefaultConnectionChange(defaultConnection);
        }
    }

    public void SetRecentAsDefaultConnection(IRecentConnection recent)
    {
        SetDefaultConnectionType(true, DefaultConnectionType.Recent, recent.Id);
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateRecents);
    }
}