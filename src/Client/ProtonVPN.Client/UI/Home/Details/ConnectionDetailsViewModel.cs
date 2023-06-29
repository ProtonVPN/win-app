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
using CommunityToolkit.Mvvm.Messaging;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.UI.Home.Details;

public partial class ConnectionDetailsViewModel : ActivatableViewModelBase, IRecipient<ConnectionStatusChanged>
{
    private const int REFRESH_TIMER_INTERVAL_IN_MS = 1000;

    private readonly IConnectionManager _connectionManager;
    private readonly IDialogActivator _dialogActivator;

    private readonly VpnSpeedViewModel _vpnSpeedViewModel;

    private readonly DispatcherTimer _refreshTimer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SessionLength))]
    [NotifyPropertyChangedFor(nameof(FormattedSessionLength))]
    [NotifyPropertyChangedFor(nameof(Country))]
    [NotifyPropertyChangedFor(nameof(ServerLoad))]
    [NotifyPropertyChangedFor(nameof(FormattedServerLoad))]
    [NotifyPropertyChangedFor(nameof(ServerLatency))]
    private ConnectionDetails? _currentConnectionDetails;

    public bool IsConnecting => _connectionManager.ConnectionStatus == ConnectionStatus.Connecting;

    public TimeSpan? SessionLength => CurrentConnectionDetails != null
        ? DateTime.UtcNow - CurrentConnectionDetails.EstablishedConnectionTime
        : null;

    public string? FormattedSessionLength => SessionLength != null
        ? Localizer.GetFormattedTime(SessionLength.Value)
        : null;

    public string? Country => !string.IsNullOrEmpty(CurrentConnectionDetails?.CountryCode)
        ? Localizer.GetCountryName(CurrentConnectionDetails.CountryCode)
        : null;

    public double ServerLoad => CurrentConnectionDetails?.ServerLoad ?? 0;

    public string FormattedServerLoad => $"{ServerLoad:P0}";

    public string? ServerLatency => CurrentConnectionDetails?.ServerLatency != null
        ? Localizer.GetFormat("Format_Milliseconds", CurrentConnectionDetails.ServerLatency.Value.TotalMilliseconds)
        : null;

    public ConnectionDetailsViewModel(ILocalizationProvider localizationProvider,
        IConnectionManager connectionManager,
        IDialogActivator dialogActivator,
        VpnSpeedViewModel vpnSpeedViewModel)
        : base(localizationProvider)
    {
        _connectionManager = connectionManager;
        _dialogActivator = dialogActivator;
        _vpnSpeedViewModel = vpnSpeedViewModel;

        _refreshTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(REFRESH_TIMER_INTERVAL_IN_MS)
        };
        _refreshTimer.Tick += OnRefreshTimerTick;
    }

    public void Receive(ConnectionStatusChanged message)
    {
        OnPropertyChanged(nameof(IsConnecting));
    }

    protected override void OnActivated()
    {
        CurrentConnectionDetails = _connectionManager.ConnectionStatus == ConnectionStatus.Connected
            ? _connectionManager.GetConnectionDetails()
            : null;

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
        OnPropertyChanged(nameof(ServerLatency));
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

        _vpnSpeedViewModel.Refresh();
    }

    private void OnRefreshTimerTick(object? sender, EventArgs e)
    {
        Refresh();
    }

    [RelayCommand]
    public async Task OpenOverlayAsync(string dialogKey)
    {
        await _dialogActivator.ShowAsync(dialogKey);
    }
}