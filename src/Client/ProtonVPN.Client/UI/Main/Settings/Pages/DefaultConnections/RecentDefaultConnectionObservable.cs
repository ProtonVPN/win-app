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
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.DefaultConnections;

public partial class RecentDefaultConnectionObservable : ObservableObject
{
    private readonly DefaultConnectionSettingsPageViewModel _parentViewModel;

    public ILocalizationProvider Localizer { get; }
    public IRecentConnection Recent { get; }

    public string? ExitCountryCode => Recent.ConnectionIntent.Location.GetCountryCode();

    public string? EntryCountryCode => Recent.ConnectionIntent?.Feature is SecureCoreFeatureIntent secureCoreIntent && !secureCoreIntent.IsFastest
        ? secureCoreIntent.EntryCountryCode
        : string.Empty;

    public string Header => Profile is not null
        ? Profile!.Name
        : Localizer.GetConnectionIntentTitle(Recent.ConnectionIntent);

    public string SubHeader => Profile is not null
        ? Localizer.GetConnectionProfileSubtitle(Profile)
        : Localizer.GetConnectionIntentSubtitle(Recent.ConnectionIntent);

    public bool IsSecureCore => Recent?.ConnectionIntent.Feature is SecureCoreFeatureIntent;
    public bool IsTor => Recent?.ConnectionIntent.Feature is TorFeatureIntent;
    public bool IsP2P => Recent?.ConnectionIntent.Feature is P2PFeatureIntent;

    public FlagType FlagType => Recent.ConnectionIntent.GetFlagType();

    public IConnectionProfile? Profile {  get; }

    [ObservableProperty]
    private bool _isDefaultConnection;

    public RecentDefaultConnectionObservable(ILocalizationProvider localizer,
        DefaultConnection defaultConnection,
        DefaultConnectionSettingsPageViewModel parentViewModel,
        IRecentConnection recent)
    {
        Localizer = localizer;
        _parentViewModel = parentViewModel;

        Recent = recent;

        if (recent.ConnectionIntent is IConnectionProfile profile)
        {
            Profile = profile;
        }

        OnDefaultConnectionChange(defaultConnection);
    }

    public void OnDefaultConnectionChange(DefaultConnection defaultConnection)
    {
        IsDefaultConnection = defaultConnection.RecentId == Recent.Id;
    }

    partial void OnIsDefaultConnectionChanged(bool value)
    {
        if (value)
        {
            _parentViewModel.SetRecentAsDefaultConnection(Recent);
        }
    }
}