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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Common.Channels;
using ProtonVPN.ProcessCommunication.Common.Registration;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;

namespace ProtonVPN.ProcessCommunication.App
{
    public static class FirstAppInstanceCaller
    {
        public static async Task OpenMainWindowAsync()
        {
            AppServerPortRegister appServerPortRegister = new(new NullLogger());
            int? appServerPort = appServerPortRegister.ReadOnce();
            if (appServerPort.HasValue && GrpcChannelWrapperFactory.IsPortValid(appServerPort.Value))
            {
                await CreateGrpcChannelAndSendOpenWindowCommandAsync(appServerPort.Value);
            }
        }

        private static async Task CreateGrpcChannelAndSendOpenWindowCommandAsync(int appServerPort)
        {
            try
            {
                GrpcChannelWrapper grpcChannelWrapper = new(appServerPort);
                IAppController appController = grpcChannelWrapper.CreateService<IAppController>();
                await SendOpenWindowCommandAsync(appController);
                await grpcChannelWrapper.ShutdownAsync();
            }
            catch
            {
            }
        }

        private static async Task SendOpenWindowCommandAsync(IAppController appController)
        {
            try
            {
                await appController.OpenWindow();
            }
            catch
            {
            }
        }
    }
}