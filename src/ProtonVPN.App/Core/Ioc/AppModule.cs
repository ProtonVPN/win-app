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

using System;
using Autofac;
using Caliburn.Micro;
using ProtonVPN.About;
using ProtonVPN.Account;
using ProtonVPN.Api;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Events;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.OS.Registry;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Text.Serialization;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Profiles.Cached;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.FileStoraging;
using ProtonVPN.Core.Service;
using ProtonVPN.Core.Service.Settings;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Startup;
using ProtonVPN.Core.Storage;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Core.Windows;
using ProtonVPN.Core.Windows.Popups;
using ProtonVPN.Crypto;
using ProtonVPN.FlashNotifications;
using ProtonVPN.GuestHoles.FileStoraging;
using ProtonVPN.HumanVerification;
using ProtonVPN.HumanVerification.Contracts;
using ProtonVPN.Login;
using ProtonVPN.Map;
using ProtonVPN.Modals;
using ProtonVPN.Modals.Dialogs;
using ProtonVPN.Modals.Welcome;
using ProtonVPN.Notifications;
using ProtonVPN.Partners;
using ProtonVPN.PlanDowngrading;
using ProtonVPN.PortForwarding;
using ProtonVPN.Resource.Colors;
using ProtonVPN.Servers;
using ProtonVPN.Settings;
using ProtonVPN.Settings.Migrations;
using ProtonVPN.Settings.ReconnectNotification;
using ProtonVPN.Settings.SplitTunneling;
using ProtonVPN.Sidebar;
using ProtonVPN.Streaming;
using ProtonVPN.Vpn;
using ProtonVPN.Vpn.Connectors;
using ProtonVPN.Windows;
using ProtonVPN.Windows.Popups;

namespace ProtonVPN.Core.Ioc
{
    public class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(c => new ConfigFactory().Config()).AsSelf().As<IConfiguration>().SingleInstance(); // REMOVE AS SELF
            builder.RegisterType<ConfigWriter>().As<IConfigWriter>().SingleInstance();

            builder.RegisterType<Bootstrapper>().SingleInstance();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<JsonSerializerFactory>().As<ITextSerializerFactory>().SingleInstance();

            builder.RegisterType<SidebarManager>().SingleInstance();
            builder.RegisterType<UpdateViewModel>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<VpnConnectionSpeed>().AsImplementedInterfaces().AsSelf().SingleInstance();

            builder.RegisterType<ServersFileStorage>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<GuestHoleServersFileStorage>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<ApiServers>().As<IApiServers>().SingleInstance();
            builder.RegisterType<ServerUpdater>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<AuthCertificateUpdater>().AsImplementedInterfaces().SingleInstance();
            builder.Register(c => new ServerLoadUpdater(
                    c.Resolve<IConfiguration>().ServerLoadUpdateInterval, // REMOVE THIS CUSTOM REGISTRATION
                    c.Resolve<ServerManager>(),
                    c.Resolve<IScheduler>(),
                    c.Resolve<IEventAggregator>(),
                    c.Resolve<IMainWindowState>(),
                    c.Resolve<IApiServers>(),
                    c.Resolve<ISingleActionFactory>(),
                    c.Resolve<ILastServerLoadTimeProvider>()))
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<UserStorage>().As<IUserStorage>().SingleInstance();
            builder.RegisterType<TruncatedLocation>().SingleInstance();

            builder.RegisterType<PinFactory>()
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ServerListFactory>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<VpnService>().SingleInstance();
            builder.RegisterType<ModalWindows>().As<IModalWindows>().SingleInstance();
            builder.RegisterType<ProtonVPN.Modals.Modals>().As<IModals>().SingleInstance();
            builder.RegisterType<PopupWindows>().As<IPopupWindows>().SingleInstance();
            builder.RegisterType<Dialogs>().As<IDialogs>().SingleInstance();
            builder.RegisterType<CurrentUserStartupRecord>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<AutoStartup>().As<IAutoStartup>().SingleInstance();
            builder.RegisterType<SyncableAutoStartup>().As<ISyncableAutoStartup>().SingleInstance();
            builder.RegisterType<SyncedAutoStartup>().AsSelf().As<ISettingsAware>().SingleInstance();

            builder.RegisterType<AppSettingsStorage>().SingleInstance();
            builder.Register(c =>
                    new EnumerableAsJsonSettings(
                        new CachedSettings(
                            new EnumAsStringSettings(
                                new SelfRepairingSettings(
                                    c.Resolve<AppSettingsStorage>(),
                                    c.Resolve<IAppExitInvoker>())))))
                .As<ISettingsStorage>()
                .SingleInstance();

            builder.RegisterType<PerUserSettings>()
                .AsSelf()
                .As<ILoggedInAware>()
                .As<ILogoutAware>()
                .SingleInstance();
            builder.RegisterType<UserSettings>().SingleInstance();

            builder.RegisterType<PredefinedProfiles>().SingleInstance();
            builder.RegisterType<CachedProfiles>().SingleInstance();
            builder.RegisterType<ApiProfiles>().SingleInstance();
            builder.RegisterType<Profiles.Profiles>().SingleInstance();
            builder.RegisterType<SyncProfiles>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<SyncProfile>().SingleInstance();

            builder.RegisterType<AppSettings>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<InitialAppSettingsMigration>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<ProtonVPN.Settings.Migrations.v1_7_2.AppSettingsMigration>().AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<ProtonVPN.Settings.Migrations.v1_7_2.UserSettingsMigration>().AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<ProtonVPN.Settings.Migrations.v1_8_0.AppSettingsMigration>().AsImplementedInterfaces()
                .SingleInstance();
            builder.Register(c => new ProtonVPN.Settings.Migrations.v1_8_0.UserSettingsMigration(
                    c.Resolve<ISettingsStorage>(),
                    c.Resolve<UserSettings>()))
                .As<IUserSettingsMigration>()
                .SingleInstance();
            builder.RegisterType<ProtonVPN.Settings.Migrations.v1_10_0.AppSettingsMigration>().AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<ProtonVPN.Settings.Migrations.v1_17_0.AppSettingsMigration>().AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<ProtonVPN.Settings.Migrations.v1_18_3.AppSettingsMigration>().AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<ProtonVPN.Settings.Migrations.v1_20_0.AppSettingsMigration>().AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<ProtonVPN.Settings.Migrations.v1_22_0.AppSettingsMigration>().AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<ProtonVPN.Settings.Migrations.v1_27_1.AppSettingsMigration>().AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<ProtonVPN.Settings.Migrations.v2_0_0.AppSettingsMigration>().AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<ProtonVPN.Settings.Migrations.v2_0_2.UserSettingsMigration>().AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<ProtonVPN.Settings.Migrations.v2_0_6.AppSettingsMigration>().AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<MapLineManager>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<VpnEvents>();
            builder.RegisterType<SettingsServiceClientManager>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<SettingsServiceClient>().SingleInstance();
            builder.RegisterType<ServiceChannelFactory>().SingleInstance();
            builder.RegisterType<SettingsContractProvider>().SingleInstance();
            builder.RegisterType<AutoConnect>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterInstance(Properties.Settings.Default);

            builder.Register(c => new ServiceRetryPolicy(2, TimeSpan.FromSeconds(2))).SingleInstance();
            builder.Register(c =>
                    new VpnSystemService(
                        new ReliableService(
                            c.Resolve<ServiceRetryPolicy>(),
                            new SafeService(
                                new LoggingService(
                                    c.Resolve<ILogger>(),
                                    new SystemService(
                                        c.Resolve<IConfiguration>().ServiceName, // REMOVE THIS CUSTOM REGISTRATION
                                        c.Resolve<IOsProcesses>())))),
                        c.Resolve<ILogger>(),
                        c.Resolve<IServiceEnabler>()))
                .SingleInstance();

            builder.RegisterType<VpnServiceManager>().SingleInstance();
            builder.RegisterType<ServiceEnabler>().As<IServiceEnabler>().SingleInstance();
            builder.Register(c => new VpnServiceActionDecorator(
                    c.Resolve<VpnSystemService>(),
                    c.Resolve<VpnServiceManager>(),
                    c.Resolve<IModals>(),
                    c.Resolve<BaseFilteringEngineService>()))
                .As<IVpnServiceManager>().SingleInstance();
            builder.RegisterType<NetworkAdapterValidator>().As<INetworkAdapterValidator>().SingleInstance();
            builder.RegisterType<VpnManager>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<VpnReconnector>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<VpnConnector>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<SimilarServerCandidatesGenerator>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ServerConnector>().SingleInstance();
            builder.RegisterType<ProfileConnector>().SingleInstance();
            builder.RegisterType<CountryConnector>().SingleInstance();
            builder.RegisterType<GuestHoleConnector>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<GuestHoleState>().SingleInstance();
            builder.RegisterType<DisconnectError>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<VpnStateNotification>()
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<OutdatedAppNotification>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<AppExitHandler>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<UserLocationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<InstalledApps>().SingleInstance();
            builder.RegisterType<Onboarding.Onboarding>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<SystemNotification>().AsImplementedInterfaces().SingleInstance();
            builder.Register(c => new MonitoredVpnService(
                    c.Resolve<IConfiguration>(), // REMOVE THIS CUSTOM REGISTRATION
                    c.Resolve<VpnSystemService>(),
                    c.Resolve<IVpnManager>(),
                    c.Resolve<ILogger>()))
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<BaseFilteringEngineService>().SingleInstance();
            builder.Register(c => new UpdateNotification(
                    c.Resolve<IConfiguration>().UpdateRemindInterval, // REMOVE THIS CUSTOM REGISTRATION
                    c.Resolve<IUserAuthenticator>(),
                    c.Resolve<IEventAggregator>(),
                    c.Resolve<UpdateFlashNotificationViewModel>()))
                .AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<SystemProxyNotification>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<InsecureNetworkNotification>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.Register(c => new Language(
                    c.Resolve<IAppSettings>(),
                    c.Resolve<ILanguageProvider>(),
                    c.Resolve<IConfiguration>().DefaultLocale)) // REMOVE THIS CUSTOM REGISTRATION
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
            builder.Register(c => new LanguageProvider(c.Resolve<ILogger>(),
                    c.Resolve<IConfiguration>().TranslationsFolder,
                    c.Resolve<IConfiguration>().DefaultLocale)) // REMOVE THIS CUSTOM REGISTRATION
                .As<ILanguageProvider>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<ExpiredSessionHandler>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<ReconnectState>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<SettingsBuilder>().SingleInstance();
            builder.RegisterType<ReconnectManager>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<VpnInfoUpdater>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<PlanDowngradeHandler>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<StreamingServicesUpdater>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<StreamingServices>().As<IStreamingServices>().SingleInstance();
            builder.RegisterType<StreamingServicesFileStorage>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<PartnersService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<PartnersUpdater>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<PartnersFileStorage>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<NotificationSender>().As<INotificationSender>().SingleInstance();
            builder.RegisterType<NotificationUserActionHandler>().As<INotificationUserActionHandler>().SingleInstance();

            builder.RegisterType<Ed25519Asn1KeyGenerator>().As<IEd25519Asn1KeyGenerator>().SingleInstance();
            builder.RegisterType<X25519KeyGenerator>().As<IX25519KeyGenerator>().SingleInstance();
            builder.RegisterType<AuthKeyManager>().As<IAuthKeyManager>().SingleInstance();
            builder.RegisterType<AuthCertificateManager>().As<IAuthCertificateManager>().SingleInstance();
            builder.RegisterType<AuthCredentialManager>().As<IAuthCredentialManager>().SingleInstance();
            builder.RegisterType<SystemTimeValidator>().SingleInstance();
            builder.RegisterType<WelcomeModalManager>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();
            builder.RegisterType<AppExitInvoker>().As<IAppExitInvoker>().SingleInstance();
            builder.RegisterType<PortForwardingManager>().As<IPortForwardingManager>().SingleInstance();
            builder.RegisterType<PortForwardingNotifier>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ApiAvailabilityVerifier>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<PromoCodeManager>().As<IPromoCodeManager>().SingleInstance();
            builder.RegisterType<DeviceInfoProvider>().As<IDeviceInfoProvider>().SingleInstance();
            builder.RegisterType<ApplicationResourcesLoader>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ColorPalette>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<WindowPositionSetter>().As<IWindowPositionSetter>().SingleInstance();
            builder.RegisterType<HumanVerifier>().As<IHumanVerifier>().SingleInstance();
            builder.RegisterType<SubscriptionManager>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<WebAuthenticator>().AsImplementedInterfaces().SingleInstance();
        }
    }
}