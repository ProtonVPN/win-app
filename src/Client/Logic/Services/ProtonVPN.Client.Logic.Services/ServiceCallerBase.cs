/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Client.Contracts.ProcessCommunication;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;

namespace ProtonVPN.Client.Logic.Services;

public abstract class ServiceCallerBase<TController> : IServiceCaller
    where TController : IServiceController
{
    private static readonly TimeSpan _callTimeout = TimeSpan.FromSeconds(3);

    private readonly IGrpcClient _grpcClient;
    private readonly Lazy<IServiceCommunicationErrorHandler> _serviceCommunicationErrorHandler;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    protected ILogger Logger { get; }

    protected ServiceCallerBase(ILogger logger,
        IGrpcClient grpcClient,
        Lazy<IServiceCommunicationErrorHandler> serviceCommunicationErrorHandler)
    {
        Logger = logger;
        _grpcClient = grpcClient;
        _serviceCommunicationErrorHandler = serviceCommunicationErrorHandler;
    }

    protected async Task<Result<T>> InvokeAsync<T>(Func<TController, CancellationToken, Task<T>> serviceCall,
        [CallerMemberName] string memberName = "")
    {
        int retryCount = 5;
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                TController serviceController =
                    await _grpcClient.GetServiceControllerOrThrowAsync<TController>(TimeSpan.FromSeconds(1));
                CancellationTokenSource cancellationTokenSource = new(_callTimeout);
                T result = await serviceCall(serviceController, cancellationTokenSource.Token);
                if (result is Task task)
                {
                    await task;
                }

                return Result.Ok(result);
            }
            catch (Exception e)
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    await _serviceCommunicationErrorHandler.Value.HandleAsync();
                }

                if (_cancellationTokenSource.IsCancellationRequested || retryCount <= 0)
                {
                    LogError(e, memberName, isToRetry: false);
                    return Result.Fail<T>(e.CombinedMessage());
                }

                LogError(e, memberName, isToRetry: true);
            }

            retryCount--;
        }

        return Result.Fail<T>($"Can't send message because the {GetType().Name} has been stopped.");
    }

    private void LogError(Exception exception, string callerMemberName, bool isToRetry)
    {
        Logger.Error<AppServiceCommunicationFailedLog>(
            $"The invocation of '{callerMemberName}' on VPN Service channel returned an exception and will " +
            (isToRetry ? string.Empty : "not ") +
            $"be retried. Exception message: {exception.Message}");
    }

    public void Stop()
    {
        Logger.Info<ProcessCommunicationLog>($"The {GetType().Name} has been stopped.");
        _cancellationTokenSource.Cancel();
    }
}