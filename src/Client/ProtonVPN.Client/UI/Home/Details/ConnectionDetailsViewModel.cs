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

using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.UI.Dialogs.Overlays;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Home.Details;

public partial class ConnectionDetailsViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<MainWindowStateChangedMessage>
{
    private const int REFRESH_TIMER_INTERVAL_IN_MS = 1000;

    private readonly IConnectionManager _connectionManager;
    private readonly IOverlayActivator _overlayActivator;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly VpnSpeedViewModel _vpnSpeedViewModel;

    private readonly DispatcherTimer _refreshTimer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasGateway))]
    [NotifyPropertyChangedFor(nameof(Gateway))]
    [NotifyPropertyChangedFor(nameof(HasCountry))]
    [NotifyPropertyChangedFor(nameof(Country))]
    [NotifyPropertyChangedFor(nameof(IsSecureCore))]
    [NotifyPropertyChangedFor(nameof(SecureCoreLabel))]
    [NotifyPropertyChangedFor(nameof(HasCity))]
    [NotifyPropertyChangedFor(nameof(HasServer))]
    [NotifyPropertyChangedFor(nameof(ServerLoad))]
    [NotifyPropertyChangedFor(nameof(FormattedServerLoad))]
    [NotifyPropertyChangedFor(nameof(HasServerLatency))]
    [NotifyPropertyChangedFor(nameof(ServerLatency))]
    [NotifyPropertyChangedFor(nameof(HasVpnProtocol))]
    [NotifyPropertyChangedFor(nameof(VpnProtocol))]
    private ConnectionDetails? _currentConnectionDetails;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedSessionLength))]
    private TimeSpan? _sessionLength;

    public string? FormattedSessionLength => Localizer.GetFormattedTime(SessionLength ?? TimeSpan.Zero);

    public bool HasGateway => CurrentConnectionDetails?.Server != null
                           && CurrentConnectionDetails.IsGateway
                           && !string.IsNullOrEmpty(CurrentConnectionDetails.GatewayName);

    public string? Gateway => CurrentConnectionDetails?.GatewayName;

    public bool HasCountry => CurrentConnectionDetails?.Server != null
                           && !string.IsNullOrEmpty(CurrentConnectionDetails.ExitCountryCode);

    public string? Country => Localizer.GetCountryName(CurrentConnectionDetails?.ExitCountryCode);

    public bool IsSecureCore => CurrentConnectionDetails?.Server != null
                             && CurrentConnectionDetails.IsSecureCore
                             && !string.IsNullOrEmpty(CurrentConnectionDetails.EntryCountryCode);

    public string? SecureCoreLabel => string.IsNullOrEmpty(CurrentConnectionDetails?.EntryCountryCode)
        ? null
        : Localizer.GetSecureCoreLabel(CurrentConnectionDetails.EntryCountryCode);

    public bool HasCity => CurrentConnectionDetails?.Server != null
                        && !string.IsNullOrEmpty(CurrentConnectionDetails.CityState);

    public bool HasServer => CurrentConnectionDetails?.Server != null
                          && !string.IsNullOrEmpty(CurrentConnectionDetails.ServerName);

    public double ServerLoad => CurrentConnectionDetails?.ServerLoad ?? 0;

    public string FormattedServerLoad => $"{ServerLoad:P0}";

    public bool HasServerLatency => CurrentConnectionDetails?.ServerLatency != null;

    public string? ServerLatency => Localizer.GetFormat("Format_Milliseconds", CurrentConnectionDetails?.ServerLatency?.TotalMilliseconds ?? 0);

    public bool HasVpnProtocol => CurrentConnectionDetails?.Protocol != null;

    public string VpnProtocol => CurrentConnectionDetails is null
        ? string.Empty
        : Localizer.GetVpnProtocol(CurrentConnectionDetails.Protocol);

    public ConnectionDetailsViewModel(
        ILocalizationProvider localizationProvider,
        IConnectionManager connectionManager,
        IOverlayActivator overlayActivator,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainWindowActivator mainWindowActivator,
        VpnSpeedViewModel vpnSpeedViewModel)
        : base(localizationProvider, logger, issueReporter)
    {
        _connectionManager = connectionManager;
        _overlayActivator = overlayActivator;
        _mainWindowActivator = mainWindowActivator;
        _vpnSpeedViewModel = vpnSpeedViewModel;

        _refreshTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(REFRESH_TIMER_INTERVAL_IN_MS)
        };
        _refreshTimer.Tick += OnRefreshTimerTick;
    }

    [RelayCommand]
    public async Task OpenServerLoadOverlayAsync()
    {
        await _overlayActivator.ShowOverlayAsync<ServerLoadOverlayViewModel>();
    }

    [RelayCommand]
    public async Task OpenLatencyOverlayAsync()
    {
        await _overlayActivator.ShowOverlayAsync<LatencyOverlayViewModel>();
    }

    [RelayCommand]
    public async Task OpenProtocolOverlayAsync()
    {
        await _overlayActivator.ShowOverlayAsync<ProtocolOverlayViewModel>();
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            CurrentConnectionDetails = _connectionManager.IsConnected
                ? _connectionManager.CurrentConnectionDetails
                : null;

            InvalidateSessionLength();
        });
    }

    public void Receive(MainWindowStateChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAutoRefreshTimer);
    }

    protected override void OnActivated()
    {
        InvalidateAutoRefreshTimer();
    }

    protected override void OnDeactivated()
    {
        InvalidateAutoRefreshTimer();
    }

    protected override void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(FormattedSessionLength));
        OnPropertyChanged(nameof(Country));
        OnPropertyChanged(nameof(SecureCoreLabel));
        OnPropertyChanged(nameof(ServerLatency));
        OnPropertyChanged(nameof(VpnProtocol));
    }

    private void InvalidateSessionLength()
    {
        SessionLength = CurrentConnectionDetails is null
            ? null
            : DateTime.UtcNow - CurrentConnectionDetails.EstablishedConnectionTimeUtc;
    }

    private void InvalidateAutoRefreshTimer()
    {
        if (IsActive && !_mainWindowActivator.IsWindowMinimized)
        {
            StartAutoRefresh();
        }
        else
        {
            StopAutoRefresh();
        }
    }

    private void StartAutoRefresh()
    {
        if (!_refreshTimer.IsEnabled)
        {
            Refresh();

            _refreshTimer.Start();
        }
    }

    private void StopAutoRefresh()
    {
        if (_refreshTimer.IsEnabled)
        {
            _refreshTimer.Stop();
        }
    }

    private void Refresh()
    {
        InvalidateSessionLength();

        _vpnSpeedViewModel.RefreshAsync();
    }

    private void OnRefreshTimerTick(object? sender, EventArgs e)
    {
        Refresh();
    }
}