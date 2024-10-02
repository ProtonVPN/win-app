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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.Models;

namespace ProtonVPN.Client.Legacy.UI.Settings.Pages.DefaultConnections;

public partial class ProfileDefaultConnectionObservable : ObservableObject
{
    private readonly DefaultConnectionViewModel _parentViewModel;

    public ILocalizationProvider Localizer { get; }
    public IConnectionProfile Profile { get; }

    public string? ExitCountryCode => Profile.Location.GetCountryCode();
    public string Header => Profile.Name;
    public string SubHeader => Localizer.GetConnectionProfileSubtitle(Profile);
    public bool IsSecureCore => Profile?.Feature is SecureCoreFeatureIntent;
    public bool IsTor => Profile?.Feature is TorFeatureIntent;
    public bool IsP2P => Profile?.Feature is P2PFeatureIntent;

    public FlagType FlagType => Profile.GetFlagType();

    public bool HasFeature => IsTor || IsP2P || IsSecureCore;

    [ObservableProperty]
    private bool _isDefaultConnection;

    public ProfileDefaultConnectionObservable(ILocalizationProvider localizer,
        DefaultConnection defaultConnection,
        DefaultConnectionViewModel parentViewModel,
        IConnectionProfile profile)
    {
        Localizer = localizer;
        _parentViewModel = parentViewModel;

        Profile = profile;
        OnDefaultConnectionChange(defaultConnection);
    }

    public void OnDefaultConnectionChange(DefaultConnection defaultConnection)
    {
        IsDefaultConnection = defaultConnection.ProfileId == Profile.Id;
    }

    partial void OnIsDefaultConnectionChanged(bool value)
    {
        if (value)
        {
            _parentViewModel.SetProfileAsDefaultConnection(Profile);
        }
    }
}