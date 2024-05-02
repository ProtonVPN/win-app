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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Connections.Tor;
using ProtonVPN.Client.UI.Sidebar.Bases;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Sidebar;

public class SidebarTorCountriesViewModel : SidebarNavigationItemViewModelBase<TorCountriesPageViewModel>
{
    public override IconElement? Icon { get; } = new BrandTor();

    public override string Header => Localizer.Get("Countries_Tor");

    public override string AutomationId => "Sidebar_Countries_Tor";

    public override bool RequiresPaidAccess => true;

    public SidebarTorCountriesViewModel(
        IMainViewNavigator mainViewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings)
        : base(mainViewNavigator, localizationProvider, logger, issueReporter, settings)
    { }
}