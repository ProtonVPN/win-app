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
using ProtonVPN.Client.Legacy.UI;
using ProtonVPN.Client.Legacy.UI.Announcements.Banner;
using ProtonVPN.Client.Legacy.UI.Announcements.Modals;
using ProtonVPN.Client.Legacy.UI.Connections.Countries;
using ProtonVPN.Client.Legacy.UI.Connections.Gateways;
using ProtonVPN.Client.Legacy.UI.Connections.P2P;
using ProtonVPN.Client.Legacy.UI.Connections.Profiles;
using ProtonVPN.Client.Legacy.UI.Connections.Profiles.Controls;
using ProtonVPN.Client.Legacy.UI.Connections.Profiles.Overlays;
using ProtonVPN.Client.Legacy.UI.Connections.SecureCore;
using ProtonVPN.Client.Legacy.UI.Connections.Tor;
using ProtonVPN.Client.Legacy.UI.Dialogs.Overlays;
using ProtonVPN.Client.Legacy.UI.Dialogs.Overlays.Welcome;
using ProtonVPN.Client.Legacy.UI.Features.KillSwitch;
using ProtonVPN.Client.Legacy.UI.Features.NetShield;
using ProtonVPN.Client.Legacy.UI.Features.PortForwarding;
using ProtonVPN.Client.Legacy.UI.Features.SplitTunneling;
using ProtonVPN.Client.Legacy.UI.Gallery;
using ProtonVPN.Client.Legacy.UI.Home;
using ProtonVPN.Client.Legacy.UI.Home.ConnectionCard;
using ProtonVPN.Client.Legacy.UI.Home.ConnectionCard.Overlays;
using ProtonVPN.Client.Legacy.UI.Home.ConnectionError;
using ProtonVPN.Client.Legacy.UI.Home.Details;
using ProtonVPN.Client.Legacy.UI.Home.Help;
using ProtonVPN.Client.Legacy.UI.Home.Recents;
using ProtonVPN.Client.Legacy.UI.Home.Status;
using ProtonVPN.Client.Legacy.UI.Login;
using ProtonVPN.Client.Legacy.UI.Login.Forms;
using ProtonVPN.Client.Legacy.UI.Login.Overlays;
using ProtonVPN.Client.Legacy.UI.ReportIssue;
using ProtonVPN.Client.Legacy.UI.ReportIssue.Results;
using ProtonVPN.Client.Legacy.UI.ReportIssue.Steps;
using ProtonVPN.Client.Legacy.UI.Settings;
using ProtonVPN.Client.Legacy.UI.Settings.Pages;
using ProtonVPN.Client.Legacy.UI.Settings.Pages.About;
using ProtonVPN.Client.Legacy.UI.Settings.Pages.Advanced;
using ProtonVPN.Client.Legacy.UI.Settings.Pages.DefaultConnections;
using ProtonVPN.Client.Legacy.UI.Sidebar;
using ProtonVPN.Client.Legacy.UI.Tray;
using ProtonVPN.Client.Legacy.UI.Upsell.Banner;
using ProtonVPN.Client.Legacy.UI.Upsell.Carousel;
using ProtonVPN.Client.Legacy.UI.Upsell.Carousel.Features;
using ProtonVPN.Client.Legacy.UI.Upsell.Carousel.Features.Base;

namespace ProtonVPN.Client.Legacy.Installers;

public class ViewModelsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterViewModel<ShellViewModel>(builder);
        RegisterViewModel<LoginShellViewModel>(builder);
        RegisterViewModel<ReportIssueShellViewModel>(builder);
        RegisterViewModel<UpsellCarouselShellViewModel>(builder);

        RegisterViewModel<AnnouncementModalViewModel>(builder);
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
        RegisterViewModel<VpnSpeedViewModel>(builder).AutoActivate();
        RegisterViewModel<IpAddressViewModel>(builder).AutoActivate();
        RegisterViewModel<ConnectionDetailsViewModel>(builder).AutoActivate();
        RegisterViewModel<RecentsViewModel>(builder);
        RegisterViewModel<VpnStatusViewModel>(builder);
        RegisterViewModel<NetShieldStatsViewModel>(builder);
        RegisterViewModel<ConnectionCardViewModel>(builder);
        RegisterViewModel<FreeConnectionCardViewModel>(builder);
        RegisterViewModel<HelpViewModel>(builder);
        RegisterViewModel<SettingsViewModel>(builder);
        RegisterViewModel<GatewaysPageViewModel>(builder);
        RegisterViewModel<ProfilesPageViewModel>(builder);
        RegisterViewModel<EditProfileOverlayViewModel>(builder);
        RegisterViewModel<ConnectionIntentSelectorViewModel>(builder);
        RegisterViewModel<ProfileIconSelectorViewModel>(builder);
        RegisterViewModel<ProfileSettingsSelectorViewModel>(builder);
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
        RegisterViewModel<WelcomeOverlayViewModel>(builder);
        RegisterViewModel<WelcomeToVpnPlusOverlayViewModel>(builder);
        RegisterViewModel<WelcomeToVpnUnlimitedOverlayViewModel>(builder);
        RegisterViewModel<WelcomeToVpnB2BOverlayViewModel>(builder);
        RegisterViewModel<ConnectionErrorViewModel>(builder);
        RegisterViewModel<UpsellBannerViewModel>(builder);
        RegisterViewModel<DeveloperToolsViewModel>(builder);
        RegisterViewModel<LicensingViewModel>(builder);
        RegisterViewModel<GalleryViewModel>(builder);
        RegisterViewModel<GalleryItemViewModel>(builder);
        RegisterViewModel<TroubleshootingOverlayViewModel>(builder);
        RegisterViewModel<DisableKillSwitchBannerViewModel>(builder);
        RegisterViewModel<AnnouncementBannerViewModel>(builder);

        RegisterViewModel<SidebarHomeViewModel>(builder);
        RegisterViewModel<SidebarGatewaysHeaderViewModel>(builder);
        RegisterViewModel<SidebarGatewaysViewModel>(builder);
        RegisterViewModel<SidebarConnectionsHeaderViewModel>(builder);
        RegisterViewModel<SidebarProfilesViewModel>(builder);
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
        return builder.RegisterType<TType>().AsSelf().AsImplementedInterfaces().SingleInstance();
    }

    private IRegistrationBuilder<TType, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterUpsellFeatureViewModel<TType>(ContainerBuilder builder)
        where TType : notnull
    {
        return builder.RegisterType<TType>().AsSelf().As<IEventMessageReceiver>().As<UpsellFeatureViewModelBase>().SingleInstance();
    }
}