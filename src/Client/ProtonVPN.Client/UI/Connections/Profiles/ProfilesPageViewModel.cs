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
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.Connections.Common.Factories;
using ProtonVPN.Client.UI.Connections.Common.Items;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Connections.Profiles;

public partial class ProfilesPageViewModel : PageViewModelBase<IMainViewNavigator>,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;
    private readonly IProfilesManager _profilesManager;
    private readonly IProfileEditor _profileEditor;

    private readonly ProfileItemFactory _profileItemFactory;

    public SmartObservableCollection<ProfileItem> Profiles { get; } = [];

    public bool HasProfiles => Profiles.Any();

    public override string? Title => Localizer.Get("Profiles_Page_Title");

    public string Description => Localizer.Get("Profiles_Page_Description");

    public string Header => Localizer.GetPluralFormat("Connections_Profiles", Profiles.Count);

    public override bool IsBackEnabled => false;

    public ImageSource IllustrationSource { get; } = ResourceHelper.GetIllustration("ProfilesIllustrationSource");

    public ProfilesPageViewModel(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        ProfileItemFactory profileItemFactory,
        IConnectionManager connectionManager,
        ISettings settings,
        IProfilesManager profilesManager,
        IProfileEditor profileEditor)
        : base(viewNavigator,
               localizationProvider,
               logger,
               issueReporter)
    {
        _profileItemFactory = profileItemFactory;
        _connectionManager = connectionManager;
        _settings = settings;
        _profilesManager = profilesManager;
        _profileEditor = profileEditor;

        FetchProfiles();
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(InvalidateActiveConnection);
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateUnderMaintenance);
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateRestrictions);
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateRestrictions);
    }

    public void Receive(ProfilesChangedMessage message)
    {
        ExecuteOnUIThread(FetchProfiles);
    }

    protected override void OnActivated()
    {
        InvalidateActiveConnection();
        InvalidateRestrictions();
        InvalidateUnderMaintenance();
    }

    protected void FetchProfiles()
    {
        Profiles.Reset(
            _profilesManager.GetAll()
                .Select(_profileItemFactory.GetProfile));

        InvalidateActiveConnection();
        InvalidateRestrictions();
        InvalidateUnderMaintenance();

        OnPropertyChanged(nameof(HasProfiles));
        OnPropertyChanged(nameof(Header));
    }

    protected void InvalidateActiveConnection()
    {
        if (IsActive)
        {
            ConnectionDetails? connectionDetails = _connectionManager.CurrentConnectionDetails;

            foreach (ProfileItem profile in Profiles)
            {
                profile.InvalidateIsActiveConnection(connectionDetails);
            }
        }
    }

    protected void InvalidateUnderMaintenance()
    {
        if (IsActive)
        {
            foreach (ProfileItem profile in Profiles)
            {
                profile.InvalidateIsUnderMaintenance();
            }
        }
    }

    protected void InvalidateRestrictions()
    {
        if (IsActive)
        {
            bool isPaidUser = _settings.VpnPlan.IsPaid;

            foreach (ProfileItem profile in Profiles)
            {
                profile.InvalidateIsRestricted(isPaidUser);
            }
        }
    }

    [RelayCommand]
    private async Task CreateProfileAsync()
    {
        await _profileEditor.CreateProfileAsync();
    }
}