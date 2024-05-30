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
using ProtonVPN.Client.UI.Connections.Countries;
using ProtonVPN.Client.UI.Connections.Gateways;
using ProtonVPN.Client.UI.Connections.P2P;
using ProtonVPN.Client.UI.Connections.SecureCore;
using ProtonVPN.Client.UI.Connections.Tor;
using ProtonVPN.Client.UI.Dialogs.Overlays;
using ProtonVPN.Client.UI.Features.KillSwitch;
using ProtonVPN.Client.UI.Features.NetShield;
using ProtonVPN.Client.UI.Features.PortForwarding;
using ProtonVPN.Client.UI.Features.SplitTunneling;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Home.ConnectionCard;
using ProtonVPN.Client.UI.Home.ConnectionCard.Overlays;
using ProtonVPN.Client.UI.Home.ConnectionError;
using ProtonVPN.Client.UI.Home.Details;
using ProtonVPN.Client.UI.Home.Help;
using ProtonVPN.Client.UI.Home.Recents;
using ProtonVPN.Client.UI.Home.Status;
using ProtonVPN.Client.UI.Login;
using ProtonVPN.Client.UI.Login.Forms;
using ProtonVPN.Client.UI.Login.Overlays;
using ProtonVPN.Client.UI.ReportIssue;
using ProtonVPN.Client.UI.ReportIssue.Results;
using ProtonVPN.Client.UI.ReportIssue.Steps;
using ProtonVPN.Client.UI.Settings;
using ProtonVPN.Client.UI.Settings.Pages;
using ProtonVPN.Client.UI.Settings.Pages.About;
using ProtonVPN.Client.UI.Settings.Pages.Advanced;
using ProtonVPN.Client.UI.Sidebar;
using ProtonVPN.Client.UI.Tray;
using ProtonVPN.Client.UI.Upsell.Banner;
using ProtonVPN.Client.UI.Upsell.Carousel;
using ProtonVPN.Client.UI.Upsell.Carousel.Features;
using ProtonVPN.Client.UI.Upsell.Carousel.Features.Base;

namespace ProtonVPN.Client.Installers;

public class ViewModelsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterViewModel<ShellViewModel>(builder);
        RegisterViewModel<LoginShellViewModel>(builder);
        RegisterViewModel<ReportIssueShellViewModel>(builder);
        RegisterViewModel<UpsellCarouselShellViewModel>(builder);

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
        RegisterViewModel<GatewaysPageViewModel>(builder);
        RegisterViewModel<CountriesPageViewModel>(builder);
        RegisterViewModel<CountryPageViewModel>(builder);
        RegisterViewModel<P2PCountriesPageViewModel>(builder);
        RegisterViewModel<P2PCountryPageViewModel>(builder);
        RegisterViewModel<SecureCoreCountriesPageViewModel>(builder);
        RegisterViewModel<TorCountriesPageViewModel>(builder);
        RegisterViewModel<HomeViewModel>(builder);
        RegisterViewModel<TrayIconViewModel>(builder);
        RegisterViewModel<CensorshipViewModel>(builder);
        RegisterViewModel<AutoStartupViewModel>(builder);
        RegisterViewModel<CustomDnsServersViewModel>(builder);
        RegisterViewModel<DebugLogsViewModel>(builder);
        RegisterViewModel<AdvancedSettingsViewModel>(builder);
        RegisterViewModel<VpnAcceleratorViewModel>(builder);
        RegisterViewModel<DefaultConnectionViewModel>(builder);
        RegisterViewModel<ProtocolViewModel>(builder);
        RegisterViewModel<SplitTunnelingViewModel>(builder);
        RegisterViewModel<PortForwardingViewModel>(builder);
        RegisterViewModel<KillSwitchViewModel>(builder);
        RegisterViewModel<NetShieldViewModel>(builder);
        RegisterViewModel<LoginFormViewModel>(builder);
        RegisterViewModel<TwoFactorFormViewModel>(builder);
        RegisterViewModel<LoadingFormViewModel>(builder);
        RegisterViewModel<SsoLoginOverlayViewModel>(builder);
        RegisterViewModel<ConnectionErrorViewModel>(builder);
        RegisterViewModel<UpsellBannerViewModel>(builder);
        RegisterViewModel<DeveloperToolsViewModel>(builder);
        RegisterViewModel<LicensingViewModel>(builder);
        RegisterViewModel<GalleryViewModel>(builder);
        RegisterViewModel<GalleryItemViewModel>(builder);
        RegisterViewModel<TroubleshootingOverlayViewModel>(builder);

        RegisterViewModel<SidebarHomeViewModel>(builder);
        RegisterViewModel<SidebarGatewaysHeaderViewModel>(builder);
        RegisterViewModel<SidebarGatewaysViewModel>(builder);
        RegisterViewModel<SidebarConnectionsHeaderViewModel>(builder);
        RegisterViewModel<SidebarCountriesViewModel>(builder);
        RegisterViewModel<SidebarP2PCountriesViewModel>(builder);
        RegisterViewModel<SidebarSecureCoreCountriesViewModel>(builder);
        RegisterViewModel<SidebarTorCountriesViewModel>(builder);
        RegisterViewModel<SidebarFeaturesHeaderViewModel>(builder);
        RegisterViewModel<SidebarNetShieldViewModel>(builder);
        RegisterViewModel<SidebarKillSwitchViewModel>(builder);
        RegisterViewModel<SidebarPortForwardingViewModel>(builder);
        RegisterViewModel<SidebarSplitTunnelingViewModel>(builder);
        RegisterViewModel<SidebarGalleryViewModel>(builder);
        RegisterViewModel<SidebarUpdateViewModel>(builder);
        RegisterViewModel<SidebarSettingsViewModel>(builder);
        RegisterViewModel<SidebarSeparatorViewModel>(builder);
        RegisterViewModel<SidebarAccountViewModel>(builder);

        RegisterUpsellFeatureViewModel<WorldwideCoverageUpsellFeatureViewModel>(builder);
        RegisterUpsellFeatureViewModel<SpeedUpsellFeatureViewModel>(builder);
        RegisterUpsellFeatureViewModel<StreamingUpsellFeatureViewModel>(builder);
        RegisterUpsellFeatureViewModel<NetShieldUpsellFeatureViewModel>(builder);
        RegisterUpsellFeatureViewModel<SecureCoreUpsellFeatureViewModel>(builder);
        RegisterUpsellFeatureViewModel<P2PUpsellFeatureViewModel>(builder);
        RegisterUpsellFeatureViewModel<MultipleDevicesUpsellFeatureViewModel>(builder);
        RegisterUpsellFeatureViewModel<TorUpsellFeatureViewModel>(builder);
        RegisterUpsellFeatureViewModel<SplitTunnelingUpsellFeatureViewModel>(builder);
        RegisterUpsellFeatureViewModel<AdvancedSettingsUpsellFeatureViewModel>(builder);

        RegisterViewModel<HumanVerificationOverlayViewModel>(builder).AutoActivate();
        RegisterViewModel<AboutViewModel>(builder).AutoActivate();
    }

    private IRegistrationBuilder<TType, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterViewModel<TType>(ContainerBuilder builder)
        where TType : notnull
    {
        return builder.RegisterType<TType>().AsSelf().As<IEventMessageReceiver>().SingleInstance();
    }

    private IRegistrationBuilder<TType, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterUpsellFeatureViewModel<TType>(ContainerBuilder builder)
    where TType : notnull
    {
        return builder.RegisterType<TType>().AsSelf().As<IEventMessageReceiver>().As<UpsellFeatureViewModelBase>().SingleInstance();
    }
}