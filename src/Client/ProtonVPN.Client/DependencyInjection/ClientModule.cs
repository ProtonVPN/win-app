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
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Activation;
using ProtonVPN.Client.Localization.Installers;
using ProtonVPN.Client.Logic.Connection.Installers;
using ProtonVPN.Client.Logic.Recents.Installers;

namespace ProtonVPN.Client.DependencyInjection
{
    public class ClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Default Activation Handler
            builder.RegisterType<DefaultActivationHandler>().As<ActivationHandler<LaunchActivatedEventArgs>>().InstancePerDependency();

            // Other Activation Handlers

            // Modules
            builder.RegisterModule<ServicesModule>();
            builder.RegisterModule<LocalizationModule>();
            builder.RegisterModule<ViewModelsModule>();
            builder.RegisterModule<RecentsLogicModule>();
            builder.RegisterModule<ConnectionLogicModule>();
        }
    }
}