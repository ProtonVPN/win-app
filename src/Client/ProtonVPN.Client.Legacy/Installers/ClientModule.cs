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
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Legacy.Bootstrapping;
using ProtonVPN.Client.Legacy.Contracts;
using ProtonVPN.Client.Legacy.Dispatching;
using ProtonVPN.Client.Legacy.Handlers;
using ProtonVPN.Client.Legacy.HumanVerification;
using ProtonVPN.Client.Legacy.Models.Activation;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Legacy.Models.Announcements;
using ProtonVPN.Client.Legacy.Models.Clipboards;
using ProtonVPN.Client.Legacy.Models.Icons;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.Models.Themes;
using ProtonVPN.Client.Legacy.Models.Urls;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Factories;
using ProtonVPN.Client.Legacy.UI.Settings.Pages.About;
using ProtonVPN.Common.Legacy.OS.DeviceIds;
using ProtonVPN.Common.Legacy.OS.Net.NetworkInterface;
using ProtonVPN.Common.Legacy.OS.Processes;
using ProtonVPN.Common.Legacy.OS.Systems;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.Installers;

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
        builder.RegisterType<ProfileItemFactory>().SingleInstance();
        builder.RegisterType<CommonItemFactory>().SingleInstance();

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