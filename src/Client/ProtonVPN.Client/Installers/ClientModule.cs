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
using ProtonVPN.Client.Dispatching;
using ProtonVPN.Client.HumanVerification;
using ProtonVPN.Client.Models.MainWindowActivation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Configuration.Source;

namespace ProtonVPN.Client.Installers;

public class ClientModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Bootstrapper>().As<IBootstrapper>().SingleInstance();

        builder.RegisterType<UIThreadDispatcher>().As<IUIThreadDispatcher>().SingleInstance();

        builder.RegisterType<ThemeSelector>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ViewNavigator>().As<IViewNavigator>().InstancePerDependency();
        builder.RegisterType<MainWindowActivator>().As<IMainWindowActivator>().SingleInstance();
        builder.RegisterType<PageMapper>().As<IPageMapper>().SingleInstance();
        builder.RegisterType<PageNavigator>().As<IPageNavigator>().SingleInstance();
        builder.RegisterType<DialogActivator>().As<IDialogActivator>().SingleInstance();
        builder.RegisterType<HumanVerifier>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<HumanVerificationConfig>().AsImplementedInterfaces().SingleInstance();

        builder.Register(c => new DefaultConfig().Value()).As<IConfiguration>().SingleInstance();
        builder.RegisterType<Urls>().As<IUrls>().SingleInstance();
    }
}