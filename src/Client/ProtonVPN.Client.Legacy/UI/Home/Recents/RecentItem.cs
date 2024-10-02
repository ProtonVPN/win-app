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
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Legacy.UI.Connections.Profiles;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Legacy.UI.Home.Recents;

public partial class RecentItem : ObservableObject
{
    private readonly IConnectionManager _connectionManager;
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly IRecentConnection _recentConnection;
    private readonly IProfilesManager _profilesManager;

    public ILocalizationProvider Localizer { get; }

    public bool IsPinned => _recentConnection.IsPinned;
    public bool IsActiveConnection => _recentConnection.IsActiveConnection;
    public bool IsServerAvailable => !_recentConnection.IsServerUnderMaintenance;
    public string? ExitCountry => _recentConnection.ConnectionIntent?.Location?.GetCountryCode();
    public string? EntryCountry => (_recentConnection.ConnectionIntent?.Feature as SecureCoreFeatureIntent)?.EntryCountryCode;
    public IConnectionProfile? Profile => _recentConnection.ConnectionIntent as IConnectionProfile;
    public bool IsProfileIntent => Profile != null;
    public bool IsSecureCore => _recentConnection.ConnectionIntent?.Feature is SecureCoreFeatureIntent;
    public bool IsTor => _recentConnection.ConnectionIntent?.Feature is TorFeatureIntent;
    public bool IsP2P => _recentConnection.ConnectionIntent?.Feature is P2PFeatureIntent;
    public bool IsB2B => _recentConnection.ConnectionIntent?.Feature is B2BFeatureIntent;
    public FlagType FlagType => _recentConnection.ConnectionIntent.GetFlagType();
    public bool HasFeature => IsTor || IsP2P;

    public string Title => IsProfileIntent
        ? Profile!.Name
        : Localizer.GetConnectionIntentTitle(_recentConnection.ConnectionIntent);

    public string Subtitle => (IsProfileIntent
        ? Localizer.GetConnectionProfileSubtitle(Profile)
        : Localizer.GetConnectionIntentSubtitle(_recentConnection.ConnectionIntent)).FormatIfNotEmpty(" -  {0}");

    public string PrimaryCommandText => IsActiveConnection
        ? Localizer.Get("Common_Actions_Disconnect")
        : Localizer.Get("Common_Actions_Connect");

    public string SecondaryCommandToolTip => IsServerAvailable
        ? Localizer.Get("Home_Recents_SecondaryActions_ToolTip")
        : Localizer.Get("Home_Recents_ServerUnderMaintenance");

    public string FullTitle => $"{Title}{(string.IsNullOrEmpty(Subtitle) ? string.Empty : Subtitle)}";

    public string PrimaryCommandDescription => IsActiveConnection
        ? Localizer.GetFormat("Common_Actions_DisconnectFrom", FullTitle)
        : Localizer.GetFormat("Common_Actions_ConnectTo", FullTitle);

    public string SecondaryCommandDescription => Localizer.GetFormat("Common_Actions_ActionsFor", FullTitle);

    public RecentItem(
        IConnectionManager connectionManager,
        IRecentConnectionsManager recentConnectionsManager,
        IRecentConnection recentConnection,
        ILocalizationProvider localizer,
        IProfilesManager profilesManager)
    {
        ArgumentNullException.ThrowIfNull(recentConnection, nameof(recentConnection));
        ArgumentNullException.ThrowIfNull(recentConnection.ConnectionIntent, nameof(recentConnection.ConnectionIntent));

        _connectionManager = connectionManager;
        _recentConnectionsManager = recentConnectionsManager;
        _recentConnection = recentConnection;
        _profilesManager = profilesManager;

        Localizer = localizer;
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
        return IsActiveConnection || IsServerAvailable;
    }

    [RelayCommand(CanExecute = nameof(CanPin))]
    private void Pin()
    {
        _recentConnectionsManager.Pin(_recentConnection);
    }

    private bool CanPin()
    {
        return !IsPinned;
    }

    [RelayCommand(CanExecute = nameof(CanUnpin))]
    private void Unpin()
    {
        _recentConnectionsManager.Unpin(_recentConnection);
    }

    private bool CanUnpin()
    {
        return IsPinned;
    }

    [RelayCommand]
    private void Remove()
    {
        _recentConnectionsManager.Remove(_recentConnection);
    }
}