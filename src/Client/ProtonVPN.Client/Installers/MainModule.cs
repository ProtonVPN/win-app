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
using ProtonVPN.Client.EventMessaging.Installers;
using ProtonVPN.Client.Localization.Installers;
using ProtonVPN.Client.Logic.Connection.Installers;
using ProtonVPN.Client.Logic.Recents.Installers;
using ProtonVPN.Client.Logic.Services.Installers;
using ProtonVPN.Client.Settings.Installers;
using ProtonVPN.Logging;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Installers;
using ProtonVPN.OperatingSystems.Registries.Installers;
using ProtonVPN.ProcessCommunication.App.Installers;
using ProtonVPN.ProcessCommunication.Installers;

namespace ProtonVPN.Client.Installers;

public class MainModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Default Activation Handler
        builder.RegisterType<DefaultActivationHandler>()
               .As<ActivationHandler<LaunchActivatedEventArgs>>()
               .InstancePerDependency();

        // TODO
#warning This should come from IConfiguration.AppLogDefaultFullFilePath
        const string LOGS_FOLDER_PATH = "Proton/Proton VPN/Logs";
        const Environment.SpecialFolder ROOT_FOLDER = Environment.SpecialFolder.LocalApplicationData;
        string logsFullFolderPath = Path.Combine(Environment.GetFolderPath(ROOT_FOLDER), LOGS_FOLDER_PATH);
        string clientLogsFullFilePath = Path.Combine(logsFullFolderPath, "client-logs.txt");
        builder.Register(c => new LoggerConfiguration(clientLogsFullFilePath))
               .As<ILoggerConfiguration>()
               .SingleInstance();

        // Modules
        builder.RegisterModule<EventMessageReceiverActivationModule>()
               .RegisterModule<LoggingModule>()
               .RegisterModule<RegistriesModule>()
               .RegisterModule<ClientModule>()
               .RegisterModule<ServicesModule>()
               .RegisterModule<ConnectionLogicModule>()
               .RegisterModule<AppProcessCommunicationModule>()
               .RegisterModule<ProcessCommunicationModule>()
               .RegisterModule<LocalizationModule>()
               .RegisterModule<EventMessagingModule>()
               .RegisterModule<ViewModelsModule>()
               .RegisterModule<RecentsLogicModule>()
               .RegisterModule<SettingsModule>();
    }
}