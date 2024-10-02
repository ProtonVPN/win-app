/*
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

using Autofac;
using Autofac.Builder;
using ProtonVPN.Api.Installers;
using ProtonVPN.Client.EventMessaging.Installers;
using ProtonVPN.Client.Files.Installers;
using ProtonVPN.Client.Localization.Installers;
using ProtonVPN.Client.Logic.Announcements.Installers;
using ProtonVPN.Client.Logic.Auth.Installers;
using ProtonVPN.Client.Logic.Connection.Installers;
using ProtonVPN.Client.Logic.Feedback.Installers;
using ProtonVPN.Client.Logic.Profiles.Installers;
using ProtonVPN.Client.Logic.Recents.Installers;
using ProtonVPN.Client.Logic.Servers.Installers;
using ProtonVPN.Client.Logic.Services.Installers;
using ProtonVPN.Client.Logic.Updates.Installers;
using ProtonVPN.Client.Logic.Users.Installers;
using ProtonVPN.Client.Notifications.Installers;
using ProtonVPN.Client.Settings.Installers;
using ProtonVPN.Common.Legacy.OS.Net.NetworkInterface;
using ProtonVPN.Configurations.Installers;
using ProtonVPN.Crypto.Installers;
using ProtonVPN.Dns.Installers;
using ProtonVPN.EntityMapping.Installers;
using ProtonVPN.Files.Installers;
using ProtonVPN.IssueReporting.Installers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Installers;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Models.Clipboards;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Services.Activation;
using ProtonVPN.Client.Services.Bootstrapping;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Services.Dispatching;
using ProtonVPN.Client.Services.Mapping;
using ProtonVPN.Client.Services.Navigation;
using ProtonVPN.Client.Services.Notification;
using ProtonVPN.Client.Services.Selection;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Services.Verification;
using ProtonVPN.Client.TEMP;
using ProtonVPN.Client.UI;
using ProtonVPN.Client.UI.Dialogs.ReportIssue;
using ProtonVPN.Client.UI.Dialogs.ReportIssue.Pages;
using ProtonVPN.Client.UI.Login;
using ProtonVPN.Client.UI.Login.Components;
using ProtonVPN.Client.UI.Login.Overlays;
using ProtonVPN.Client.UI.Login.Pages;
using ProtonVPN.Client.UI.Main;
using ProtonVPN.Client.UI.Main.Features.KillSwitch;
using ProtonVPN.Client.UI.Main.Features.NetShield;
using ProtonVPN.Client.UI.Main.Features.PortForwarding;
using ProtonVPN.Client.UI.Main.Features.SplitTunneling;
using ProtonVPN.Client.UI.Main.Home;
using ProtonVPN.Client.UI.Main.Home.Card;
using ProtonVPN.Client.UI.Main.Home.Details;
using ProtonVPN.Client.UI.Main.Home.Details.Connection;
using ProtonVPN.Client.UI.Main.Home.Details.Location;
using ProtonVPN.Client.UI.Main.Home.Map;
using ProtonVPN.Client.UI.Main.Settings;
using ProtonVPN.Client.UI.Main.Settings.Pages;
using ProtonVPN.Client.UI.Main.Settings.Pages.About;
using ProtonVPN.Client.UI.Main.Settings.Pages.Advanced;
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
using ProtonVPN.Client.UI.Tray;
using ProtonVPN.OperatingSystems.Processes.Installers;
using ProtonVPN.OperatingSystems.Registries.Installers;
using ProtonVPN.OperatingSystems.Services.Installers;
using ProtonVPN.ProcessCommunication.Installers;
using ProtonVPN.Serialization.Installers;
using ProtonVPN.ProcessCommunication.Client.Installers;

namespace ProtonVPN.Client.Modules;

public class AppModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterLoggerConfiguration(c => c.ClientLogsFilePath);

        RegisterExternalModules(builder);
        RegisterLocalServices(builder);
        RegisterLocalHandlers(builder);
        RegisterViewModels(builder);
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
               .RegisterModule<AnnouncementsModule>();
    }

    private void RegisterLocalServices(ContainerBuilder builder)
    {
        builder.RegisterType<Bootstrapper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<PageViewMapper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<OverlayViewMapper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UIThreadDispatcher>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<HumanVerifier>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<HumanVerificationConfig>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UrlsBrowser>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ThemeSelector>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MainWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MainWindowOverlayActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MainWindowViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LoginViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MainViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<DetailsViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SidebarViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionsViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SettingsViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ClipboardEditor>().As<IClipboardEditor>().SingleInstance();
        builder.RegisterType<Urls>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ReportIssueWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ReportIssueWindowOverlayActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ReportIssueViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<UpsellCarouselWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<AppNotificationSender>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ApplicationIconSelector>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ApplicationThemeSelector>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ConnectionGroupFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionItemFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LocationItemFactory>().AsImplementedInterfaces().SingleInstance();

        builder.Register(c =>
            new SafeSystemNetworkInterfaces(c.Resolve<ILogger>(), new SystemNetworkInterfaces()))
            .As<INetworkInterfaces>().SingleInstance();
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

    private void RegisterViewModels(ContainerBuilder builder)
    {
        RegisterViewModel<MainWindowShellViewModel>(builder);
        RegisterViewModel<NavigationMasterControllerViewModel>(builder); // TEMP
        RegisterViewModel<GalleryPageViewModel>(builder); // TEMP
        RegisterViewModel<GalleryWidgetViewModel>(builder); // TEMP

        RegisterViewModel<TrayIconComponentViewModel>(builder);

        RegisterViewModel<ReportIssueComponentViewModel>(builder);

        RegisterViewModel<LoginPageViewModel>(builder);
        RegisterViewModel<SignInPageViewModel>(builder);
        RegisterViewModel<TwoFactorPageViewModel>(builder);
        RegisterViewModel<LoadingPageViewModel>(builder);
        RegisterViewModel<DisableKillSwitchBannerViewModel>(builder);

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
        RegisterViewModel<HomePageViewModel>(builder);
        RegisterViewModel<MapComponentViewModel>(builder);
        RegisterViewModel<ConnectionCardComponentViewModel>(builder);
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
        RegisterViewModel<DeveloperToolsPageViewModel>(builder);
        RegisterViewModel<AutoStartupSettingsPageViewModel>(builder);
        RegisterViewModel<SideWidgetsHostComponentViewModel>(builder);
        RegisterViewModel<VpnSpeedViewModel>(builder);

        RegisterViewModel<ReportIssueShellViewModel>(builder);
        RegisterViewModel<ReportIssueCategoriesPageViewModel>(builder);
        RegisterViewModel<ReportIssueCategoryPageViewModel>(builder);
        RegisterViewModel<ReportIssueContactPageViewModel>(builder);
        RegisterViewModel<ReportIssueResultPageViewModel>(builder);

        RegisterViewModel<HumanVerificationOverlayViewModel>(builder);
        RegisterViewModel<P2POverlayViewModel>(builder);
        RegisterViewModel<SecureCoreOverlayViewModel>(builder);
        RegisterViewModel<TorOverlayViewModel>(builder);
        RegisterViewModel<SmartRoutingOverlayViewModel>(builder);
        RegisterViewModel<ServerLoadOverlayViewModel>(builder);
        RegisterViewModel<TroubleshootingOverlayViewModel>(builder);
        RegisterViewModel<SsoLoginOverlayViewModel>(builder);

        builder.RegisterType<ReleaseViewModelFactory>().SingleInstance();
    }

    private static IRegistrationBuilder<TViewModel, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterViewModel<TViewModel>(ContainerBuilder builder)
        where TViewModel : ViewModelBase
    {
        return builder.RegisterType<TViewModel>().AsSelf().AsImplementedInterfaces().SingleInstance();
    }
}