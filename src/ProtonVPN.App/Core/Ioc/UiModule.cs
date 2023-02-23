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
using Caliburn.Micro;
using ProtonVPN.ConnectionInfo;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Login.Views;
using ProtonVPN.QuickLaunch;
using ProtonVPN.ViewModels;
using ProtonVPN.Windows;
using ProtonVPN.Windows.Popups.DeveloperTools;
using QuickLaunchWindow = ProtonVPN.QuickLaunch.QuickLaunchWindow;

namespace ProtonVPN.Core.Ioc
{
    public class UiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<AppWindow>()
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<LoginWindow>()
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<QuickLaunchWindow>()
                .AsImplementedInterfaces()
                .OnActivated(c => { c.Instance.DataContext = c.Context.Resolve<QuickLaunchViewModel>(); })
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<TrayContextMenu>()
                .AsImplementedInterfaces()
                .OnActivated(c => { c.Instance.DataContext = c.Context.Resolve<TrayNotificationViewModel>(); })
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ConnectionErrorResolver>().AsImplementedInterfaces().AsSelf().SingleInstance();

            builder.RegisterAssemblyTypes(typeof(App).Assembly)
                .Where(t => t.Name.EndsWith("ViewModel") && t.Name != nameof(DeveloperToolsPopupViewModel))
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<DeveloperToolsPopupViewModel>()
                .As<IVpnStateAware>()
                .As<ISettingsAware>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<TrayIcon>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<TrayIconMouse>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<BalloonNotification>().SingleInstance();
        }
    }
}
