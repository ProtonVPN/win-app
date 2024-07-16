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
using ProtonVPN.Client.Bootstrapping;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Contracts;
using ProtonVPN.Client.Dispatching;
using ProtonVPN.Client.Handlers;
using ProtonVPN.Client.HumanVerification;
using ProtonVPN.Client.Logic.Announcements;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Activation.Custom;
using ProtonVPN.Client.Models.Clipboards;
using ProtonVPN.Client.Models.Icons;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.UI.Connections.Common.Factories;
using ProtonVPN.Client.UI.Settings.Pages.About;
using ProtonVPN.Common.Legacy.OS.DeviceIds;
using ProtonVPN.Common.Legacy.OS.Net.NetworkInterface;
using ProtonVPN.Common.Legacy.OS.Processes;
using ProtonVPN.Common.Legacy.OS.Systems;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Installers;

public class ClientModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Bootstrapper>().As<IBootstrapper>().SingleInstance();

        builder.RegisterType<UIThreadDispatcher>().As<IUIThreadDispatcher>().SingleInstance();

        builder.RegisterType<ThemeSelector>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MainWindowActivator>().As<IMainWindowActivator>().SingleInstance();
        builder.RegisterType<ViewMapper>().As<IViewMapper>().SingleInstance();
        builder.RegisterType<MainViewNavigator>().As<IMainViewNavigator>().SingleInstance();
        builder.RegisterType<LoginViewNavigator>().As<ILoginViewNavigator>().SingleInstance();
        builder.RegisterType<ReportIssueViewNavigator>().As<IReportIssueViewNavigator>().SingleInstance();
        builder.RegisterType<UpsellCarouselViewNavigator>().As<IUpsellCarouselViewNavigator>().SingleInstance();
        builder.RegisterType<DialogActivator>().As<IDialogActivator>().SingleInstance();
        builder.RegisterType<ReportIssueDialogActivator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UpsellCarouselDialogActivator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<OverlayActivator>().As<IOverlayActivator>().SingleInstance();
        builder.RegisterType<HumanVerifier>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<HumanVerificationConfig>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<Urls>().As<IUrls>().SingleInstance();
        builder.RegisterType<ClipboardEditor>().As<IClipboardEditor>().SingleInstance();
        builder.RegisterType<ApplicationIconSelector>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AnnouncementActivator>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<DeviceIdCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SystemState>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SystemProcesses>().As<IOsProcesses>().SingleInstance();

        builder.RegisterType<ReleaseViewModelFactory>().SingleInstance();
        builder.RegisterType<LocationItemFactory>().SingleInstance();

        RegisterHandlers(builder);

        builder.Register(c =>
            new SafeSystemNetworkInterfaces(c.Resolve<ILogger>(), new SystemNetworkInterfaces()))
            .As<INetworkInterfaces>().SingleInstance(); // VPNWIN-2108 - Remove this custom manual registration
    }

    private void RegisterHandlers(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(PortForwardingNotificationHandler).Assembly)
               .Where(t => typeof(IHandler).IsAssignableFrom(t))
               .AsImplementedInterfaces()
               .SingleInstance()
               .AutoActivate();
    }
}