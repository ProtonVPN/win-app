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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.Contracts.Services.Navigation;

namespace ProtonVPN.Client.UI.Main.Home.Details.Connection;

public partial class ConnectionDetailsPageViewModel : PageViewModelBase<IDetailsViewNavigator>,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<ConnectionDetailsChanged>,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private const int REFRESH_TIMER_INTERVAL_IN_MS = 1000;

    private readonly IConnectionManager _connectionManager;
    private readonly VpnSpeedViewModel _vpnSpeedViewModel;
    private readonly DispatcherTimer _refreshTimer;

    private bool _isMainWindowVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasState))]
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

    [ObservableProperty]
    private string _serverIpAddress;

    public string? FormattedSessionLength => Localizer.GetFormattedTime(SessionLength ?? TimeSpan.Zero);

    public bool HasState => CurrentConnectionDetails?.Server != null
                         && !string.IsNullOrEmpty(CurrentConnectionDetails.State);

    public bool HasCity => CurrentConnectionDetails?.Server != null
                        && !string.IsNullOrEmpty(CurrentConnectionDetails.City);

    public bool HasServer => CurrentConnectionDetails?.Server != null
                          && !string.IsNullOrEmpty(CurrentConnectionDetails.ServerName);

    public double ServerLoad => CurrentConnectionDetails?.ServerLoad ?? 0;

    public string FormattedServerLoad => $"{ServerLoad:P0}";

    public bool HasServerLatency => CurrentConnectionDetails?.ServerLatency != null;

    public string ServerLatency => Localizer.GetFormat("Format_Milliseconds", CurrentConnectionDetails?.ServerLatency?.TotalMilliseconds ?? 0);

    public bool HasVpnProtocol => CurrentConnectionDetails?.Protocol != null;

    public string VpnProtocol => CurrentConnectionDetails is null
        ? string.Empty
        : Localizer.GetVpnProtocol(CurrentConnectionDetails.Protocol);

    public ConnectionDetailsPageViewModel(
        IConnectionManager connectionManager,
        VpnSpeedViewModel vpnSpeedViewModel,
        IDetailsViewNavigator viewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator, localizer, logger, issueReporter)
    {
        _connectionManager = connectionManager;
        _vpnSpeedViewModel = vpnSpeedViewModel;

        _refreshTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(REFRESH_TIMER_INTERVAL_IN_MS)
        };
        _refreshTimer.Tick += OnRefreshTimerTick;
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            CurrentConnectionDetails = _connectionManager.IsConnected
                ? _connectionManager.CurrentConnectionDetails
                : null;

            InvalidateSessionLength();
            InvalidateAutoRefreshTimer();
        });
    }

    public void Receive(ConnectionDetailsChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            ServerIpAddress = message.ServerIpAddress;
        });
    }

    protected override void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(FormattedSessionLength));
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
        if (_connectionManager.IsConnected)
        {
            if (!_refreshTimer.IsEnabled)
            {
                Refresh();
                _refreshTimer.Start();
            }
        }
        else
        {
            if (_refreshTimer.IsEnabled)
            {
                _refreshTimer.Stop();
            }
        }
    }

    private void Refresh()
    {
        InvalidateSessionLength();

        _vpnSpeedViewModel.RefreshAsync(IsActive && _isMainWindowVisible);
    }

    private void OnRefreshTimerTick(object? sender, EventArgs e)
    {
        Refresh();
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        _isMainWindowVisible = message.IsMainWindowVisible;
    }
}