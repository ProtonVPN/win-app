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
using ProtonVPN.Common.Logging;
using ProtonVPN.Update.Files.Launchable;
using ProtonVPN.Update.Updates;

namespace ProtonVPN.Update.Config
{
    /// <summary>
    /// Initializes Update module and registers public interfaces.
    /// </summary>
    public class Module
    {
        public void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppUpdates>().SingleInstance();
            builder.RegisterType<LaunchableFile>().As<ILaunchableFile>().SingleInstance();

            builder.Register(c =>
                new CleanableOnceAppUpdates(
                    new AsyncAppUpdates(
                        new SafeAppUpdates(c.Resolve<ILogger>(),
                            c.Resolve<AppUpdates>())
                    ))).As<IAppUpdates>().SingleInstance();

            builder.Register(c =>
                new SafeAppUpdate(c.Resolve<ILogger>(),
                    new ExtendedProgressAppUpdate(c.Resolve<IAppUpdateConfig>().MinProgressDuration,
                        new NotifyingAppUpdate(
                            new AppUpdate(c.Resolve<AppUpdates>()), c.Resolve<ILogger>()
                        )))).As<INotifyingAppUpdate>().SingleInstance();
        }
    }
}