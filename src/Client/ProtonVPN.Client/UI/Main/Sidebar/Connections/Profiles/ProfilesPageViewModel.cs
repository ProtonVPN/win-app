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

using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Models;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles;

public class ProfilesPageViewModel : ConnectionPageViewModelBase
{
    public override string Header => "Profiles"; // Localizer.Get("Profiles_Page_Title");

    public override IconElement Icon => new WindowTerminal();

    public override bool IsAvailable => Profiles.Count > 0;

    public ObservableCollection<string> Profiles { get; } = [];

    public override int SortIndex { get; } = 4;

    public ProfilesPageViewModel(
        IConnectionsViewNavigator parentViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IConnectionGroupFactory connectionGroupFactory)
        : base(parentViewNavigator, localizer, logger, issueReporter, settings, serversLoader, connectionManager, connectionGroupFactory)
    { }

    protected override void OnNavigation(NavigationEventArgs e)
    {
        base.OnNavigation(e);

        // Test to confirm that page can appear and disappear dynamically
        if (e.SourcePageType.Name.Contains("Gateway"))
        {
            Profiles.Add("hello");
            OnPropertyChanged(nameof(IsAvailable));
        }
    }

    protected override IEnumerable<ConnectionItemBase> GetItems()
    {
        return [];
    }
}