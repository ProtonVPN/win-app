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

using System.Runtime.CompilerServices;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;

namespace ProtonVPN.Client.Logic.Services;

public abstract class ServiceCallerBase
{
    private readonly IAppGrpcClient _grpcClient;
    private readonly IServiceManager _serviceManager;

    protected ILogger Logger { get; }

    protected ServiceCallerBase(ILogger logger,
        IAppGrpcClient grpcClient,
        IServiceManager serviceManager)
    {
        Logger = logger;
        _grpcClient = grpcClient;
        _serviceManager = serviceManager;
    }

    protected async Task<Result<T>> InvokeAsync<T>(Func<IVpnController, Task<T>> serviceCall,
        [CallerMemberName] string memberName = "")
    {
        int retryCount = 5;
        while (true)
        {
            try
            {
                IVpnController serviceController =
                    await _grpcClient.GetServiceControllerOrThrowAsync<IVpnController>(TimeSpan.FromSeconds(1));
                T result = await serviceCall(serviceController);
                if (result is Task task)
                {
                    await task;
                }

                return Result.Ok(result);
            }
            catch (Exception e)
            {
                await StartServiceIfStoppedAsync();
                if (retryCount <= 0)
                {
                    LogError(e, memberName, isToRetry: false);
                    return Result.Fail<T>(e.CombinedMessage());
                }

                await _grpcClient.RecreateAsync();
                LogError(e, memberName, isToRetry: true);
            }

            retryCount--;
        }
    }

    private async Task StartServiceIfStoppedAsync()
    {
        await _serviceManager.StartAsync();
    }

    private void LogError(Exception exception, string callerMemberName, bool isToRetry)
    {
        Logger.Error<AppServiceCommunicationFailedLog>(
            $"The invocation of '{callerMemberName}' on VPN Service channel returned an exception and will " +
            (isToRetry ? string.Empty : "not ") +
            $"be retried. Exception message: {exception.Message}");
    }
}