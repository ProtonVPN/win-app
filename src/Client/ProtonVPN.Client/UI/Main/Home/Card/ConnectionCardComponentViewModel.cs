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
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.UI.Main.Home.Card;

public partial class ConnectionCardComponentViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<RecentConnectionsChangedMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private const int FREE_COUNTRIES_DISPLAYED_AS_FLAGS = 3;

    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IServersLoader _serversLoader;
    private readonly IMainWindowActivator _mainWindowActivator;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelConnectionCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowFreeConnectionsOverlayCommand))]
    [NotifyPropertyChangedFor(nameof(Profile))]
    [NotifyPropertyChangedFor(nameof(IsProfileIntent))]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitleOrFeature))]
    [NotifyPropertyChangedFor(nameof(IsDisconnected))]
    [NotifyPropertyChangedFor(nameof(IsConnecting))]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    [NotifyPropertyChangedFor(nameof(IsFreeConnectionsTaglineVisible))]
    [NotifyPropertyChangedFor(nameof(IsChangeServerOptionVisible))]
    [NotifyPropertyChangedFor(nameof(IsChangeDefaultConnectionOptionVisible))]
    [NotifyPropertyChangedFor(nameof(ExitCountry))]
    [NotifyPropertyChangedFor(nameof(EntryCountry))]
    [NotifyPropertyChangedFor(nameof(IsSecureCore))]
    [NotifyPropertyChangedFor(nameof(IsTor))]
    [NotifyPropertyChangedFor(nameof(IsP2P))]
    [NotifyPropertyChangedFor(nameof(IsB2B))]
    [NotifyPropertyChangedFor(nameof(FlagType))]
    private ConnectionStatus _currentConnectionStatus;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Profile))]
    [NotifyPropertyChangedFor(nameof(IsProfileIntent))]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitleOrFeature))]
    [NotifyPropertyChangedFor(nameof(ExitCountry))]
    [NotifyPropertyChangedFor(nameof(EntryCountry))]
    [NotifyPropertyChangedFor(nameof(IsSecureCore))]
    [NotifyPropertyChangedFor(nameof(IsTor))]
    [NotifyPropertyChangedFor(nameof(IsP2P))]
    [NotifyPropertyChangedFor(nameof(IsB2B))]
    [NotifyPropertyChangedFor(nameof(FlagType))]
    private IConnectionIntent? _currentConnectionIntent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Profile))]
    [NotifyPropertyChangedFor(nameof(IsProfileIntent))]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitleOrFeature))]
    [NotifyPropertyChangedFor(nameof(ExitCountry))]
    [NotifyPropertyChangedFor(nameof(EntryCountry))]
    [NotifyPropertyChangedFor(nameof(IsSecureCore))]
    [NotifyPropertyChangedFor(nameof(IsTor))]
    [NotifyPropertyChangedFor(nameof(IsP2P))]
    [NotifyPropertyChangedFor(nameof(IsB2B))]
    [NotifyPropertyChangedFor(nameof(FlagType))]
    private ConnectionDetails? _currentConnectionDetails;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedFreeCountriesCount))]
    private int _freeCountriesCount;

    public string FormattedFreeCountriesCount => FreeCountriesCount > FREE_COUNTRIES_DISPLAYED_AS_FLAGS
        ? $"+{FreeCountriesCount - FREE_COUNTRIES_DISPLAYED_AS_FLAGS}"
        : string.Empty;

    public IConnectionProfile? Profile => CurrentConnectionStatus switch
    {
        ConnectionStatus.Connected => CurrentConnectionDetails?.OriginalConnectionIntent as IConnectionProfile,
        _ => CurrentConnectionIntent as IConnectionProfile
    };

    public bool IsProfileIntent => Profile != null;

    public string Title => GetConnectionCardTitle();

    public string Subtitle => GetConnectionCardSubtitle();

    public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);

    public bool HasSubtitleOrFeature => HasSubtitle || IsTor || IsP2P;

    public bool IsDisconnected => CurrentConnectionStatus == ConnectionStatus.Disconnected;

    public bool IsConnecting => CurrentConnectionStatus == ConnectionStatus.Connecting;

    public bool IsConnected => CurrentConnectionStatus == ConnectionStatus.Connected;

    public bool IsFreeUser => !_settings.VpnPlan.IsPaid;

    public bool IsFreeConnectionsTaglineVisible => IsFreeUser && !IsConnected;

    public bool IsChangeServerOptionVisible => IsFreeUser && IsConnected;

    public bool IsChangeDefaultConnectionOptionVisible => !IsFreeUser && IsDisconnected;

    public string? ExitCountry =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected => CurrentConnectionDetails?.ExitCountryCode,
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

    public FlagType FlagType => (CurrentConnectionStatus switch
    {
        ConnectionStatus.Connected => CurrentConnectionDetails?.OriginalConnectionIntent.Location,
        _ => CurrentConnectionIntent?.Location
    }).GetFlagType(CurrentConnectionStatus is ConnectionStatus.Connected);

    public bool IsMainWindowFocused => _mainWindowActivator.IsWindowFocused;

    public ConnectionCardComponentViewModel(
        IViewModelHelper viewModelHelper,
        IConnectionManager connectionManager,
        ISettings settings,
        IRecentConnectionsManager recentConnectionsManager,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IServersLoader serversLoader,
        IMainWindowActivator mainWindowActivator)
        : base(viewModelHelper)
    {
        _connectionManager = connectionManager;
        _settings = settings;
        _recentConnectionsManager = recentConnectionsManager;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _serversLoader = serversLoader;
        _mainWindowActivator = mainWindowActivator;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            if (IsActive)
            {
                InvalidateConnectionStatus();

                if (message.ConnectionStatus == ConnectionStatus.Connected)
                {
                    InvalidateConnectionDetails();
                }
                else
                {
                    InvalidateConnectionIntent();
                }
            }
        });
    }

    public void Receive(ServerListChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateFreeCountriesCount);
        }
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateVpnPlan);
        }
    }

    public void Receive(RecentConnectionsChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateConnectionIntent);
        }
    }

    public void Receive(ProfilesChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateConnectionIntent);
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (IsActive && message.PropertyName == nameof(ISettings.DefaultConnection))
        {
            ExecuteOnUIThread(InvalidateConnectionIntent);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateConnectionStatus();
        InvalidateConnectionIntent();
        InvalidateConnectionDetails();
        InvalidateFreeCountriesCount();
        InvalidateVpnPlan();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subtitle));
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private Task ConnectAsync()
    {
        return _connectionManager.ConnectAsync(VpnTriggerDimension.ConnectionCard, CurrentConnectionIntent);
    }

    private bool CanConnect()
    {
        return IsDisconnected;
    }

    [RelayCommand(CanExecute = nameof(CanCancelConnection))]
    private Task CancelConnectionAsync()
    {
        return _connectionManager.DisconnectAsync(VpnTriggerDimension.ConnectionCard);
    }

    private bool CanCancelConnection()
    {
        return IsConnecting;
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private Task DisconnectAsync()
    {
        return _connectionManager.DisconnectAsync(VpnTriggerDimension.ConnectionCard);
    }

    private bool CanDisconnect()
    {
        return IsConnected;
    }

    [RelayCommand(CanExecute = nameof(CanShowFreeConnectionsOverlay))]
    private Task ShowFreeConnectionsOverlayAsync()
    {
        return _mainWindowOverlayActivator.ShowFreeConnectionsOverlayAsync();
    }

    private bool CanShowFreeConnectionsOverlay()
    {
        return IsDisconnected;
    }

    [RelayCommand]
    private Task ShowP2PInfoOverlayAsync()
    {
        return _mainWindowOverlayActivator.ShowP2PInfoOverlayAsync();
    }

    [RelayCommand]
    private Task ShowTorInfoOverlayAsync()
    {
        return _mainWindowOverlayActivator.ShowTorInfoOverlayAsync();
    }

    private void InvalidateConnectionStatus()
    {
        CurrentConnectionStatus = _connectionManager.ConnectionStatus;
    }

    private void InvalidateConnectionIntent()
    {
        IConnectionIntent? currentConnectionIntent = _connectionManager.CurrentConnectionIntent;

        CurrentConnectionIntent = _connectionManager.IsDisconnected || currentConnectionIntent == null
            ? _recentConnectionsManager.GetDefaultConnection()
            : currentConnectionIntent;

        // If intent is a profile, the reference might be the same even though profile details/settings have changed.
        // Force invalidating all properties.
        InvalidateAllProperties();
    }

    private void InvalidateConnectionDetails()
    {
        CurrentConnectionDetails = _connectionManager.CurrentConnectionDetails;
    }

    private void InvalidateFreeCountriesCount()
    {
        FreeCountriesCount = _serversLoader.GetFreeCountries().Count();
    }

    private void InvalidateVpnPlan()
    {
        OnPropertyChanged(nameof(IsFreeUser));
        OnPropertyChanged(nameof(IsFreeConnectionsTaglineVisible));
        OnPropertyChanged(nameof(IsChangeServerOptionVisible));
        OnPropertyChanged(nameof(IsChangeDefaultConnectionOptionVisible));
    }

    private string GetConnectionCardTitle()
    {
        return IsProfileIntent
            ? Profile!.Name
            : CurrentConnectionStatus switch
            {
                ConnectionStatus.Connected => Localizer.GetConnectionDetailsTitle(CurrentConnectionDetails),
                _ => Localizer.GetConnectionIntentTitle(CurrentConnectionIntent),
            };
    }

    private string GetConnectionCardSubtitle()
    {
        return CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected => IsProfileIntent
                ? Localizer.GetConnectionProfileDetailsSubtitle(CurrentConnectionDetails)
                : Localizer.GetConnectionDetailsSubtitle(CurrentConnectionDetails),
            _ => IsProfileIntent
                ? Localizer.GetConnectionProfileSubtitle(Profile)
                : Localizer.GetConnectionIntentSubtitle(CurrentConnectionIntent, useDetailedSubtitle: true),
        };
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