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

using Autofac;
using Autofac.Builder;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.UI;
using ProtonVPN.Client.UI.Account;
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.Client.UI.Countries.Controls;
using ProtonVPN.Client.UI.Countries.CountriesFeatureTabs;
using ProtonVPN.Client.UI.Countries.CountryFeatureTabs;
using ProtonVPN.Client.UI.Dialogs.Overlays;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Gateways;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Home.ConnectionCard;
using ProtonVPN.Client.UI.Home.ConnectionCard.Overlays;
using ProtonVPN.Client.UI.Home.ConnectionError;
using ProtonVPN.Client.UI.Home.Details;
using ProtonVPN.Client.UI.Home.Help;
using ProtonVPN.Client.UI.Home.Map;
using ProtonVPN.Client.UI.Home.Recents;
using ProtonVPN.Client.UI.Home.Status;
using ProtonVPN.Client.UI.HumanVerification;
using ProtonVPN.Client.UI.Login;
using ProtonVPN.Client.UI.Login.Forms;
using ProtonVPN.Client.UI.Login.Overlays;
using ProtonVPN.Client.UI.ReportIssue;
using ProtonVPN.Client.UI.ReportIssue.Results;
using ProtonVPN.Client.UI.ReportIssue.Steps;
using ProtonVPN.Client.UI.Settings;
using ProtonVPN.Client.UI.Settings.Pages;
using ProtonVPN.Client.UI.Settings.Pages.Advanced;
using ProtonVPN.Client.UI.Tray;
using ProtonVPN.Client.UI.Update;

namespace ProtonVPN.Client.Installers;

public class ViewModelsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterViewModel<ReportIssueShellViewModel>(builder);
        RegisterViewModel<CategorySelectionViewModel>(builder);
        RegisterViewModel<QuickFixesViewModel>(builder);
        RegisterViewModel<ContactFormViewModel>(builder);
        RegisterViewModel<ReportIssueResultViewModel>(builder);
        RegisterViewModel<ProtocolOverlayViewModel>(builder);
        RegisterViewModel<LatencyOverlayViewModel>(builder);
        RegisterViewModel<ServerLoadOverlayViewModel>(builder);
        RegisterViewModel<SecureCoreOverlayViewModel>(builder);
        RegisterViewModel<SmartRoutingOverlayViewModel>(builder);
        RegisterViewModel<P2POverlayViewModel>(builder);
        RegisterViewModel<TorOverlayViewModel>(builder);
        RegisterViewModel<FreeConnectionsOverlayViewModel>(builder);
        RegisterViewModel<ChangeServerOverlayViewModel>(builder);
        RegisterViewModel<VpnSpeedViewModel>(builder);
        RegisterViewModel<IpAddressViewModel>(builder);
        RegisterViewModel<ConnectionDetailsViewModel>(builder);
        RegisterViewModel<RecentsViewModel>(builder);
        RegisterViewModel<VpnStatusViewModel>(builder);
        RegisterViewModel<NetShieldStatsViewModel>(builder);
        RegisterViewModel<ConnectionCardViewModel>(builder);
        RegisterViewModel<FreeConnectionCardViewModel>(builder);
        RegisterViewModel<HelpViewModel>(builder);
        RegisterViewModel<SettingsViewModel>(builder);
        RegisterViewModel<CountriesViewModel>(builder);
        RegisterViewModel<GatewaysViewModel>(builder);
        RegisterViewModel<AllCountriesPageViewModel>(builder);
        RegisterViewModel<SecureCoreCountriesPageViewModel>(builder);
        RegisterViewModel<SecureCoreCountryPageViewModel>(builder);
        RegisterViewModel<TorServersPageViewModel>(builder);
        RegisterViewModel<P2PCountriesPageViewModel>(builder);
        RegisterViewModel<TorCountriesPageViewModel>(builder);
        RegisterViewModel<CitiesPageViewModel>(builder);
        RegisterViewModel<P2PCitiesPageViewModel>(builder);
        RegisterViewModel<HomeViewModel>(builder);
        RegisterViewModel<ShellViewModel>(builder);
        RegisterViewModel<TrayIconViewModel>(builder);
        RegisterViewModel<CensorshipViewModel>(builder);
        RegisterViewModel<AutoStartupViewModel>(builder);
        RegisterViewModel<CustomDnsServersViewModel>(builder);
        RegisterViewModel<DebugLogsViewModel>(builder);
        RegisterViewModel<AdvancedSettingsViewModel>(builder);
        RegisterViewModel<VpnAcceleratorViewModel>(builder);
        RegisterViewModel<ProtocolViewModel>(builder);
        RegisterViewModel<SplitTunnelingViewModel>(builder);
        RegisterViewModel<PortForwardingViewModel>(builder);
        RegisterViewModel<KillSwitchViewModel>(builder);
        RegisterViewModel<NetShieldViewModel>(builder);
        RegisterViewModel<CountryTabViewModel>(builder);
        RegisterViewModel<AccountViewModel>(builder);
        RegisterViewModel<LoginShellViewModel>(builder);
        RegisterViewModel<LoginFormViewModel>(builder);
        RegisterViewModel<TwoFactorFormViewModel>(builder);
        RegisterViewModel<NoSearchResultsViewModel>(builder);
        RegisterViewModel<SsoLoginOverlayViewModel>(builder);
        RegisterViewModel<ConnectionErrorViewModel>(builder);
        RegisterViewModel<DeveloperToolsViewModel>(builder);
        RegisterViewModel<UpdateViewModel>(builder);

        RegisterViewModel<GalleryViewModel>(builder);
        RegisterViewModel<GalleryItemViewModel>(builder);
        RegisterViewModel<HumanVerificationViewModel>(builder).AutoActivate();
    }

    private IRegistrationBuilder<TType, ConcreteReflectionActivatorData, SingleRegistrationStyle>
        RegisterViewModel<TType>(ContainerBuilder builder) where TType : notnull
    {
        return builder.RegisterType<TType>().AsSelf().As<IEventMessageReceiver>().SingleInstance();
    }
}