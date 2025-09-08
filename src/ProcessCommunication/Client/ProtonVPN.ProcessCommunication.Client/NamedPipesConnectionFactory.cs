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

using System.IO.Pipes;
using System.Security.Principal;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.OperatingSystems.Registries.Contracts;
using ProtonVPN.ProcessCommunication.Common;

namespace ProtonVPN.ProcessCommunication.Client;

public class NamedPipesConnectionFactory : INamedPipesConnectionFactory
{
    private readonly RegistryUri _registryUri = RegistryUri.CreateLocalMachineUri(
        NamedPipeConfiguration.REGISTRY_PATH, NamedPipeConfiguration.REGISTRY_KEY);
    private readonly TimeSpan _minConnectionRetryInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _maxConnectionRetryInterval = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _minRegistryRetryInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _maxRegistryRetryInterval = TimeSpan.FromSeconds(5);
    private readonly IRegistryEditor _registryEditor;

    private TimeSpan? _connectionRetryInterval;
    private TimeSpan? _registryRetryInterval;
    private string? _pipeName;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public NamedPipesConnectionFactory(IRegistryEditor registryEditor)
    {
        _registryEditor = registryEditor;
    }

    public async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext _,
        CancellationToken cancellationToken = default)
    {
        return await InternalConnectAsync(CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _cancellationTokenSource.Token).Token);
    }

    private async Task<Stream> InternalConnectAsync(CancellationToken cancellationToken)
    {
        string pipeName = await GetPipeNameAsync(cancellationToken) ?? throw new ArgumentNullException("PipeName");
        NamedPipeClientStream clientStream = new(
            serverName: ".",
            pipeName: pipeName,
            direction: PipeDirection.InOut,
            options: PipeOptions.WriteThrough | PipeOptions.Asynchronous,
            impersonationLevel: TokenImpersonationLevel.Anonymous);

        try
        {
            _pipeName = pipeName;
            await clientStream.ConnectAsync(cancellationToken).ConfigureAwait(false);

            // No authorization checks are made because user processes without admin permissions such as this Client
            // cannot get the executable path of SYSTEM processes such as our Service

            return clientStream;
        }
        catch
        {
            clientStream?.Dispose();

            _connectionRetryInterval = _connectionRetryInterval is null
                ? _minConnectionRetryInterval
                : TimeSpanExtensions.Min(_connectionRetryInterval.Value * 2, _maxConnectionRetryInterval);
            await Task.Delay(_connectionRetryInterval.Value, cancellationToken);

            throw;
        }
    }

    private async Task<string?> GetPipeNameAsync(CancellationToken cancellationToken)
    {
        _registryRetryInterval = null;
        while (!cancellationToken.IsCancellationRequested)
        {
            string? pipeName = GetPipeName();
            if (!string.IsNullOrWhiteSpace(pipeName))
            {
                return pipeName;
            }

            _registryRetryInterval = _registryRetryInterval is null
                ? _minRegistryRetryInterval
                : TimeSpanExtensions.Min(_registryRetryInterval.Value * 2, _maxRegistryRetryInterval);
            await Task.Delay(_registryRetryInterval.Value, cancellationToken);
            continue;
        }
        return null;
    }

    private string? GetPipeName()
    {
        return _registryEditor.ReadString(_registryUri);
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    public bool HasPipeNameChanged()
    {
        return _pipeName != GetPipeName();
    }
}