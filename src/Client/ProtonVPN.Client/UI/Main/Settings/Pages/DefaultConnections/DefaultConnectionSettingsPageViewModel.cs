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
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.UI.Settings.Pages.Entities;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.DefaultConnections;

public partial class DefaultConnectionSettingsPageViewModel : SettingsPageViewModelBase,
    IEventMessageReceiver<ProfilesChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>
{
    private readonly IProfilesManager _profilesManager;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.DefaultConnection))]
    [NotifyPropertyChangedFor(nameof(IsFastestConnection))]
    [NotifyPropertyChangedFor(nameof(IsLastConnection))]
    private DefaultConnection _currentDefaultConnection;

    public override string Title => Localizer.Get("Settings_Connection_Default");

    public bool IsFastestConnection
    {
        get => IsDefaultConnectionType(DefaultConnectionType.Fastest);
        set => SetDefaultConnectionType(value, DefaultConnectionType.Fastest);
    }

    public bool IsLastConnection
    {
        get => IsDefaultConnectionType(DefaultConnectionType.Last);
        set => SetDefaultConnectionType(value, DefaultConnectionType.Last);
    }

    public SmartObservableCollection<ProfileDefaultConnectionObservable> Profiles { get; } = [];

    public bool HasProfiles => Profiles.Any();

    public DefaultConnectionSettingsPageViewModel(
        IProfilesManager profilesManager,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager)
        : base(
            requiredReconnectionSettings,
            mainViewNavigator,
            settingsViewNavigator,
            localizer,
            logger,
            issueReporter,
            mainWindowOverlayActivator,
            settings,
            settingsConflictResolver,
            connectionManager)
    {
        _profilesManager = profilesManager;
        InvalidateProfiles();
    }

    protected override void OnSaveSettings()
    {
        Settings.DefaultConnection = CurrentDefaultConnection;
    }

    protected override void OnRetrieveSettings()
    {
        CurrentDefaultConnection = Settings.DefaultConnection;
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        yield return new(nameof(ISettings.DefaultConnection), CurrentDefaultConnection, Settings.DefaultConnection != CurrentDefaultConnection);
    }

    private bool IsDefaultConnectionType(DefaultConnectionType defaultConnectionType)
    {
        return CurrentDefaultConnection.Type == defaultConnectionType;
    }

    private void SetDefaultConnectionType(bool value, DefaultConnectionType defaultConnectionType, Guid? profileId = null)
    {
        if (value)
        {
            CurrentDefaultConnection = defaultConnectionType switch
            {
                DefaultConnectionType.Fastest => DefaultConnection.Fastest,
                DefaultConnectionType.Last => DefaultConnection.Last,
                DefaultConnectionType.Profile when profileId.HasValue => new DefaultConnection(profileId.Value),
                _ => DefaultSettings.DefaultConnection
            };
        }
    }

    public void Receive(ProfilesChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateProfiles);
    }

    private void InvalidateProfiles()
    {
        List<IConnectionProfile> profiles = _profilesManager.GetAll().ToList();
        DefaultConnection defaultConnection = Settings.DefaultConnection;

        Profiles.Reset(profiles.Select(p => new ProfileDefaultConnectionObservable(Localizer, defaultConnection, this, p)));

        OnPropertyChanged(nameof(HasProfiles));
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        base.OnSettingsChanged(propertyName);
        if (propertyName == nameof(ISettings.DefaultConnection))
        {
            InvalidateProfileDefaultConnection();
        }
    }

    private void InvalidateProfileDefaultConnection()
    {
        DefaultConnection defaultConnection = Settings.DefaultConnection;
        List<ProfileDefaultConnectionObservable> profiles = Profiles.ToList();
        foreach (ProfileDefaultConnectionObservable profile in profiles)
        {
            profile.OnDefaultConnectionChange(defaultConnection);
        }
    }

    public void SetProfileAsDefaultConnection(IConnectionProfile profile)
    {
        SetDefaultConnectionType(true, DefaultConnectionType.Profile, profile.Id);
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateProfiles);
    }
}