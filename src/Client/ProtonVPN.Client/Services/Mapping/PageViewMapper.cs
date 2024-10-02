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
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Mapping;
using ProtonVPN.Client.Services.Mapping.Bases;
using ProtonVPN.Client.TEMP;
using ProtonVPN.Client.UI.Dialogs.ReportIssue.Pages;
using ProtonVPN.Client.UI.Login;
using ProtonVPN.Client.UI.Login.Pages;
using ProtonVPN.Client.UI.Main;
using ProtonVPN.Client.UI.Main.Features.KillSwitch;
using ProtonVPN.Client.UI.Main.Features.NetShield;
using ProtonVPN.Client.UI.Main.Features.PortForwarding;
using ProtonVPN.Client.UI.Main.Features.SplitTunneling;
using ProtonVPN.Client.UI.Main.Home;
using ProtonVPN.Client.UI.Main.Home.Details.Connection;
using ProtonVPN.Client.UI.Main.Home.Details.Location;
using ProtonVPN.Client.UI.Main.Settings;
using ProtonVPN.Client.UI.Main.Settings.Pages;
using ProtonVPN.Client.UI.Main.Settings.Pages.About;
using ProtonVPN.Client.UI.Main.Settings.Pages.Advanced;
using ProtonVPN.Client.UI.Main.Settings.Pages.DefaultConnections;
using ProtonVPN.Client.UI.Main.Sidebar.Connections;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Countries;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Gateways;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Recents;
using ProtonVPN.Client.UI.Main.Sidebar.Search;

namespace ProtonVPN.Client.Services.Mapping;

public class PageViewMapper : ViewMapperBase<PageViewModelBase, Page>, IPageViewMapper
{
    protected override void ConfigureMappings()
    {
        ConfigureMapping<GalleryPageViewModel, GalleryPageView>(); // TEMP

        ConfigureMapping<LoginPageViewModel, LoginPageView>();
        ConfigureMapping<SignInPageViewModel, SignInPageView>();
        ConfigureMapping<TwoFactorPageViewModel, TwoFactorPageView>();
        ConfigureMapping<LoadingPageViewModel, LoadingPageView>();

        ConfigureMapping<MainPageViewModel, MainPageView>();
        ConfigureMapping<SettingsPageViewModel, SettingsPageView>();
        ConfigureMapping<CommonSettingsPageViewModel, CommonSettingsPageView>();
        ConfigureMapping<AdvancedSettingsPageViewModel, AdvancedSettingsPageView>();
        ConfigureMapping<ProtocolSettingsPageViewModel, ProtocolSettingsPageView>();
        ConfigureMapping<DefaultConnectionSettingsPageViewModel, DefaultConnectionPageView>();
        ConfigureMapping<VpnAcceleratorSettingsPageViewModel, VpnAcceleratorSettingsPageView>();
        ConfigureMapping<CustomDnsServersViewModel, CustomDnsServersPageView>();
        ConfigureMapping<AutoStartupSettingsPageViewModel, AutoStartupSettingsPageView>();
        ConfigureMapping<DebugLogsPageViewModel, DebugLogsPageView>();
        ConfigureMapping<DeveloperToolsPageViewModel, DeveloperToolsPageView>();
        ConfigureMapping<ConnectionsPageViewModel, ConnectionsPageView>();
        ConfigureMapping<CensorshipSettingsPageViewModel, CensorshipSettingsPageView>();
        ConfigureMapping<AboutPageViewModel, AboutPageView>();
        ConfigureMapping<LicensingViewModel, LicensingPageView>();
        ConfigureMapping<RecentsPageViewModel, RecentsPageView>();
        ConfigureMapping<ProfilesPageViewModel, ProfilesPageView>();
        ConfigureMapping<GatewaysPageViewModel, GatewaysPageView>();
        ConfigureMapping<CountriesPageViewModel, CountriesPageView>();
        ConfigureMapping<SearchResultsPageViewModel, SearchResultsPageView>();
        ConfigureMapping<HomePageViewModel, HomePageView>();
        ConfigureMapping<ConnectionDetailsPageViewModel, ConnectionDetailsPageView>();
        ConfigureMapping<LocationDetailsPageViewModel, LocationDetailsPageView>();
        ConfigureMapping<KillSwitchPageViewModel, KillSwitchPageView>();
        ConfigureMapping<NetShieldPageViewModel, NetShieldPageView>();
        ConfigureMapping<PortForwardingPageViewModel, PortForwardingPageView>();
        ConfigureMapping<SplitTunnelingPageViewModel, SplitTunnelingPageView>();

        ConfigureMapping<ReportIssueCategoriesPageViewModel, ReportIssueCategoriesPageView>();
        ConfigureMapping<ReportIssueCategoryPageViewModel, ReportIssueCategoryPageView>();
        ConfigureMapping<ReportIssueContactPageViewModel, ReportIssueContactPageView>();
        ConfigureMapping<ReportIssueResultPageViewModel, ReportIssueResultPageView>();
    }
}