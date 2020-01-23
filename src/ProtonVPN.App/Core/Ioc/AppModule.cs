/*
 * Copyright (c) 2020 Proton Technologies AG
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
using Autofac.Core;
using Caliburn.Micro;
using ProtonVPN.About;
using ProtonVPN.Account;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Config;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Profiles.Cached;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Service;
using ProtonVPN.Core.Service.Settings;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Settings.Migrations;
using ProtonVPN.Core.Startup;
using ProtonVPN.Core.Storage;
using ProtonVPN.Map;
using ProtonVPN.Modals;
using ProtonVPN.Modals.Dialogs;
using ProtonVPN.Notifications;
using ProtonVPN.Servers;
using ProtonVPN.Settings.SplitTunneling;
using ProtonVPN.Sidebar;
using ProtonVPN.Vpn;
using ProtonVPN.Vpn.Connectors;
using System;
using System.Windows;

namespace ProtonVPN.Core.Ioc
{
    public class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(c => new ConfigFactory().Config());

            builder.RegisterType<SidebarManager>().SingleInstance();
            builder.RegisterType<PricingBuilder>().SingleInstance();
            builder.RegisterType<UpdateViewModel>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<VpnConnectionSpeed>().AsImplementedInterfaces().AsSelf().SingleInstance();

            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            builder.RegisterType<ServersFileStorage>().WithParameter(new ResolvedParameter(
                (pi, ctx) => pi.ParameterType == typeof(string) && pi.Name == "cacheFile",
                (pi, ctx) => ctx.Resolve<Common.Configuration.Config>().ServersJsonCacheFilePath)).SingleInstance();

            builder.RegisterType<CachedServersProvider>().SingleInstance();
            builder.RegisterType<ServersUpdater>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<Bootstrapper>().SingleInstance();
            builder.RegisterType<UserStorage>().As<IUserStorage>().SingleInstance();
            builder.RegisterType<ServerCandidatesFactory>().SingleInstance();
            builder.RegisterType<PinFactory>()
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<SortedCountries>().SingleInstance();
            builder.RegisterType<ServerListFactory>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterInstance((App) Application.Current).SingleInstance();
            builder.RegisterType<VpnService>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ModalWindows>().As<IModalWindows>().SingleInstance();
            builder.RegisterType<ProtonVPN.Modals.Modals>().As<IModals>().SingleInstance();
            builder.RegisterType<Dialogs>().As<IDialogs>().SingleInstance();
            builder.RegisterType<AutoStartup>().As<IAutoStartup>().SingleInstance();
            builder.RegisterType<SyncableAutoStartup>().As<ISyncableAutoStartup>().SingleInstance();
            builder.RegisterType<SyncedAutoStartup>().AsSelf().As<ISettingsAware>().SingleInstance();

            builder.RegisterType<AppSettingsStorage>().SingleInstance();
            builder.Register(c => 
                    new EnumerableAsJsonSettings(
                        new CachedSettings(
                            new EnumAsStringSettings(
                                new SelfRepairingSettings(
                                    c.Resolve<AppSettingsStorage>())))))
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
            builder.RegisterType<Settings.Migrations.v1_7_2.AppSettingsMigration>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<Settings.Migrations.v1_7_2.UserSettingsMigration>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<Settings.Migrations.v1_8_0.AppSettingsMigration>().AsImplementedInterfaces().SingleInstance();
            builder.Register(c => new Settings.Migrations.v1_8_0.UserSettingsMigration(
                    c.Resolve<ISettingsStorage>(),
                    c.Resolve<UserSettings>()))
                .As<IUserSettingsMigration>()
                .SingleInstance();
            builder.RegisterType<Settings.Migrations.v1_10_0.AppSettingsMigration>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<MapLineManager>().SingleInstance();
            builder.RegisterType<VpnEvents>();
            builder.RegisterType<SettingsServiceClientManager>().SingleInstance();
            builder.RegisterType<SettingsServiceClient>().SingleInstance();
            builder.RegisterType<ServiceChannelFactory>().SingleInstance();
            builder.RegisterType<SettingsContractProvider>().SingleInstance();
            builder.RegisterType<AutoConnect>().SingleInstance();
            builder.RegisterInstance(Properties.Settings.Default);

            builder.Register(c =>
                new VpnServiceWrapper(
                    c.Resolve<ServiceRetryPolicy>().Value(),
                        new SafeService(
                            new LoggingService(
                                c.Resolve<ILogger>(),
                                new SystemService(
                                    c.Resolve<Common.Configuration.Config>().ServiceName)))))
                .SingleInstance();

            builder.Register(c =>
                    new AppUpdateServiceWrapper(
                        c.Resolve<ServiceRetryPolicy>().Value(),
                            new SafeService(
                                new LoggingService(
                                    c.Resolve<ILogger>(),
                                    new SystemService(
                                        c.Resolve<Common.Configuration.Config>().UpdateServiceName)))))
                .SingleInstance();

            builder.Register(c => new ServiceRetryPolicy(2, TimeSpan.FromSeconds(1))).SingleInstance();

            builder.RegisterType<VpnServiceManager>().SingleInstance();
            builder.Register(c => new ServiceStartDecorator(
                c.Resolve<ILogger>(),
                c.Resolve<VpnServiceManager>(),
                c.Resolve<IModals>()))
                .As<IVpnServiceManager>().SingleInstance();
            builder.RegisterType<VpnManager>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<ServerConnector>().SingleInstance();
            builder.RegisterType<ProfileConnector>().SingleInstance();
            builder.RegisterType<CountryConnector>().SingleInstance();
            builder.RegisterType<DisconnectError>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<VpnStateNotification>()
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<QuickConnector>().SingleInstance();
            builder.RegisterType<AppExitHandler>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<UserLocationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<VpnInfoChecker>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<InstalledApps>().SingleInstance();
            builder.RegisterType<Onboarding.Onboarding>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<SystemNotification>().AsImplementedInterfaces().SingleInstance();
            builder.Register(c => new VpnConfig(
                    c.Resolve<IApiClient>(),
                    c.Resolve<Common.Configuration.Config>().DefaultOpenVpnTcpPorts,
                    c.Resolve<Common.Configuration.Config>().DefaultOpenVpnUdpPorts,
                    c.Resolve<Common.Configuration.Config>().DefaultBlackHoleIps))
                .As<IVpnConfig>()
                .SingleInstance();
            builder.RegisterType<MonitoredVpnService>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<Lang.Language>().AsImplementedInterfaces().SingleInstance();
            builder.Register(c => new UpdateNotification(
                    c.Resolve<Common.Configuration.Config>().UpdateRemindInterval,
                    c.Resolve<ISystemNotification>(),
                    c.Resolve<IModals>()))
                .AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<InsecureNetworkNotification>().AsImplementedInterfaces().AsSelf().SingleInstance();
        }
    }
}
