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
using CommunityToolkit.Mvvm.Messaging;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;

namespace ProtonVPN.Client.UI.Home.ConnectionCard;

public partial class ConnectionCardViewModel : ViewModelBase, IRecipient<ConnectionStatusChanged>, IRecipient<RecentConnectionsChanged>
{
    private readonly IConnectionService _connectionService;
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
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    private IConnectionIntent? _currentConnectionIntent;

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

    public string Title => Localizer.GetConnectionIntentTitle(CurrentConnectionIntent);

    public string Subtitle => Localizer.GetConnectionIntentSubtitle(CurrentConnectionIntent);

    public bool HasSubtitle => !Subtitle.IsNullOrEmpty();

    public ConnectionCardViewModel(IConnectionService connectionService, IRecentConnectionsProvider recentConnectionsProvider, HomeViewModel homeViewModel)
    {
        _connectionService = connectionService;
        _recentConnectionsProvider = recentConnectionsProvider;

        _homeViewModel = homeViewModel;

        Messenger.RegisterAll(this);

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
        CurrentConnectionStatus = _connectionService.ConnectionStatus;
    }

    private void InvalidateCurrentConnectionIntent()
    {
        CurrentConnectionIntent = _recentConnectionsProvider.GetMostRecentConnection()?.ConnectionIntent;
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        await _connectionService.ConnectAsync(CurrentConnectionIntent);
    }

    private bool CanConnect()
    {
        return CurrentConnectionStatus == ConnectionStatus.Disconnected;
    }

    [RelayCommand(CanExecute = nameof(CanCancelConnection))]
    private async Task CancelConnectionAsync()
    {
        await _connectionService.CancelConnectionAsync();
    }

    private bool CanCancelConnection()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connecting;
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private async Task DisconnectAsync()
    {
        await _connectionService.DisconnectAsync();
    }

    private bool CanDisconnect()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connected;
    }

    [RelayCommand(CanExecute = nameof(CanShowConnectionDetails))]
    private void ShowConnectionDetails()
    {
        _homeViewModel.ShowConnectionDetails();
    }

    private bool CanShowConnectionDetails()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connected;
    }
}