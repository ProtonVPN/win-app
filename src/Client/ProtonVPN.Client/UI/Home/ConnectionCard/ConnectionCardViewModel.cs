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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;

namespace ProtonVPN.Client.UI.Home.ConnectionCard;

public partial class ConnectionCardViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<RecentConnectionsChanged>
{
    private readonly IConnectionManager _connectionManager;
    private readonly IRecentConnectionsProvider _recentConnectionsProvider;

    private readonly HomeViewModel _homeViewModel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelConnectionCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowConnectionDetailsCommand))]
    private ConnectionStatus _currentConnectionStatus;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    [NotifyPropertyChangedFor(nameof(ExitCountry))]
    [NotifyPropertyChangedFor(nameof(EntryCountry))]
    [NotifyPropertyChangedFor(nameof(IsSecureCore))]
    [NotifyPropertyChangedFor(nameof(IsTor))]
    [NotifyPropertyChangedFor(nameof(IsP2P))]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitleAndFeature))]
    private IConnectionIntent? _currentConnectionIntent;

    [ObservableProperty]
    private bool _isServerUnderMaintenance;

    public string Header =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Disconnected => CurrentConnectionIntent is null
                ? Localizer.Get("Home_ConnectionCard_Header_Recommended")
                : Localizer.Get("Home_ConnectionCard_Header_LastConnectedTo"),
            ConnectionStatus.Connecting => Localizer.Get("Home_ConnectionCard_Header_ConnectingTo"),
            ConnectionStatus.Connected => Localizer.Get("Home_ConnectionCard_Header_BrowseSafely")
        };

    public string? ExitCountry => (CurrentConnectionIntent?.Location as CountryLocationIntent)?.CountryCode;

    public string? EntryCountry => (CurrentConnectionIntent?.Feature as SecureCoreFeatureIntent)?.EntryCountryCode;

    public bool IsSecureCore => CurrentConnectionIntent?.Feature is SecureCoreFeatureIntent;

    public bool IsTor => CurrentConnectionIntent?.Feature is TorFeatureIntent;

    public bool IsP2P => CurrentConnectionIntent?.Feature is P2PFeatureIntent;

    public string Title => Localizer.GetConnectionIntentTitle(CurrentConnectionIntent);

    public string Subtitle => Localizer.GetConnectionIntentSubtitle(CurrentConnectionIntent);

    public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);

    public bool HasSubtitleAndFeature => HasSubtitle && (IsTor || IsP2P);

    public ConnectionCardViewModel(IConnectionManager connectionManager,
        IRecentConnectionsProvider recentConnectionsProvider,
        ILocalizationProvider localizationProvider,
        HomeViewModel homeViewModel)
        : base(localizationProvider)
    {
        _connectionManager = connectionManager;
        _recentConnectionsProvider = recentConnectionsProvider;
        _homeViewModel = homeViewModel;

        InvalidateCurrentConnectionStatus();
        InvalidateCurrentConnectionIntent();
    }

    public void Receive(ConnectionStatusChanged message)
    {
        InvalidateCurrentConnectionStatus();
    }

    public void Receive(RecentConnectionsChanged message)
    {
        InvalidateCurrentConnectionIntent();
    }

    protected override void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subtitle));
        OnPropertyChanged(nameof(HasSubtitle));
    }

    private void InvalidateCurrentConnectionStatus()
    {
        CurrentConnectionStatus = _connectionManager.ConnectionStatus;
    }

    private void InvalidateCurrentConnectionIntent()
    {
        IRecentConnection? mostRecentConnection = _recentConnectionsProvider.GetMostRecentConnection();

        CurrentConnectionIntent = mostRecentConnection?.ConnectionIntent;
        IsServerUnderMaintenance = mostRecentConnection?.IsServerUnderMaintenance ?? false;
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        await _connectionManager.ConnectAsync(CurrentConnectionIntent);
    }

    private bool CanConnect()
    {
        return CurrentConnectionStatus == ConnectionStatus.Disconnected
            && !IsServerUnderMaintenance;
    }

    [RelayCommand(CanExecute = nameof(CanCancelConnection))]
    private async Task CancelConnectionAsync()
    {
        await _connectionManager.DisconnectAsync();
    }

    private bool CanCancelConnection()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connecting;
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private async Task DisconnectAsync()
    {
        await _connectionManager.DisconnectAsync();
    }

    private bool CanDisconnect()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connected;
    }

    [RelayCommand(CanExecute = nameof(CanShowConnectionDetails))]
    private void ShowConnectionDetails()
    {
        _homeViewModel.OpenDetailsPane();
    }

    private bool CanShowConnectionDetails()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connected;
    }
}