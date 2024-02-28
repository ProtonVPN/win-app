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
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Home.ConnectionCard;

public abstract partial class ConnectionCardViewModelBase : ViewModelBase,
    IEventMessageReceiver<ConnectionStatusChanged>
{
    protected readonly IConnectionManager ConnectionManager;
    private readonly HomeViewModel _homeViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelConnectionCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowConnectionDetailsCommand))]
    [NotifyPropertyChangedFor(nameof(Header))]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitleAndFeature))]
    [NotifyPropertyChangedFor(nameof(ExitCountry))]
    [NotifyPropertyChangedFor(nameof(EntryCountry))]
    [NotifyPropertyChangedFor(nameof(IsSecureCore))]
    [NotifyPropertyChangedFor(nameof(IsTor))]
    [NotifyPropertyChangedFor(nameof(IsP2P))]
    [NotifyPropertyChangedFor(nameof(IsB2B))]
    [NotifyPropertyChangedFor(nameof(IsDisconnected))]
    [NotifyPropertyChangedFor(nameof(IsConnecting))]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    private ConnectionStatus _currentConnectionStatus;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitleAndFeature))]
    [NotifyPropertyChangedFor(nameof(ExitCountry))]
    [NotifyPropertyChangedFor(nameof(EntryCountry))]
    [NotifyPropertyChangedFor(nameof(IsSecureCore))]
    [NotifyPropertyChangedFor(nameof(IsTor))]
    [NotifyPropertyChangedFor(nameof(IsP2P))]
    [NotifyPropertyChangedFor(nameof(IsB2B))]
    private IConnectionIntent? _currentConnectionIntent;

    [ObservableProperty]
    private bool _isServerUnderMaintenance;

    public ConnectionDetails? CurrentConnectionDetails => ConnectionManager.CurrentConnectionDetails;

    public string Header =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Disconnected => CurrentConnectionIntent is null
                ? Localizer.Get("Home_ConnectionCard_Header_Recommended")
                : CurrentConnectionIntent.Location is FreeServerLocationIntent
                    ? Localizer.Get("Home_ConnectionCard_Header_FreeConnection")
                    : Localizer.Get("Home_ConnectionCard_Header_LastConnectedTo"),                    
            ConnectionStatus.Connecting => Localizer.Get("Home_ConnectionCard_Header_ConnectingTo"),
            ConnectionStatus.Connected => Localizer.Get("Home_ConnectionCard_Header_BrowseSafely"),
            _ => string.Empty,
        };

    public string Title =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected => Localizer.GetConnectionDetailsTitle(CurrentConnectionDetails),
            _ => Localizer.GetConnectionIntentTitle(CurrentConnectionIntent)
        };

    public string Subtitle =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected => Localizer.GetConnectionDetailsSubtitle(CurrentConnectionDetails),
            _ => Localizer.GetConnectionIntentSubtitle(CurrentConnectionIntent)
        };

    public string? ExitCountry =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected =>
                CurrentConnectionDetails?.OriginalConnectionIntent.Location switch
                {
                    CountryLocationIntent countryIntent => countryIntent.IsFastest ? string.Empty : CurrentConnectionDetails.ExitCountryCode,
                    GatewayServerLocationIntent gatewayServerIntent => CurrentConnectionDetails.ExitCountryCode,
                    FreeServerLocationIntent freeServerIntent => freeServerIntent.Type == FreeServerType.Fastest ? string.Empty : CurrentConnectionDetails.ExitCountryCode,
                    _ => string.Empty,
                },
            _ => CurrentConnectionIntent?.Location?.GetCountryCode()
        };

    public string? EntryCountry =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected =>
                CurrentConnectionDetails?.OriginalConnectionIntent.Feature is SecureCoreFeatureIntent secureCoreIntent && !secureCoreIntent.IsFastest
                    ? CurrentConnectionDetails.EntryCountryCode
                    : string.Empty,
            _ => (CurrentConnectionIntent?.Feature as SecureCoreFeatureIntent)?.EntryCountryCode
        };

    public bool IsSecureCore => IsFeature<SecureCoreFeatureIntent>(ServerFeatures.SecureCore);

    public bool IsTor => IsFeature<TorFeatureIntent>(ServerFeatures.Tor);

    public bool IsP2P => IsFeature<P2PFeatureIntent>(ServerFeatures.P2P);

    public bool IsB2B => IsFeature<B2BFeatureIntent>(ServerFeatures.B2B);

    public bool IsDisconnected => CurrentConnectionStatus == ConnectionStatus.Disconnected;

    public bool IsConnecting => CurrentConnectionStatus == ConnectionStatus.Connecting;

    public bool IsConnected => CurrentConnectionStatus == ConnectionStatus.Connected;

    public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);

    public bool HasSubtitleAndFeature => HasSubtitle && (IsTor || IsP2P);

    protected ConnectionCardViewModelBase(
        IConnectionManager connectionManager,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        HomeViewModel homeViewModel)
        : base(localizationProvider, logger, issueReporter)
    {
        ConnectionManager = connectionManager;
        _homeViewModel = homeViewModel;
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(InvalidateCurrentConnectionStatus);
    }

    protected override void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subtitle));
        OnPropertyChanged(nameof(HasSubtitle));
        OnPropertyChanged(nameof(HasSubtitleAndFeature));
    }

    protected virtual void InvalidateCurrentConnectionStatus()
    {
        CurrentConnectionStatus = ConnectionManager.ConnectionStatus;
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        await ConnectionManager.ConnectAsync(CurrentConnectionIntent);
    }

    private bool CanConnect()
    {
        return CurrentConnectionStatus == ConnectionStatus.Disconnected
            && !IsServerUnderMaintenance;
    }

    [RelayCommand(CanExecute = nameof(CanCancelConnection))]
    private async Task CancelConnectionAsync()
    {
        await ConnectionManager.DisconnectAsync();
    }

    private bool CanCancelConnection()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connecting;
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private async Task DisconnectAsync()
    {
        await ConnectionManager.DisconnectAsync();
    }

    private bool CanDisconnect()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connected;
    }

    [RelayCommand(CanExecute = nameof(CanShowConnectionDetails))]
    private void ShowConnectionDetails()
    {
        _homeViewModel.ToggleConnectionDetailsPane();
    }

    private bool CanShowConnectionDetails()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connected;
    }

    private bool IsFeature<TFeatureIntent>(ServerFeatures serverFeature)
        where TFeatureIntent : class, IFeatureIntent
    {
        return CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected => CurrentConnectionDetails != null
                                       && CurrentConnectionDetails?.OriginalConnectionIntent.Feature is TFeatureIntent
                                       && CurrentConnectionDetails.Server.Features.IsSupported(serverFeature),
            _ => CurrentConnectionIntent?.Feature is TFeatureIntent
        };
    }

}