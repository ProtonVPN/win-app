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
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles.Contracts;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles.Overlays;

public partial class EditProfileOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>, IProfileEditor
{
    private readonly IProfilesManager _profilesManager;
    private readonly IProfileIconSelector _profileIconSelector;
    private readonly IProfileSettingsSelector _profileSettingsSelector;
    private readonly IConnectionIntentSelector _connectionIntentSelector;
    private readonly IConfiguration _configuration;

    private IConnectionProfile _profile;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    private string _profileName = string.Empty;

    public int MaximumProfileNameLength => _configuration.MaximumProfileNameLength;

    public EditProfileOverlayViewModel(
        IMainWindowOverlayActivator overlayActivator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IProfilesManager profilesManager,
        IProfileIconSelector profileIconSelector,
        IProfileSettingsSelector profileSettingsSelector,
        IConnectionIntentSelector connectionIntentSelector,
        IConfiguration configuration)
        : base(overlayActivator,
               localizer,
               logger,
               issueReporter)
    {
        _profilesManager = profilesManager;
        _profileIconSelector = profileIconSelector;
        _profileSettingsSelector = profileSettingsSelector;
        _connectionIntentSelector = connectionIntentSelector;
        _configuration = configuration;

        _profile = ConnectionProfile.Default;
    }

    public Task EditProfileAsync(IConnectionProfile profile)
    {
        _profile = profile;

        ProfileName = _profile.Name;

        _profileIconSelector.SetProfileIcon(_profile.Category, _profile.Color);
        _profileSettingsSelector.SetProfileSettings(_profile.Settings);
        _connectionIntentSelector.SetConnectionIntent(_profile);

        return InvokeAsync();
    }

    public async Task CreateProfileAsync()
    {
        await EditProfileAsync(ConnectionProfile.Default);
    }

    [RelayCommand(CanExecute = nameof(CanSaveProfile))]
    private void SaveProfile()
    {
        _profile.Name = ProfileName;
        _profile.Category = _profileIconSelector.GetProfileCategory();
        _profile.Color = _profileIconSelector.GetProfileColor();
        _profile.Settings = _profileSettingsSelector.GetProfileSettings();

        IConnectionIntent connectionIntent = _connectionIntentSelector.GetConnectionIntent();
        _profile.UpdateIntent(connectionIntent.Location, connectionIntent.Feature);

        _profilesManager.AddOrEditProfile(_profile);

        Close();
    }

    private bool CanSaveProfile()
    {
        return !string.IsNullOrWhiteSpace(ProfileName);
    }
}