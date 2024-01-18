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
using ProtonVPN.Api.Installers;
using ProtonVPN.Client.EventMessaging.Installers;
using ProtonVPN.Client.Localization.Installers;
using ProtonVPN.Client.Logic.Auth.Installers;
using ProtonVPN.Client.Logic.Connection.Installers;
using ProtonVPN.Client.Logic.Feedback.Installers;
using ProtonVPN.Client.Logic.Recents.Installers;
using ProtonVPN.Client.Logic.Servers.Installers;
using ProtonVPN.Client.Logic.Services.Installers;
using ProtonVPN.Client.Logic.Updates.Installers;
using ProtonVPN.Client.Notifications.Installers;
using ProtonVPN.Client.Settings.Installers;
using ProtonVPN.Configurations.Installers;
using ProtonVPN.Crypto.Installers;
using ProtonVPN.Dns.Installers;
using ProtonVPN.EntityMapping.Installers;
using ProtonVPN.IssueReporting.Installers;
using ProtonVPN.Logging.Installers;
using ProtonVPN.OperatingSystems.Processes.Installers;
using ProtonVPN.OperatingSystems.Registries.Installers;
using ProtonVPN.OperatingSystems.Services.Installers;
using ProtonVPN.ProcessCommunication.App.Installers;
using ProtonVPN.ProcessCommunication.Installers;
using ProtonVPN.Serialization.Installers;

namespace ProtonVPN.Client.Installers;

public class MainModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterLoggerConfiguration(c => c.ClientLogsFilePath);

        RegisterModules(builder);
    }

    private void RegisterModules(ContainerBuilder builder)
    {
        builder.RegisterModule<EventMessageReceiverActivationModule>()
               .RegisterModule<LoggingModule>()
               .RegisterModule<RegistriesModule>()
               .RegisterModule<ProcessesModule>()
               .RegisterModule<ServicesModule>()
               .RegisterModule<ClientModule>()
               .RegisterModule<ServicesLogicModule>()
               .RegisterModule<ConnectionLogicModule>()
               .RegisterModule<AppProcessCommunicationModule>()
               .RegisterModule<EntityMappingModule>()
               .RegisterModule<ProcessCommunicationModule>()
               .RegisterModule<LocalizationModule>()
               .RegisterModule<EventMessagingModule>()
               .RegisterModule<ViewModelsModule>()
               .RegisterModule<RecentsLogicModule>()
               .RegisterModule<ServersLogicModule>()
               .RegisterModule<ConfigurationsModule>()
               .RegisterModule<ApiModule>()
               .RegisterModule<SettingsModule>()
               .RegisterModule<AuthLogicModule>()
               .RegisterModule<CryptoModule>()
               .RegisterModule<FeedbackLogicModule>()
               .RegisterModule<DnsModule>()
               .RegisterModule<SerializationModule>()
               .RegisterModule<UpdatesLogicModule>()
               .RegisterModule<IssueReportingModule>()
               .RegisterModule<NotificationsModule>();
    }
}