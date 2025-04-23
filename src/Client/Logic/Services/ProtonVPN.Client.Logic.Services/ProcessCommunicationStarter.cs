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

using ProtonVPN.Client.Contracts.Services.Lifecycle;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.ProcessCommunication.Contracts;

namespace ProtonVPN.Client.Logic.Services;

public class ProcessCommunicationStarter : IProcessCommunicationStarter
{
    private readonly IGrpcClient _grpcClient;
    private readonly ILogger _logger;
    private readonly IClientControllerListener _clientControllerListener;
    private readonly IAppExitInvoker _appExitInvoker;

    public ProcessCommunicationStarter(IGrpcClient grpcClient,
        ILogger logger,
        IClientControllerListener clientControllerListener,
        IEnumerable<IServiceCaller> serviceCallers,
        IAppExitInvoker appExitInvoker)
    {
        _grpcClient = grpcClient;
        _logger = logger;
        _clientControllerListener = clientControllerListener;
        _appExitInvoker = appExitInvoker;

        _grpcClient.InvokingClientRestart += OnInvokingClientRestartAsync;
    }

    private async void OnInvokingClientRestartAsync(object? sender, EventArgs e)
    {
        await _appExitInvoker.RestartAsync();
    }

    public void Start()
    {
        try
        {
            _grpcClient.Create();
            _clientControllerListener.Start();
        }
        catch (Exception e)
        {
            _logger.Error<AppServiceStartFailedLog>("An error occurred when starting the gRPC client.", e);
            throw;
        }
    }
}