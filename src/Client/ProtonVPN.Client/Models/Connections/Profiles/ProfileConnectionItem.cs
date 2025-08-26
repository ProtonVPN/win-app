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

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;
using ProtonVPN.Client.Contracts.Enums;

namespace ProtonVPN.Client.Models.Connections.Profiles;

public partial class ProfileConnectionItem : ConnectionItemBase
{
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IProfilesManager _profilesManager;
    private readonly IProfileEditor _profileEditor;

    public IConnectionProfile Profile { get; }

    public override ConnectionGroupType GroupType => ConnectionGroupType.Profiles;

    public override string Header => Profile.Name;

    public override string Description => Localizer.GetConnectionProfileSubtitle(Profile);

    public override string? ToolTip => IsRestricted
        ? Localizer.Get("Connections_Profiles_NotAvailable_Tooltip")
        : null;

    public override string SecondaryCommandAutomationId => $"Actions_for_{AutomationName}";

    public override VpnTriggerDimension VpnTriggerDimension { get; } = VpnTriggerDimension.Profile;

    public string? ExitCountryCode => (Profile.Location as CountryLocationIntent)?.CountryCode;

    public bool IsTor => Profile.Feature is TorFeatureIntent;

    public bool IsP2P => Profile.Feature is P2PFeatureIntent;

    public bool IsSecureCore => Profile.Feature is SecureCoreFeatureIntent;

    public FlagType FlagType => Profile.GetFlagType();

    public override object FirstSortProperty => Profile.CreationDateTimeUtc;

    public override object SecondSortProperty => Header;

    public ProfileConnectionItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IProfilesManager profilesManager,
        IProfileEditor profileEditor,
        IConnectionProfile profile)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator,
               false)
    {
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _profilesManager = profilesManager;
        _profileEditor = profileEditor;

        Profile = profile;
    }

    public override IConnectionIntent GetConnectionIntent()
    {
        return Profile;
    }

    public override void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        base.InvalidateIsActiveConnection(currentConnectionDetails);

        DeleteProfileCommand.NotifyCanExecuteChanged();
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return Profile.IsSameAs(currentConnectionDetails?.OriginalConnectionIntent);
    }

    [RelayCommand]
    private Task EditProfileAsync()
    {
        return _profileEditor.EditProfileAsync(Profile);
    }

    [RelayCommand]
    private Task DuplicateProfileAsync()
    {
        return _profileEditor.DuplicateProfileAsync(Profile);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteProfile))]
    private async Task DeleteProfileAsync()
    {
        ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = Localizer.Get("Connections_Profiles_Delete_Title"),
                Message = Localizer.GetFormat("Connections_Profiles_Delete_Message", Profile.Name),
                PrimaryButtonText = Localizer.Get("Connections_Profiles_Delete"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
            });

        if (result == ContentDialogResult.Primary)
        {
            _profilesManager.DeleteProfile(Profile.Id);
        }
    }

    private bool CanDeleteProfile()
    {
        return !IsActiveConnection;
    }

    public void InvalidateIsUnderMaintenance(IEnumerable<Server> servers, DeviceLocation? deviceLocation)
    {
        IsUnderMaintenance = GetConnectionIntent().AreAllServersUnderMaintenance(servers, deviceLocation);
    }
}