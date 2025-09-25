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

using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.UI.Main.Profiles.Contracts;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.UI.Main.Profiles;

public partial class ProfilePageViewModel : PageViewModelBase<IMainViewNavigator>, IProfileEditor, IProfilePropertiesSelector
{
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IConnectionManager _connectionManager;
    private readonly IProfilesManager _profilesManager;
    private readonly IProfileIconSelector _profileIconSelector;
    private readonly IProfileSettingsSelector _profileSettingsSelector;
    private readonly IProfileOptionsSelector _profileOptionsSelector;
    private readonly IConnectionIntentSelector _connectionIntentSelector;
    private readonly IConfiguration _configuration;
    private readonly IVpnServiceSettingsUpdater _vpnServiceSettingsUpdater;

    private IConnectionProfile _profile;

    private bool _isNewProfile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProfileNameValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    private string _profileName = string.Empty;

    public bool IsProfileNameValid => !string.IsNullOrWhiteSpace(ProfileName);

    public int MaximumProfileNameLength => _configuration.MaximumProfileNameLength;

    public string ApplyCommandText => Localizer.Get(IsReconnectionRequired()
        ? "Common_Actions_Reconnect"
        : "Common_Actions_Save");

    public ProfilePageViewModel(
        IMainViewNavigator parentViewNavigator,
        IViewModelHelper viewModelHelper,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IConnectionManager connectionManager,
        IProfilesManager profilesManager,
        IProfileIconSelector profileIconSelector,
        IProfileSettingsSelector profileSettingsSelector,
        IProfileOptionsSelector profileOptionsSelector,
        IConnectionIntentSelector connectionIntentSelector,
        IConfiguration configuration,
        IVpnServiceSettingsUpdater vpnServiceSettingsUpdater)
        : base(parentViewNavigator,
               viewModelHelper)
    {
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _connectionManager = connectionManager;
        _profilesManager = profilesManager;
        _profileIconSelector = profileIconSelector;
        _profileSettingsSelector = profileSettingsSelector;
        _profileOptionsSelector = profileOptionsSelector;
        _connectionIntentSelector = connectionIntentSelector;
        _configuration = configuration;
        _vpnServiceSettingsUpdater = vpnServiceSettingsUpdater;

        _profile = ConnectionProfile.Default;

        _profileIconSelector.PropertyChanged += OnSelectorPropertyChanged;
        _connectionIntentSelector.PropertyChanged += OnSelectorPropertyChanged;
        _profileSettingsSelector.PropertyChanged += OnSelectorPropertyChanged;
        _profileOptionsSelector.PropertyChanged += OnSelectorPropertyChanged;
    }

    public async Task CreateProfileAsync()
    {
        await EditProfileAsync(ConnectionProfile.Default);

        _isNewProfile = true;

        SaveProfileCommand.NotifyCanExecuteChanged();
    }

    public async Task EditProfileAsync(IConnectionProfile profile)
    {
        _profile = profile;
        _isNewProfile = false;

        ProfileName = _profile.Name;

        _profileIconSelector.SetProfileIcon(_profile.Icon);
        _profileSettingsSelector.SetProfileSettings(_profile.Settings);
        await _profileOptionsSelector.SetProfileOptionsAsync(_profile.Options);
        _connectionIntentSelector.SetConnectionIntent(_profile);

        await InvokeAsync();
    }

    public async Task DuplicateProfileAsync(IConnectionProfile profile)
    {
        string profileName = Localizer.GetFormat("Connections_Profiles_Name_Duplicate", profile.Name);

        await EditProfileAsync(profile.Duplicate(profileName));

        _isNewProfile = true;

        SaveProfileCommand.NotifyCanExecuteChanged();
    }

    public async Task<bool> TryRedirectToProfileAsync(string overriddenSettingsName, IConnectionProfile profile)
    {
        ContentDialogResult result = await _mainWindowOverlayActivator.ShowSettingsOverriddenByProfileOverlayAsync(overriddenSettingsName);
        if (result == ContentDialogResult.Primary)
        {
            await EditProfileAsync(profile);
            return true;
        }
        return false;
    }

    [RelayCommand]
    public async Task<bool> CloseAsync()
    {
        bool navigationCompleted =
            await ParentViewNavigator.NavigateToHomeViewAsync();

        if (navigationCompleted)
        {
            RequestResetContentScroll();
        }

        return navigationCompleted;
    }

    public override async Task<bool> CanNavigateFromAsync()
    {
        if (!CanSaveProfile()) // No changes made, simply leave page
        {
            return true;
        }

        ContentDialogResult result = await _mainWindowOverlayActivator.ShowDiscardConfirmationOverlayAsync();
        return result switch
        {
            // Do nothing, user decided to discard profile changes.
            ContentDialogResult.Primary => true,
            // Save profile and trigger reconnection if needed
            ContentDialogResult.Secondary => await SaveAsync(),
            // Cancel navigation, stays on current page without deleting changes user have made
            _ => false,
        };
    }

    public bool HasChanged()
    {
        return _isNewProfile
            || _profile.Name != ProfileName
            || _profileIconSelector.HasChanged()
            || _profileSettingsSelector.HasChanged()
            || _profileOptionsSelector.HasChanged()
            || _connectionIntentSelector.HasChanged();
    }

    public bool IsReconnectionRequired()
    {
        if (!IsConnectedToProfile())
        {
            return false;
        }

        return _profileIconSelector.IsReconnectionRequired()
            || _profileSettingsSelector.IsReconnectionRequired()
            || _profileOptionsSelector.IsReconnectionRequired()
            || _connectionIntentSelector.IsReconnectionRequired();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName != nameof(ApplyCommandText))
        {
            OnProfilePropertyChanged();
        }
    }

    private bool IsConnectedToProfile()
    {
        return !_connectionManager.IsDisconnected &&
               _connectionManager.CurrentConnectionIntent?.IsSameAs(_profile) == true;
    }

    [RelayCommand(CanExecute = nameof(CanSaveProfile))]
    private async Task<bool> SaveProfileAsync()
    {
        return await SaveAsync()
            && await CloseAsync();
    }

    private bool CanSaveProfile()
    {
        return HasChanged();
    }

    private async Task<bool> SaveAsync()
    {
        bool isReconnectionRequired = IsReconnectionRequired();
        bool haveSettingsChanged = _profileSettingsSelector.HasChanged();

        _profile.Name = IsProfileNameValid
            ? ProfileName.Trim()
            : Localizer.Get("Connections_Profiles_Name_Default");
        _profile.Icon = _profileIconSelector.GetProfileIcon();
        _profile.Settings = _profileSettingsSelector.GetProfileSettings();
        _profile.Options = _profileOptionsSelector.GetProfileOptions();

        IConnectionIntent connectionIntent = _connectionIntentSelector.GetConnectionIntent();
        _profile.UpdateIntent(connectionIntent.Location, connectionIntent.Feature);

        _profilesManager.AddOrEditProfile(_profile);

        await EditProfileAsync(_profile);

        if (isReconnectionRequired)
        {
            return await _connectionManager.ReconnectAsync(VpnTriggerDimension.NewConnection);
        }
        else if (IsConnectedToProfile() && haveSettingsChanged)
        {
            await _vpnServiceSettingsUpdater.SendAsync();
        }

        return true;
    }

    private void OnSelectorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnProfilePropertyChanged();
    }

    private void OnProfilePropertyChanged()
    {
        OnPropertyChanged(nameof(ApplyCommandText));

        SaveProfileCommand.NotifyCanExecuteChanged();
    }
}