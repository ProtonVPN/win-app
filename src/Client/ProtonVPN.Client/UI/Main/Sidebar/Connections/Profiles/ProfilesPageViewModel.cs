﻿/*
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

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.Base;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Models.Connections.Profiles;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles;

public partial class ProfilesPageViewModel : ConnectionPageViewModelBase,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    private readonly IConnectionItemFactory _connectionItemFactory;
    private readonly IProfilesManager _profilesManager;
    private readonly IProfileEditor _profileEditor;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IWebAuthenticator _webAuthenticator;

    public override string Header => Localizer.Get("Profiles_Page_Title");

    public override IconElement Icon => new WindowTerminal() { Size = PathIconSize.Pixels16 };

    public override int SortIndex { get; } = 3;

    public bool IsUpsellBannerVisible => !Settings.VpnPlan.IsPaid;

    public override bool IsAvailable => ParentViewNavigator.CanNavigateToProfilesView();

    public ProfilesPageViewModel(
        IConnectionsViewNavigator parentViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IConnectionGroupFactory connectionGroupFactory,
        IConnectionItemFactory connectionItemFactory,
        IProfilesManager profilesManager,
        IProfileEditor profileEditor,
        IUrlsBrowser urlsBrowser,
        IWebAuthenticator webAuthenticator)
        : base(parentViewNavigator, localizer, logger, issueReporter, settings, serversLoader, connectionManager, connectionGroupFactory)
    {
        _connectionItemFactory = connectionItemFactory;
        _profilesManager = profilesManager;
        _profileEditor = profileEditor;
        _urlsBrowser = urlsBrowser;
        _webAuthenticator = webAuthenticator;
    }

    public void Receive(ProfilesChangedMessage message)
    {
        ExecuteOnUIThread(FetchItems);
    }

    protected override IEnumerable<ConnectionItemBase> GetItems()
    {
        return _profilesManager.GetAll()
                               .Select(_connectionItemFactory.GetProfile);
    }

    protected override void InvalidateMaintenanceStates()
    {
        if (IsActive)
        {
            IEnumerable<Server> servers = ServersLoader.GetServers();
            DeviceLocation? deviceLocation = Settings.DeviceLocation;

            foreach (ProfileConnectionItem item in Items.OfType<ProfileConnectionItem>())
            {
                item.InvalidateIsUnderMaintenance(servers, deviceLocation);
            }
        }
    }

    protected override void OnVpnPlanChanged(VpnPlan oldPlan, VpnPlan newPlan)
    {
        base.OnVpnPlanChanged(oldPlan, newPlan);

        OnPropertyChanged(nameof(IsUpsellBannerVisible));
    }

    protected override void OnLoggedIn()
    {
        base.OnLoggedIn();

        OnPropertyChanged(nameof(IsUpsellBannerVisible));
    }

    [RelayCommand]
    private Task CreateProfileAsync()
    {
        return _profileEditor.CreateProfileAsync();
    }

    [RelayCommand]
    private async Task UpgradeAsync()
    {
        _urlsBrowser.BrowseTo(await _webAuthenticator.GetUpgradeAccountUrlAsync(ModalSource.Profiles));
    }
}