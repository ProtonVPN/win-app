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

using System.IO.Pipes;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.OperatingSystems.Processes.Contracts;
using ProtonVPN.OperatingSystems.Registries.Contracts;
using ProtonVPN.ProcessCommunication.Common;

namespace ProtonVPN.ProcessCommunication.Server;

public class NamedPipeAuthorizationMiddleware
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly IIssueReporter _issueReporter;
    private readonly IConfiguration _config;
    private readonly IPipeStreamProcessIdentifier _pipeStreamProcessIdentifier;
    private readonly IRegistryEditor _registryEditor;
    private readonly Func<Task> _recreateAndStartAsyncFunc;
    private readonly RegistryUri _registryUri;

    private const string INSTALLED_FOLDER_VERSION_PATTERN = @"\\v(\d+\.\d+\.\d+)\\";
    private readonly Regex _installedFolderVersionRegex = new(INSTALLED_FOLDER_VERSION_PATTERN);

    private event EventHandler _invokingServiceStop;
    private bool _wasServiceStopRequested;

    public NamedPipeAuthorizationMiddleware(RequestDelegate next,
        ILogger logger,
        IIssueReporter issueReporter,
        IConfiguration config,
        IPipeStreamProcessIdentifier pipeStreamProcessIdentifier,
        IRegistryEditor registryEditor,
        EventHandler invokingServiceStop,
        Func<Task> recreateAndStartAsyncFunction)
    {
        _next = next;
        _logger = logger;
        _issueReporter = issueReporter;
        _config = config;
        _pipeStreamProcessIdentifier = pipeStreamProcessIdentifier;
        _registryEditor = registryEditor;
        _recreateAndStartAsyncFunc = recreateAndStartAsyncFunction;

        _invokingServiceStop = invokingServiceStop;

        _registryUri = RegistryUri.CreateLocalMachineUri($@"SYSTEM\CurrentControlSet\Services\{_config.ServiceName}", "ImagePath");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        AuthorizationResult authorizationResult = await AuthorizeAsync(context);

        if (authorizationResult.StatusCode > 0)
        {
            context.Response.StatusCode = authorizationResult.StatusCode;

            string installedServicePath = Path.GetFullPath(_registryEditor.ReadString(_registryUri));

            string clientProcessVersionString = GetVersionFromFilePath(authorizationResult.ClientProcessFileName);
            string serviceProcessVersionString = GetVersionFromFilePath(authorizationResult.ServerProcessFileName);
            string installedServiceVersionString = GetVersionFromFilePath(installedServicePath);

            if (Version.TryParse(clientProcessVersionString, out Version clientProcessVersion) &&
                Version.TryParse(serviceProcessVersionString, out Version serviceProcessVersion) &&
                clientProcessVersion != serviceProcessVersion &&
                Version.TryParse(installedServiceVersionString, out Version installedServiceVersion) &&
                installedServiceVersion > serviceProcessVersion)
            {
                string serviceStopDescription = 
                    $"Client Process Path: '{authorizationResult.ClientProcessFileName}' Version '{clientProcessVersionString}', " +
                    $"Service Process Path: '{authorizationResult.ServerProcessFileName}' Version '{serviceProcessVersionString}', " +
                    $"Installed Service Path: '{installedServicePath}' Version '{installedServiceVersionString}'";
                await StopServiceAsync(serviceStopDescription);
                context.Response.StatusCode = StatusCodes.Status409Conflict;
            }

            _logger.Error<ProcessCommunicationErrorLog>($"Sending HTTP status code {context.Response.StatusCode} to gRPC client. " +
                $"Client Process Path: '{authorizationResult.ClientProcessFileName}' Version '{clientProcessVersionString}', " +
                $"Service Process Path: '{authorizationResult.ServerProcessFileName}' Version '{serviceProcessVersionString}', " +
                $"Installed Service Path: '{installedServicePath}' Version '{installedServiceVersionString}'");

            context.Response.Headers[HttpConfiguration.CLIENT_PROCESS_PATH] = authorizationResult.ClientProcessFileName;
            context.Response.Headers[HttpConfiguration.SERVICE_PROCESS_PATH] = authorizationResult.ServerProcessFileName;
            context.Response.Headers[HttpConfiguration.INSTALLED_SERVICE_PATH] = installedServicePath;

            context.Response.Headers[HttpConfiguration.CLIENT_PROCESS_VERSION] = clientProcessVersionString;
            context.Response.Headers[HttpConfiguration.SERVICE_PROCESS_VERSION] = serviceProcessVersionString;
            context.Response.Headers[HttpConfiguration.INSTALLED_SERVICE_VERSION] = installedServiceVersionString;
        }
        else
        {
            await _next(context);
        }
    }

    private async Task StopServiceAsync(string serviceStopDescription)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_wasServiceStopRequested)
            {
                _logger.Warn<ProcessCommunicationErrorLog>(
                    "Not requesting service to stop (due to version mismatch) as it was already requested.");
            }
            else
            {
                const string serviceStopTitle = "Stopping the service due to a version mismatch between the " +
                    "service and client processes when a more recent version of the service is installed.";
                _logger.Warn<ProcessCommunicationErrorLog>($"{serviceStopTitle} {serviceStopDescription}");
                _issueReporter.CaptureMessage(serviceStopTitle, serviceStopDescription);

                _invokingServiceStop?.Invoke(this, EventArgs.Empty);
                _wasServiceStopRequested = true;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private string GetVersionFromFilePath(string filePath)
    {
        MatchCollection matches = _installedFolderVersionRegex.Matches(filePath);
        if (matches.Count > 0)
        {
            Match lastMatch = matches[matches.Count - 1];
            return lastMatch.Groups[1].Value;
        }
        return string.Empty;
    }

    private async Task<AuthorizationResult> AuthorizeAsync(HttpContext context)
    {
        try
        {
            NamedPipeServerStream namedPipe = GetNamedPipe(context);

            string clientProcessPath = _pipeStreamProcessIdentifier.GetClientProcessFullFilePath(namedPipe) ?? string.Empty;
            string serverProcessPath = _pipeStreamProcessIdentifier.GetServerProcessFullFilePath(namedPipe) ?? string.Empty;

            string expectedServiceProcessPath = Path.GetFullPath(_config.ServiceExePath);
            if (!expectedServiceProcessPath.Equals(serverProcessPath, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Warn<ProcessCommunicationErrorLog>($"The owner of the Named Pipe is not this Service. Dispose and recreate.");
                await _recreateAndStartAsyncFunc();
                return AuthorizationResult.Error(StatusCodes.Status409Conflict, clientProcessPath, serverProcessPath);
            }

            string expectedClientProcessPath = Path.GetFullPath(_config.ClientExePath);
            if (expectedClientProcessPath.Equals(clientProcessPath, StringComparison.InvariantCultureIgnoreCase))
            {
                return AuthorizationResult.Ok();
            }

            _logger.Warn<ProcessCommunicationErrorLog>($"The connected client is unauthorized. " +
                $"Client path: '{clientProcessPath}'. Expected: '{expectedClientProcessPath}'.");
            return AuthorizationResult.Error(StatusCodes.Status401Unauthorized, clientProcessPath, serverProcessPath);
        }
        catch (Exception ex)
        {
            _logger.Error<ProcessCommunicationErrorLog>("An exception was thrown when checking if the gRPC client is authorized.", ex);
            _issueReporter.CaptureError(ex);
            return AuthorizationResult.Error(StatusCodes.Status500InternalServerError);
        }
    }

    // The properties needed are under internal types and therefore can only be obtained through reflection
    private NamedPipeServerStream GetNamedPipe(HttpContext context)
    {
        Type http2StreamType = Type.GetType("Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http2.Http2Stream, Microsoft.AspNetCore.Server.Kestrel.Core");
        Type namedPipesConnectionType = Type.GetType("Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes.Internal.NamedPipeConnection, Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes");

        PropertyInfo connectionContextProperty = http2StreamType.GetProperty("ConnectionContext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        object connectionContext = connectionContextProperty.GetValue(context.Features);

        PropertyInfo namedPipeProperty = namedPipesConnectionType.GetProperty("NamedPipe", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        NamedPipeServerStream namedPipe = (NamedPipeServerStream)namedPipeProperty.GetValue(connectionContext);

        return namedPipe;
    }
}
