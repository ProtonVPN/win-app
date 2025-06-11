/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Api.Installers;
using ProtonVPN.Client.Commands;
using ProtonVPN.Client.Core.Bases.Helpers;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Installers;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Files.Installers;
using ProtonVPN.Client.Handlers;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Localization.Installers;
using ProtonVPN.Client.Logic.Announcements.Installers;
using ProtonVPN.Client.Logic.Auth.Installers;
using ProtonVPN.Client.Logic.Connection.Installers;
using ProtonVPN.Client.Logic.Feedback.Installers;
using ProtonVPN.Client.Logic.Profiles.Installers;
using ProtonVPN.Client.Logic.Recents.Installers;
using ProtonVPN.Client.Logic.Searches.Installers;
using ProtonVPN.Client.Logic.Servers.Installers;
using ProtonVPN.Client.Logic.Services.Installers;
using ProtonVPN.Client.Logic.Updates.Installers;
using ProtonVPN.Client.Logic.Users.Installers;
using ProtonVPN.Client.Models.Announcements;
using ProtonVPN.Client.Notifications.Installers;
using ProtonVPN.Client.Services.Activation;
using ProtonVPN.Client.Services.Bootstrapping;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Services.Dispatching;
using ProtonVPN.Client.Services.Edition;
using ProtonVPN.Client.Services.Enabling;
using ProtonVPN.Client.Services.Lifecycle;
using ProtonVPN.Client.Services.Mapping;
using ProtonVPN.Client.Services.Navigation;
using ProtonVPN.Client.Services.PortForwarding;
using ProtonVPN.Client.Services.ProcessCommunication;
using ProtonVPN.Client.Services.Selection;
using ProtonVPN.Client.Services.SignoutHandling;
using ProtonVPN.Client.Services.Validation;
using ProtonVPN.Client.Services.Verification;
using ProtonVPN.Client.Settings.Installers;
using ProtonVPN.Client.UI;
using ProtonVPN.Client.UI.Dialogs.DebugTools;
using ProtonVPN.Client.UI.Dialogs.OneTimeAnnouncement;
using ProtonVPN.Client.UI.Dialogs.ReportIssue;
using ProtonVPN.Client.UI.Dialogs.ReportIssue.Pages;
using ProtonVPN.Client.UI.Dialogs.Troubleshooting;
using ProtonVPN.Client.UI.Dialogs.Upsell;
using ProtonVPN.Client.UI.Dialogs.Upsell.Features;
using ProtonVPN.Client.UI.Login;
using ProtonVPN.Client.UI.Login.Components;
using ProtonVPN.Client.UI.Login.Overlays;
using ProtonVPN.Client.UI.Login.Pages;
using ProtonVPN.Client.UI.Main;
using ProtonVPN.Client.UI.Main.Components;
using ProtonVPN.Client.UI.Main.Components.Banners;
using ProtonVPN.Client.UI.Main.FeatureIcons;
using ProtonVPN.Client.UI.Main.Features.KillSwitch;
using ProtonVPN.Client.UI.Main.Features.NetShield;
using ProtonVPN.Client.UI.Main.Features.PortForwarding;
using ProtonVPN.Client.UI.Main.Features.SplitTunneling;
using ProtonVPN.Client.UI.Main.Home;
using ProtonVPN.Client.UI.Main.Home.Card;
using ProtonVPN.Client.UI.Main.Home.Card.DefaultConnections;
using ProtonVPN.Client.UI.Main.Home.Details;
using ProtonVPN.Client.UI.Main.Home.Details.Connection;
using ProtonVPN.Client.UI.Main.Home.Details.Flyouts;
using ProtonVPN.Client.UI.Main.Home.Details.Location;
using ProtonVPN.Client.UI.Main.Home.Upsell;
using ProtonVPN.Client.UI.Main.Map;
using ProtonVPN.Client.UI.Main.Profiles;
using ProtonVPN.Client.UI.Main.Profiles.Components;
using ProtonVPN.Client.UI.Main.Settings;
using ProtonVPN.Client.UI.Main.Settings.Connection;
using ProtonVPN.Client.UI.Main.Settings.Pages;
using ProtonVPN.Client.UI.Main.Settings.Pages.About;
using ProtonVPN.Client.UI.Main.Settings.Pages.Advanced;
using ProtonVPN.Client.UI.Main.Settings.Pages.Connection;
using ProtonVPN.Client.UI.Main.Settings.Pages.DefaultConnections;
using ProtonVPN.Client.UI.Main.Sidebar;
using ProtonVPN.Client.UI.Main.Sidebar.Connections;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Countries;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Countries.All;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Countries.P2P;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Countries.SecureCore;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Countries.Tor;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Gateways;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Recents;
using ProtonVPN.Client.UI.Main.Sidebar.Search;
using ProtonVPN.Client.UI.Main.Widgets;
using ProtonVPN.Client.UI.Overlays.HumanVerification;
using ProtonVPN.Client.UI.Overlays.Information;
using ProtonVPN.Client.UI.Overlays.Information.Notification;
using ProtonVPN.Client.UI.Overlays.WhatsNew;
using ProtonVPN.Client.UI.Overlays.Upsell;
using ProtonVPN.Client.UI.Overlays.Welcome;
using ProtonVPN.Client.UI.Tray;
using ProtonVPN.Client.UI.Update;
using ProtonVPN.Client.UnsecureWifiDetection.Installers;
using ProtonVPN.Common.Legacy.OS.DeviceIds;
using ProtonVPN.Common.Legacy.OS.Processes;
using ProtonVPN.Common.Legacy.OS.Systems;
using ProtonVPN.Configurations.Installers;
using ProtonVPN.Crypto.Installers;
using ProtonVPN.Dns.Installers;
using ProtonVPN.EntityMapping.Installers;
using ProtonVPN.Files.Installers;
using ProtonVPN.IssueReporting.Installers;
using ProtonVPN.Logging.Installers;
using ProtonVPN.NetworkTimeProtocols.Installers;
using ProtonVPN.OperatingSystems.Network.Installers;
using ProtonVPN.OperatingSystems.Processes.Installers;
using ProtonVPN.OperatingSystems.Registries.Installers;
using ProtonVPN.OperatingSystems.Services.Installers;
using ProtonVPN.ProcessCommunication.Client.Installers;
using ProtonVPN.ProcessCommunication.Installers;
using ProtonVPN.Serialization.Installers;
using ProtonVPN.StatisticalEvents.Installers;
using ProtonVPN.Client.Services.Upselling;
using ProtonVPN.Client.Services.Bootstrapping.Activators;
using ProtonVPN.Client.UI.Dialogs.Tray;
using ProtonVPN.Client.UI.Dialogs.Tray.Pages;
using ProtonVPN.Client.UI.Main.Home.Status;
using ProtonVPN.Client.UI.Dialogs.NpsSurvey;

namespace ProtonVPN.Client.Installers;

public class AppModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterLoggerConfiguration(c => c.ClientLogsFilePath);

        RegisterExternalModules(builder);
        RegisterLocalServices(builder);
        RegisterLocalHandlers(builder);
        RegisterViewModels(builder);
        RegisterExternalServices(builder);
        RegisterCommands(builder);
    }

    private static IRegistrationBuilder<TViewModel, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterViewModel<TViewModel>(ContainerBuilder builder)
        where TViewModel : ViewModelBase
    {
        return builder.RegisterType<TViewModel>().AsSelf().AsImplementedInterfaces().SingleInstance();
    }

    private void RegisterExternalServices(ContainerBuilder builder)
    {
        builder.RegisterType<SystemProcesses>().As<IOsProcesses>().SingleInstance();
        builder.RegisterType<SystemState>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<DeviceIdCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServiceEnabler>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<BaseFilteringEngineDialogHandler>().AsImplementedInterfaces().SingleInstance();
    }

    private void RegisterExternalModules(ContainerBuilder builder)
    {
        builder.RegisterModule<EventMessageReceiverActivationModule>()
               .RegisterModule<LoggingModule>()
               .RegisterModule<RegistriesModule>()
               .RegisterModule<ProcessesModule>()
               .RegisterModule<ServicesModule>()
               .RegisterModule<ServicesLogicModule>()
               .RegisterModule<ConnectionLogicModule>()
               .RegisterModule<ClientProcessCommunicationModule>()
               .RegisterModule<EntityMappingModule>()
               .RegisterModule<ProcessCommunicationModule>()
               .RegisterModule<LocalizationModule>()
               .RegisterModule<EventMessagingModule>()
               .RegisterModule<RecentsLogicModule>()
               .RegisterModule<ProfilesLogicModule>()
               .RegisterModule<ServersLogicModule>()
               .RegisterModule<ConfigurationsModule>()
               .RegisterModule<ApiModule>()
               .RegisterModule<SettingsModule>()
               .RegisterModule<AuthLogicModule>()
               .RegisterModule<CryptoModule>()
               .RegisterModule<FeedbackLogicModule>()
               .RegisterModule<DnsModule>()
               .RegisterModule<SerializationModule>()
               .RegisterModule<UpdatesLogicModule>()
               .RegisterModule<IssueReportingModule>()
               .RegisterModule<NotificationsModule>()
               .RegisterModule<FilesModule>()
               .RegisterModule<ClientFilesModule>()
               .RegisterModule<PowerEventsModule>()
               .RegisterModule<UsersLogicModule>()
               .RegisterModule<AnnouncementsModule>()
               .RegisterModule<SearchesModule>()
               .RegisterModule<NetworkModule>()
               .RegisterModule<UnsecureWifiDetectionModule>()
               .RegisterModule<StatisticalEventsModule>()
               .RegisterModule<NetworkTimeProtocolsModule>();
    }

    private void RegisterLocalServices(ContainerBuilder builder)
    {
        builder.RegisterType<Bootstrapper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AppProtocolActivator>().AsImplementedInterfaces().SingleInstance().AutoActivate();
        builder.RegisterType<AppStartupActivator>().AsImplementedInterfaces().SingleInstance().AutoActivate();
        builder.RegisterType<PageViewMapper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<OverlayViewMapper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UIThreadDispatcher>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<HumanVerifier>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<HumanVerificationConfig>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UrlsBrowser>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AccountUpgradeUrlLauncher>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ClipboardEditor>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SignOutHandler>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<PortForwardingClipboardService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AnnouncementActivator>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<MainWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MainWindowOverlayActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MainWindowViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LoginViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MainViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<DetailsViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SidebarViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionsViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SettingsViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ReportIssueWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ReportIssueWindowOverlayActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ReportIssueViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<UpsellCarouselWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UpsellCarouselViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<TrayAppWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<TrayAppViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<DebugToolsWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<TroubleshootingWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<OneTimeAnnouncementWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NpsSurveyWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ApplicationIconSelector>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ApplicationThemeSelector>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ConnectionGroupFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionItemFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LocationItemFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<CommonItemFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SplitTunnelingItemFactory>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<SystemTimeValidator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ViewModelHelper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AppExitInvoker>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<CoordinatesProvider>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ServiceCommunicationErrorHandler>().AsImplementedInterfaces().SingleInstance();
    }

    private void RegisterLocalHandlers(ContainerBuilder builder)
    {
        Type handlerType = typeof(IHandler);

        builder.RegisterAssemblyTypes(handlerType.Assembly)
               .Where(handlerType.IsAssignableFrom)
               .AsImplementedInterfaces()
               .SingleInstance()
               .AutoActivate();
    }

    private void RegisterCommands(ContainerBuilder builder)
    {
        Type providerType = typeof(ICommandProvider);

        builder.RegisterAssemblyTypes(providerType.Assembly)
               .Where(providerType.IsAssignableFrom)
               .AsImplementedInterfaces()
               .SingleInstance();
    }

    private void RegisterViewModels(ContainerBuilder builder)
    {
        RegisterViewModel<MainWindowShellViewModel>(builder);

        RegisterViewModel<TrayIconComponentViewModel>(builder);

        RegisterViewModel<LoginPageViewModel>(builder);
        RegisterViewModel<SignInPageViewModel>(builder);
        RegisterViewModel<TwoFactorPageViewModel>(builder);
        RegisterViewModel<LoadingPageViewModel>(builder);
        RegisterViewModel<DisableKillSwitchBannerViewModel>(builder);

        RegisterViewModel<BannerViewModel>(builder).AutoActivate();
        RegisterViewModel<ProminentBannerViewModel>(builder).AutoActivate();
        RegisterViewModel<TitleBarMenuViewModel>(builder);
        RegisterViewModel<NetShieldStatsViewModel>(builder);
        RegisterViewModel<ActivePortComponentViewModel>(builder);
        RegisterViewModel<CurrentProfileIconViewModel>(builder);
        RegisterViewModel<MainPageViewModel>(builder);
        RegisterViewModel<SidebarComponentViewModel>(builder);
        RegisterViewModel<ConnectionsPageViewModel>(builder);
        RegisterViewModel<RecentsPageViewModel>(builder);
        RegisterViewModel<ProfilesPageViewModel>(builder);
        RegisterViewModel<GatewaysPageViewModel>(builder);
        RegisterViewModel<CountriesPageViewModel>(builder);
        RegisterViewModel<AllCountriesComponentViewModel>(builder);
        RegisterViewModel<SecureCoreCountriesComponentViewModel>(builder);
        RegisterViewModel<P2PCountriesComponentViewModel>(builder);
        RegisterViewModel<TorCountriesComponentViewModel>(builder);
        RegisterViewModel<SearchResultsPageViewModel>(builder);
        RegisterViewModel<HomeComponentViewModel>(builder);
        RegisterViewModel<MapComponentViewModel>(builder).AutoActivate();
        RegisterViewModel<ConnectionCardComponentViewModel>(builder);
        RegisterViewModel<DefaultConnectionSelectorViewModel>(builder);
        RegisterViewModel<ChangeServerComponentViewModel>(builder);
        RegisterViewModel<ConnectionCardUpsellBannerViewModel>(builder);
        RegisterViewModel<ConnectionStatusGradientViewModel>(builder);
        RegisterViewModel<ConnectionStatusHeaderViewModel>(builder);
        RegisterViewModel<DetailsComponentViewModel>(builder);
        RegisterViewModel<ConnectionDetailsPageViewModel>(builder).AutoActivate();
        RegisterViewModel<LocationDetailsPageViewModel>(builder);
        RegisterViewModel<KillSwitchPageViewModel>(builder);
        RegisterViewModel<KillSwitchWidgetViewModel>(builder);
        RegisterViewModel<NetShieldPageViewModel>(builder);
        RegisterViewModel<NetShieldWidgetViewModel>(builder);
        RegisterViewModel<PortForwardingPageViewModel>(builder);
        RegisterViewModel<PortForwardingWidgetViewModel>(builder);
        RegisterViewModel<SplitTunnelingPageViewModel>(builder);
        RegisterViewModel<SplitTunnelingWidgetViewModel>(builder);
        RegisterViewModel<SettingsPageViewModel>(builder);
        RegisterViewModel<SettingsWidgetViewModel>(builder);
        RegisterViewModel<CommonSettingsPageViewModel>(builder);
        RegisterViewModel<AdvancedSettingsPageViewModel>(builder);
        RegisterViewModel<ProtocolSettingsPageViewModel>(builder);
        RegisterViewModel<DefaultConnectionSettingsPageViewModel>(builder);
        RegisterViewModel<VpnAcceleratorSettingsPageViewModel>(builder);
        RegisterViewModel<CustomDnsServersViewModel>(builder);
        RegisterViewModel<DebugLogsPageViewModel>(builder);
        RegisterViewModel<AboutPageViewModel>(builder);
        RegisterViewModel<LicensingViewModel>(builder);
        RegisterViewModel<CensorshipSettingsPageViewModel>(builder);
        RegisterViewModel<AutoStartupSettingsPageViewModel>(builder);
        RegisterViewModel<SideWidgetsHostComponentViewModel>(builder);
        RegisterViewModel<VpnSpeedViewModel>(builder);
        RegisterViewModel<ConnectionIntentSelectorViewModel>(builder);
        RegisterViewModel<ProfileIconSelectorViewModel>(builder);
        RegisterViewModel<ProfileSettingsSelectorViewModel>(builder);
        RegisterViewModel<ProfileOptionsSelectorViewModel>(builder);
        RegisterViewModel<ProfilePageViewModel>(builder);
        RegisterViewModel<UserDetailsComponentViewModel>(builder);
        RegisterViewModel<ConnectionSettingsViewModel>(builder);
        RegisterViewModel<ConnectionErrorViewModel>(builder);
        RegisterViewModel<UpdateViewModel>(builder).AutoActivate();

        RegisterViewModel<ReportIssueShellViewModel>(builder);
        RegisterViewModel<ReportIssueCategoriesPageViewModel>(builder);
        RegisterViewModel<ReportIssueCategoryPageViewModel>(builder);
        RegisterViewModel<ReportIssueContactPageViewModel>(builder);
        RegisterViewModel<ReportIssueResultPageViewModel>(builder);

        RegisterViewModel<TrayAppShellViewModel>(builder);
        RegisterViewModel<TrayLoginPageViewModel>(builder);
        RegisterViewModel<TrayMainPageViewModel>(builder);

        RegisterViewModel<UpsellCarouselShellViewModel>(builder);
        RegisterViewModel<AdvancedSettingsUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<MultipleDevicesUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<NetShieldUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<P2PUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<SecureCoreUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<SpeedUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<SplitTunnelingUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<StreamingUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<TorUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<WorldwideCoverageUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<ProfilesUpsellFeaturePageViewModel>(builder);

        RegisterViewModel<HumanVerificationOverlayViewModel>(builder).AutoActivate();
        RegisterViewModel<P2POverlayViewModel>(builder);
        RegisterViewModel<SecureCoreOverlayViewModel>(builder);
        RegisterViewModel<TorOverlayViewModel>(builder);
        RegisterViewModel<SmartRoutingOverlayViewModel>(builder);
        RegisterViewModel<ProfileOverlayViewModel>(builder);
        RegisterViewModel<ServerLoadOverlayViewModel>(builder);
        RegisterViewModel<SsoLoginOverlayViewModel>(builder);
        RegisterViewModel<OutdatedClientOverlayViewModel>(builder).AutoActivate();
        RegisterViewModel<WelcomeOverlayViewModel>(builder);
        RegisterViewModel<WelcomeToVpnPlusOverlayViewModel>(builder);
        RegisterViewModel<WelcomeToVpnUnlimitedOverlayViewModel>(builder);
        RegisterViewModel<WelcomeToVpnB2BOverlayViewModel>(builder);
        RegisterViewModel<WhatsNewOverlayViewModel>(builder);

        RegisterViewModel<KillSwitchIconViewModel>(builder);
        RegisterViewModel<ProtocolIconViewModel>(builder);
        RegisterViewModel<NetShieldIconViewModel>(builder);
        RegisterViewModel<PortForwardingIconViewModel>(builder);
        RegisterViewModel<SplitTunnelingIconViewModel>(builder);
        RegisterViewModel<VpnAcceleratorIconViewModel>(builder);

        RegisterViewModel<FreeConnectionsOverlayViewModel>(builder);

        RegisterViewModel<IpAddressFlyoutViewModel>(builder);
        RegisterViewModel<CountryFlyoutViewModel>(builder);
        RegisterViewModel<IspFlyoutViewModel>(builder);
        RegisterViewModel<ServerLoadFlyoutViewModel>(builder);
        RegisterViewModel<ProtocolFlyoutViewModel>(builder);
        RegisterViewModel<VolumeFlyoutViewModel>(builder).AutoActivate();
        RegisterViewModel<SpeedFlyoutViewModel>(builder).AutoActivate();

        RegisterViewModel<DebugToolsShellViewModel>(builder);
        RegisterViewModel<OneTimeAnnouncementShellViewModel>(builder).AutoActivate();
        RegisterViewModel<NpsSurveyShellViewModel>(builder).AutoActivate();
        RegisterViewModel<TroubleshootingShellViewModel>(builder);

        builder.RegisterType<ReleaseViewModelFactory>().SingleInstance();
    }
}