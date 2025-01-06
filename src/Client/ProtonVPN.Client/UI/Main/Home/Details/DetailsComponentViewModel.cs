/*
 * Copyright (c) 2024 Proton AG
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
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Home.Details;

public partial class DetailsComponentViewModel : HostViewModelBase<IDetailsViewNavigator>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private const int REFRESH_TIMER_INTERVAL_IN_MS = 1000;

    private readonly DispatcherTimer _refreshTimer;

    private readonly IConnectionManager _connectionManager;
    private readonly IMainWindowActivator _mainWindowActivator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProtectionDescription))]
    private TimeSpan? _sessionLength;

    public bool IsConnected => _connectionManager.IsConnected;

    public bool IsConnecting => _connectionManager.IsConnecting;

    public bool IsDisconnected => _connectionManager.IsDisconnected;

    public string ProtectionTitle =>
        _connectionManager.ConnectionStatus switch
        {
            ConnectionStatus.Disconnected => Localizer.Get("Home_ConnectionDetails_Unprotected"),
            ConnectionStatus.Connecting => Localizer.Get("Home_ConnectionDetails_Connecting"),
            ConnectionStatus.Connected => Localizer.Get("Home_ConnectionDetails_Protected"),
            _ => string.Empty,
        };

    public string ProtectionDescription =>
        _connectionManager.ConnectionStatus switch
        {
            ConnectionStatus.Disconnected => Localizer.Get("Home_ConnectionDetails_UnprotectedSubLabel"),
            ConnectionStatus.Connecting => Localizer.Get("Home_ConnectionDetails_ConnectingSubLabel"),
            ConnectionStatus.Connected => Localizer.GetFormattedTime(SessionLength ?? TimeSpan.Zero) ?? string.Empty,
            _ => string.Empty,
        };

    public DetailsComponentViewModel(
        IDetailsViewNavigator childViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IConnectionManager connectionManager,
        IMainWindowActivator mainWindowActivator)
        : base(childViewNavigator, localizer, logger, issueReporter)
    {
        _connectionManager = connectionManager;
        _mainWindowActivator = mainWindowActivator;

        _refreshTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(REFRESH_TIMER_INTERVAL_IN_MS)
        };
        _refreshTimer.Tick += OnRefreshTimerTick;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateConnectionStatus);
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAutoRefreshTimer);
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateAutoRefreshTimer();
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        InvalidateAutoRefreshTimer();
    }

    private void InvalidateConnectionStatus()
    {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsConnecting));
        OnPropertyChanged(nameof(IsDisconnected));
        OnPropertyChanged(nameof(ProtectionTitle));
        OnPropertyChanged(nameof(ProtectionDescription));

        InvalidateAutoRefreshTimer();
    }

    private void InvalidateAutoRefreshTimer()
    {
        if (_connectionManager.IsConnected) // && IsActive && _mainWindowActivator.IsWindowVisible) 
        {
            if (!_refreshTimer.IsEnabled)
            {
                InvalidateSessionLength();
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

    private void OnRefreshTimerTick(object? sender, object e)
    {
        InvalidateSessionLength();
    }

    private void InvalidateSessionLength()
    {
        ConnectionDetails? connectionDetails = _connectionManager.CurrentConnectionDetails;
        SessionLength = connectionDetails is null
            ? null
            : DateTime.UtcNow - connectionDetails?.EstablishedConnectionTimeUtc;
    }
}