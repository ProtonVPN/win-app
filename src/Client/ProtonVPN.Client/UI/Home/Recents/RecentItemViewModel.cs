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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Client.UI.Home.Recents;

public partial class RecentItemViewModel : ViewModelBase
{
    private readonly IConnectionManager _connectionManager;
    private readonly IRecentConnectionsProvider _recentConnectionsProvider;

    private readonly IRecentConnection _recentConnection;

    public bool IsPinned => _recentConnection.IsPinned;

    public bool IsActiveConnection => _recentConnection.IsActiveConnection;

    public bool IsServerUnderMaintenance => _recentConnection.IsServerUnderMaintenance;

    public string? ExitCountry => (_recentConnection.ConnectionIntent?.Location as CountryLocationIntent)?.CountryCode;

    public string? EntryCountry => (_recentConnection.ConnectionIntent?.Feature as SecureCoreFeatureIntent)?.EntryCountryCode;

    public bool IsSecureCore => _recentConnection.ConnectionIntent?.Feature is SecureCoreFeatureIntent;

    public bool IsTor => _recentConnection.ConnectionIntent?.Feature is TorFeatureIntent;

    public bool IsP2P => _recentConnection.ConnectionIntent?.Feature is P2PFeatureIntent;

    public bool HasFeature => IsTor || IsP2P;

    public string Title => Localizer.GetConnectionIntentTitle(_recentConnection.ConnectionIntent);

    public string Subtitle => Localizer.GetConnectionIntentSubtitle(_recentConnection.ConnectionIntent).FormatIfNotEmpty("- {0}");

    public string PrimaryCommandText => IsActiveConnection
        ? Localizer.Get("Common_Actions_Disconnect")
        : Localizer.Get("Common_Actions_Connect");

    public string SecondaryCommandToolTip => IsServerUnderMaintenance
        ? Localizer.Get("Home_Recents_ServerUnderMaintenance")
        : Localizer.Get("Home_Recents_SecondaryActions_ToolTip");

    public RecentItemViewModel(IConnectionManager connectionManager,
        IRecentConnectionsProvider recentConnectionsProvider,
        IRecentConnection recentConnection,
        ILocalizationProvider localizationProvider)
        : base(localizationProvider)
    {
        ArgumentNullException.ThrowIfNull(recentConnection, nameof(recentConnection));
        ArgumentNullException.ThrowIfNull(recentConnection.ConnectionIntent, nameof(recentConnection.ConnectionIntent));

        _connectionManager = connectionManager;
        _recentConnectionsProvider = recentConnectionsProvider;

        _recentConnection = recentConnection;
    }

    protected override void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subtitle));
        OnPropertyChanged(nameof(PrimaryCommandText));
        OnPropertyChanged(nameof(SecondaryCommandToolTip));
    }

    [RelayCommand(CanExecute = nameof(CanToggleConnection))]
    private async Task ToggleConnectionAsync()
    {
        if (IsActiveConnection)
        {
            await _connectionManager.DisconnectAsync();
            return;
        }

        await _connectionManager.ConnectAsync(_recentConnection.ConnectionIntent);
    }

    private bool CanToggleConnection()
    {
        return !IsServerUnderMaintenance;
    }

    [RelayCommand(CanExecute = nameof(CanPin))]
    private void Pin()
    {
        _recentConnectionsProvider.Pin(_recentConnection);
    }

    private bool CanPin()
    {
        return !IsPinned;
    }

    [RelayCommand(CanExecute = nameof(CanUnpin))]
    private void Unpin()
    {
        _recentConnectionsProvider.Unpin(_recentConnection);
    }

    private bool CanUnpin()
    {
        return IsPinned;
    }

    [RelayCommand]
    private void Remove()
    {
        _recentConnectionsProvider.Remove(_recentConnection);
    }
}