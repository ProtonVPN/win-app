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

using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;

namespace ProtonVPN.ProcessCommunication.Client;

public class GrpcClient : IGrpcClient
{
    private readonly INamedPipesConnectionFactory _namedPipesConnectionFactory;
    private readonly ILogger _logger;
    private readonly IIssueReporter _issueReporter;
    private readonly ISettings _settings;

    private readonly object _lock = new();
    private GrpcChannel _channel;

    public IClientController ClientController { get; private set; }
    public IUpdateController UpdateController { get; private set; }
    public IVpnController VpnController { get; private set; }

    public event EventHandler InvokingClientRestart;

    public GrpcClient(INamedPipesConnectionFactory namedPipesConnectionFactory,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings)
    {
        _namedPipesConnectionFactory = namedPipesConnectionFactory;
        _logger = logger;
        _issueReporter = issueReporter;
        _settings = settings;
    }

    public void Stop()
    {
        _namedPipesConnectionFactory.Stop();
    }

    public void CreateIfPipeNameChanged()
    {
        if (_namedPipesConnectionFactory.HasPipeNameChanged())
        {
            Create();
        }
    }

    public void Create()
    {
        lock (_lock)
        {
            _channel?.Dispose();

            _channel = CreateChannel();

            ClientController = _channel.CreateGrpcService<IClientController>();
            UpdateController = _channel.CreateGrpcService<IUpdateController>();
            VpnController = _channel.CreateGrpcService<IVpnController>();
        }
    }

    private GrpcChannel CreateChannel()
    {
        SocketsHttpHandler socketsHttpHandler = new()
        {
            ConnectCallback = _namedPipesConnectionFactory.ConnectAsync,
            UseProxy = false,
            Proxy = null
        };

        return GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpHandler = new ResponseHandler(_logger, _issueReporter, _settings, InvokingClientRestart, socketsHttpHandler)
        });
    }

    public async Task<T> GetServiceControllerOrThrowAsync<T>(TimeSpan timeout) where T : IServiceController
    {
        CancellationTokenSource cts = new(timeout);
        IServiceController serviceController = GetServiceController<T>();

        try
        {
            while (serviceController is null && !cts.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200), cts.Token);
                serviceController = GetServiceController<T>();
            }
        }
        catch
        {
        }

        if (serviceController is null)
        {
            string errorMessage = $"Failed to get the Service Controller within the allotted time '{timeout}'.";
            _logger.Error<ProcessCommunicationErrorLog>(errorMessage);
            throw new TimeoutException(errorMessage);
        }

        return (T)serviceController;
    }

    public IServiceController GetServiceController<T>()
    {
        if (typeof(T).IsAssignableFrom(typeof(IVpnController)))
        {
            return VpnController;
        }

        if (typeof(T).IsAssignableFrom(typeof(IUpdateController)))
        {
            return UpdateController;
        }

        throw new NotImplementedException($"Controller of type {typeof(T)} is not supported.");
    }
}
