/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.OperatingSystems.Registries.Contracts;
using ProtonVPN.OperatingSystems.Registries.Installers;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;

namespace ProtonVPN.ProcessCommunication.Client
{
    public static class FirstAppInstanceCaller
    {
        public static async Task OpenMainWindowAsync(string args)
        {
            ILogger logger = new NullLogger();
            IRegistryEditor registryEditor = RegistryEditorFactory.Create(logger);
            NamedPipesConnectionFactory namedPipesConnectionFactory = new(registryEditor);
            GrpcClient grpcClient = new(logger, namedPipesConnectionFactory);
            grpcClient.Create();
            IUiController uiController = await grpcClient.GetServiceControllerOrThrowAsync<IUiController>(TimeSpan.FromMinutes(1));
            await SendOpenWindowCommandAsync(uiController, args);
        }

        private static async Task SendOpenWindowCommandAsync(IUiController uiController, string args)
        {
            try
            {
                await uiController.OpenWindow(args);
            }
            catch
            {
            }
        }
    }
}
