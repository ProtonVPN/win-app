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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Home.Details;

public partial class ConnectionDetailsViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChanged>
{
    private const int REFRESH_TIMER_INTERVAL_IN_MS = 1000;

    private readonly IConnectionManager _connectionManager;
    private readonly IOverlayActivator _overlayActivator;
    private readonly VpnSpeedViewModel _vpnSpeedViewModel;

    private readonly DispatcherTimer _refreshTimer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SessionLength))]
    [NotifyPropertyChangedFor(nameof(FormattedSessionLength))]
    [NotifyPropertyChangedFor(nameof(Gateway))]
    [NotifyPropertyChangedFor(nameof(Country))]
    [NotifyPropertyChangedFor(nameof(SecureCoreLabel))]
    [NotifyPropertyChangedFor(nameof(ServerLoad))]
    [NotifyPropertyChangedFor(nameof(FormattedServerLoad))]
    [NotifyPropertyChangedFor(nameof(ServerLatency))]
    [NotifyPropertyChangedFor(nameof(VpnProtocol))]
    private ConnectionDetails? _currentConnectionDetails;

    public TimeSpan? SessionLength => CurrentConnectionDetails is null
        ? null
        : DateTime.UtcNow - CurrentConnectionDetails.EstablishedConnectionTimeUtc;

    public string? FormattedSessionLength => SessionLength is null
        ? null
        : Localizer.GetFormattedTime(SessionLength.Value);

    public string? Gateway => CurrentConnectionDetails?.GatewayName;

    public string? Country => string.IsNullOrEmpty(CurrentConnectionDetails?.ExitCountryCode)
        ? null
        : Localizer.GetCountryName(CurrentConnectionDetails.ExitCountryCode);

    public string? SecureCoreLabel => string.IsNullOrEmpty(CurrentConnectionDetails?.EntryCountryCode)
        ? null
        : Localizer.GetSecureCoreLabel(CurrentConnectionDetails.EntryCountryCode);

    public double ServerLoad => CurrentConnectionDetails?.ServerLoad ?? 0;

    public string FormattedServerLoad => $"{ServerLoad:P0}";

    public string? ServerLatency => CurrentConnectionDetails?.ServerLatency is null
        ? null
        : Localizer.GetFormat("Format_Milliseconds", CurrentConnectionDetails.ServerLatency.Value.TotalMilliseconds);

    public string VpnProtocol => CurrentConnectionDetails is null
        ? string.Empty
        : Localizer.GetVpnProtocol(CurrentConnectionDetails.Protocol);

    public ConnectionDetailsViewModel(
        ILocalizationProvider localizationProvider,
        IConnectionManager connectionManager,
        IOverlayActivator overlayActivator,
        ILogger logger,
        IIssueReporter issueReporter,
        VpnSpeedViewModel vpnSpeedViewModel)
        : base(localizationProvider, logger, issueReporter)
    {
        _connectionManager = connectionManager;
        _overlayActivator = overlayActivator;
        _vpnSpeedViewModel = vpnSpeedViewModel;

        _refreshTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(REFRESH_TIMER_INTERVAL_IN_MS)
        };
        _refreshTimer.Tick += OnRefreshTimerTick;
    }

    [RelayCommand]
    public async Task OpenOverlayAsync(string dialogKey)
    {
        await _overlayActivator.ShowOverlayAsync(dialogKey);
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            CurrentConnectionDetails = _connectionManager.IsConnected
                ? _connectionManager.CurrentConnectionDetails
                : null;
        });
    }

    protected override void OnActivated()
    {
        StartAutoRefresh();
    }

    protected override void OnDeactivated()
    {
        StopAutoRefresh();
    }

    protected override void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(FormattedSessionLength));
        OnPropertyChanged(nameof(Country));
        OnPropertyChanged(nameof(SecureCoreLabel));
        OnPropertyChanged(nameof(ServerLatency));
        OnPropertyChanged(nameof(VpnProtocol));
    }

    private void StartAutoRefresh()
    {
        Refresh();

        if (!_refreshTimer.IsEnabled)
        {
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
        OnPropertyChanged(nameof(FormattedSessionLength));

        _vpnSpeedViewModel.RefreshAsync();
    }

    private void OnRefreshTimerTick(object? sender, EventArgs e)
    {
        Refresh();
    }
}