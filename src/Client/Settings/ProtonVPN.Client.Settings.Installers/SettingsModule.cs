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
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Conflicts.Bases;
using ProtonVPN.Client.Settings.Files;
using ProtonVPN.Client.Settings.Migrations;
using ProtonVPN.Client.Settings.Observers;
using ProtonVPN.Client.Settings.Repositories;
using ProtonVPN.Client.Settings.RequiredReconnections;

namespace ProtonVPN.Client.Settings.Installers;

public class SettingsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Settings>().As<ISettings>().SingleInstance();
        builder.RegisterType<GlobalSettings>().As<IGlobalSettings>().SingleInstance();
        builder.RegisterType<SessionSettings>().As<ISessionSettings>().SingleInstance();

        builder.RegisterType<UserSettingsFileReaderWriter>().As<IUserSettingsFileReaderWriter>().SingleInstance();
        builder.RegisterType<GlobalSettingsFileReaderWriter>().As<IGlobalSettingsFileReaderWriter>().SingleInstance();
        builder.RegisterType<GlobalSettingsCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UserSettingsCache>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<SettingsRestorer>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<GlobalSettingsMigrator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UserSettingsMigrator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ProfilesMigrator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SettingsCorrector>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ClientConfigObserver>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<FeatureFlagsObserver>().AsImplementedInterfaces().AutoActivate().SingleInstance();

        builder.RegisterType<SettingsConflictResolver>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<RequiredReconnectionSettings>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterAssemblyTypes(typeof(ISettingsConflict).Assembly)
               .Where(t => typeof(ISettingsConflict).IsAssignableFrom(t))
               .AsImplementedInterfaces()
               .SingleInstance();
    }
}