﻿/*
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

using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc.Server;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.OperatingSystems.Processes.Contracts;
using ProtonVPN.OperatingSystems.Registries.Contracts;
using ProtonVPN.ProcessCommunication.Common;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ILogger = ProtonVPN.Logging.Contracts.ILogger;

namespace ProtonVPN.ProcessCommunication.Server;

public class GrpcServer : IGrpcServer
{
    // AUTHENTICATED_USERS - A group that includes all users whose identities were authenticated
    // when they logged on. Users authenticated as Guest or Anonymous are not members of this group.
    private const string AUTHENTICATED_USERS_SID = "S-1-5-11";

    private readonly RegistryUri _registryUri = RegistryUri.CreateLocalMachineUri(
        NamedPipeConfiguration.REGISTRY_PATH, NamedPipeConfiguration.REGISTRY_KEY);

    private readonly ILogger _logger;
    private readonly IClientController _clientController;
    private readonly IUpdateController _updateController;
    private readonly IVpnController _vpnController;
    private readonly IPipeStreamProcessIdentifier _pipeStreamProcessIdentifier;
    private readonly IConfiguration _config;
    private readonly IHashGenerator _hashGenerator;
    private readonly IRegistryEditor _registryEditor;

    private WebApplication _app;
    private CancellationTokenSource _cancellationTokenSource = new();

    public GrpcServer(ILogger logger,
        IClientController clientController,
        IUpdateController updateController,
        IVpnController vpnController,
        IPipeStreamProcessIdentifier pipeStreamProcessIdentifier,
        IConfiguration configuration,
        IHashGenerator hashGenerator,
        IRegistryEditor registryEditor)
    {
        _logger = logger;
        _clientController = clientController;
        _updateController = updateController;
        _vpnController = vpnController;
        _pipeStreamProcessIdentifier = pipeStreamProcessIdentifier;
        _config = configuration;
        _hashGenerator = hashGenerator;
        _registryEditor = registryEditor;
    }

    public void CreateAndStart()
    {
        if (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            _app = Create();
            _app.RunAsync().ContinueWith(t => RetryAsync()).IgnoreExceptions();
        }
    }

    private async Task RetryAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(3), _cancellationTokenSource.Token);
        CreateAndStart();
    }

    public async Task StopAsync()
    {
        _cancellationTokenSource.Cancel();
        DeletePipeNameFromRegistry();

        // The code below is kept commented to remind why it should not be done. Stopping the gRPC server gracefully
        // leads to the client side Stream to take a long time to detect the server is down. When the gRPC server
        // suddenly dies as in this case or if the service is killed, the client side Stream immediately detects it.
        //await _app?.StopAsync();
    }

    private WebApplication Create()
    {
        DeletePipeNameFromRegistry();
        string pipeName = GeneratePipeName();
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        ConfigureKestrel(builder, pipeName);
        PipeSecurity pipeSecurity = CreatePipeSecurity();
        ConfigureNamedPipes(builder, pipeSecurity);

        builder.Services.AddSingleton<IClientController>(_clientController);
        builder.Services.AddSingleton<IUpdateController>(_updateController);
        builder.Services.AddSingleton<IVpnController>(_vpnController);
        builder.Services.AddCodeFirstGrpc();

        WebApplication app = builder.Build();

        app.MapGrpcService<IClientController>();
        app.MapGrpcService<IUpdateController>();
        app.MapGrpcService<IVpnController>();

        app.Lifetime.ApplicationStarted.Register(() => WritePipeNameToRegistry(pipeName));
        app.Lifetime.ApplicationStopping.Register(DeletePipeNameFromRegistry);

        return app;
    }

    private void ConfigureKestrel(WebApplicationBuilder builder, string pipeName)
    {
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Listen(new NamedPipeEndPoint(pipeName), listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
                listenOptions.Use((ConnectionContext context, Func<Task> next) =>
                {
                    return RequestDelegateAsync(context, next);
                });
            });
        });
    }

    private string GeneratePipeName()
    {
        return $"ProtonVPN-{_hashGenerator.GenerateRandomString(32)}";
    }

    private async Task RequestDelegateAsync(ConnectionContext context, Func<Task> next)
    {
        if (!IsAuthorized(context))
        {
            context.Abort();
        }

        await next();
    }

    private bool IsAuthorized(ConnectionContext context)
    {
        if (context is IConnectionNamedPipeFeature connectionNamedPipeFeature)
        {
            string clientProcessFileName = _pipeStreamProcessIdentifier.GetClientProcessFileName(connectionNamedPipeFeature.NamedPipe) ?? string.Empty;
            string serverProcessFileName = _pipeStreamProcessIdentifier.GetServerProcessFileName(connectionNamedPipeFeature.NamedPipe) ?? string.Empty;

            if (!_config.ServiceExePath.Equals(serverProcessFileName, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Warn<ProcessCommunicationErrorLog>("The owner of the Named Pipe is not this Service. Dispose and recreate.");
                _app.DisposeAsync();
                RetryAsync();
                return false;
            }

            if (_config.ClientExePath.Equals(clientProcessFileName, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            _logger.Warn<ProcessCommunicationErrorLog>($"The connected client is unauthorized ({clientProcessFileName}).");
        }

        return false;
    }

    private PipeSecurity CreatePipeSecurity()
    {
        SecurityIdentifier targetSid = new(AUTHENTICATED_USERS_SID);

        PipeSecurity pipeSecurity = new();
        pipeSecurity.AddAccessRule(
            new PipeAccessRule(
                targetSid,
                PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
                AccessControlType.Allow
            )
        );

        return pipeSecurity;
    }

    private void ConfigureNamedPipes(WebApplicationBuilder builder, PipeSecurity pipeSecurity)
    {
        builder.WebHost.UseNamedPipes(serverOptions =>
        {
            serverOptions.PipeSecurity = pipeSecurity;
            serverOptions.CurrentUserOnly = false;
            serverOptions.MaxWriteBufferSize = 100 * 1024 * 1024; // 100MB
            serverOptions.MaxReadBufferSize = 100 * 1024 * 1024; // 100MB
            serverOptions.ListenerQueueCount = 1;
        });
    }

    private void WritePipeNameToRegistry(string pipeName)
    {
        _registryEditor.WriteString(_registryUri, pipeName);
    }

    private void DeletePipeNameFromRegistry()
    {
        _registryEditor.Delete(_registryUri);
    }
}
