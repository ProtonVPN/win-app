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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
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
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Activation.Custom;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Connections.Profiles;

namespace ProtonVPN.Client.UI.Connections.Common.Items;

public partial class ProfileItem : ConnectionItemBase
{
    private readonly IOverlayActivator _overlayActivator;
    private readonly IProfilesManager _profilesManager;
    private readonly IProfileEditor _profileEditor;
    private readonly ISettings _settings;

    public IConnectionProfile Profile { get; }

    public string? ExitCountryCode => (Profile.Location as CountryLocationIntent)?.CountryCode;

    public string SubHeader => Localizer.GetConnectionProfileSubtitle(Profile);

    public override string? ToolTip => IsRestricted
        ? Localizer.Get("Connections_Profiles_NotAvailable_Tooltip")
        : null;

    public override string SecondaryCommandAutomationId => $"Actions_for_{AutomationName}";

    public override ModalSources UpsellModalSource => ModalSources.Profiles;

    public bool IsTor => Profile.Feature is TorFeatureIntent;

    public bool IsP2P => Profile.Feature is P2PFeatureIntent;

    public bool IsSecureCore => Profile.Feature is SecureCoreFeatureIntent;

    public FlagType FlagType => Profile.GetFlagType();

    public bool HasFeature => IsTor || IsP2P || IsSecureCore;

    public ProfileItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator,
        IOverlayActivator overlayActivator,
        IProfilesManager profilesManager,
        IProfileEditor profileEditor,
        ISettings settings,
        IConnectionProfile profile)
        : base(localizer,
               serversLoader,
               connectionManager,
               mainViewNavigator,
               upsellCarouselActivator,
               profile.Name)
    {
        _overlayActivator = overlayActivator;
        _profilesManager = profilesManager;
        _profileEditor = profileEditor;
        _settings = settings;

        Profile = profile;
    }

    public override IConnectionIntent GetConnectionIntent()
    {
        return Profile;
    }

    public override void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        IsActiveConnection = currentConnectionDetails?.OriginalConnectionIntent is IConnectionProfile profile
                          && profile.Id == Profile.Id;

        EditProfileCommand.NotifyCanExecuteChanged();
        DeleteProfileCommand.NotifyCanExecuteChanged();
    }

    public override void InvalidateIsUnderMaintenance()
    {
        List<Server> servers = ServersLoader.GetServers().ToList();

        IsUnderMaintenance = Profile.HasNoServers(servers, _settings.DeviceLocation)
                          || Profile.AreAllServersUnderMaintenance(servers, _settings.DeviceLocation);
    }

    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private async Task EditProfileAsync()
    {
        await _profileEditor.EditProfileAsync(Profile);
    }

    private bool CanEditProfile()
    {
        return !IsActiveConnection;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteProfile))]
    private async Task DeleteProfileAsync()
    {
        ContentDialogResult result = await _overlayActivator.ShowMessageAsync(
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

    public override void InvalidateIsRestricted(bool isPaidUser)
    {
        base.InvalidateIsRestricted(isPaidUser);
        OnPropertyChanged(nameof(ToolTip));
    }
}