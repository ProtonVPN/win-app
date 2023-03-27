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

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppServiceLogs;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;

namespace ProtonVPN.Core.Service
{
    public abstract class ServiceControllerCaller<Controller> where Controller : IServiceController
    {
        private readonly ILogger _logger;
        private readonly IAppGrpcClient _grpcClient;
        private readonly VpnSystemService _vpnSystemService;

        protected ServiceControllerCaller(ILogger logger, IAppGrpcClient grpcClient, VpnSystemService vpnSystemService)
        {
            _logger = logger;
            _grpcClient = grpcClient;
            _vpnSystemService = vpnSystemService;
        }

        protected async Task<T> Invoke<T>(Func<Controller, Task<T>> serviceCall,
            [CallerMemberName] string memberName = "")
        {
            int retryCount = 5;
            while (true)
            {
                try
                {
                    Controller serviceController =
                        await _grpcClient.GetServiceControllerOrThrowAsync<Controller>(TimeSpan.FromSeconds(1));
                    T result = await serviceCall(serviceController);
                    if (result is Task task)
                    {
                        await task;
                    }

                    return result;
                }
                catch (Exception e)
                {
                    await StartServiceIfStoppedAsync();
                    if (retryCount <= 0)
                    {
                        LogError(e, memberName, isToRetry: false);
                        throw;
                    }

                    await _grpcClient.RecreateAsync();
                    LogError(e, memberName, isToRetry: true);
                }

                retryCount--;
            }
        }

        private async Task StartServiceIfStoppedAsync()
        {
            await _vpnSystemService.StartIfStoppedAsync();
        }

        private void LogError(Exception exception, string callerMemberName, bool isToRetry)
        {
            _logger.Error<AppServiceCommunicationFailedLog>(
                $"The invocation of '{callerMemberName}' on VPN Service channel returned an exception and will " +
                (isToRetry ? string.Empty : "not ") +
                $"be retried. Exception message: {exception.Message}");
        }
    }
}