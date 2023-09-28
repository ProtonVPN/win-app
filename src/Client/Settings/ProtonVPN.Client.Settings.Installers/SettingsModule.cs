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
using ProtonVPN.Client.Settings.Repositories;

namespace ProtonVPN.Client.Settings.Installers;

public class SettingsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Settings>().As<ISettings>().SingleInstance();
        builder.RegisterType<GlobalSettings>().As<IGlobalSettings>().SingleInstance();

        builder.RegisterType<SettingsFileManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<GlobalSettingsRepository>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UserSettingsRepository>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<SettingsRestorer>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ClientConfigObserver>().AsImplementedInterfaces().AutoActivate().SingleInstance();

        builder.RegisterType<SettingsConflictResolver>().AsImplementedInterfaces().AutoActivate().SingleInstance();

        builder.RegisterAssemblyTypes(typeof(ISettingsConflict).Assembly)
               .Where(t => typeof(ISettingsConflict).IsAssignableFrom(t))
               .AsImplementedInterfaces()
               .SingleInstance();
    }
}