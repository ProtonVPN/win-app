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

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;

namespace ProtonVPN.Vpn.Management;

/// <summary>
/// TCP connection to OpenVPN management interface.
/// </summary>
internal class TcpManagementChannel : IManagementChannel, ITcpManagementChannel
{
    private readonly ILogger _logger;
    private readonly string _host;
    private StreamReader? _streamReader;
    private StreamWriter? _streamWriter;
    private TcpClient? _tcpClient;

    public TcpManagementChannel(ILogger logger, IStaticConfiguration config)
    {
        _logger = logger;
        _host = config.OpenVpn.ManagementHost;
    }

    public async Task Connect(int port)
    {
        Disconnect();

        TcpClient tcpClient = new();

        _logger.Info<ConnectLog>($"Connecting to OpenVPN management interface on {_host}:{port}");
        await tcpClient.ConnectAsync(_host, port);

        _streamReader = new StreamReader(tcpClient.GetStream());
        _streamWriter = new StreamWriter(tcpClient.GetStream())
        {
            AutoFlush = true
        };
        _tcpClient = tcpClient;
    }

    public async Task WriteLineAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            await WriteLineInternalAsync(message, cancellationToken);
        }
        catch (ObjectDisposedException ex)
        {
            throw new IOException("The NetworkStream is closed", ex);
        }
    }

    public async Task<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await ReadLineInternalAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null; // treat as disconnect
        }
        catch (ObjectDisposedException ex)
        {
            throw new IOException("The NetworkStream is closed", ex);
        }
    }

    public void Disconnect()
    {
        if (_tcpClient != null)
        {
            if (_tcpClient.Connected)
            {
                _logger.Info<DisconnectLog>("Disconnecting from OpenVPN management interface");
            }
            _tcpClient.Close();
            SafeDispose(ref _tcpClient);
        }

        SafeDispose(ref _streamReader);
        SafeDispose(ref _streamWriter);
    }

    private async Task WriteLineInternalAsync(string message, CancellationToken cancellationToken)
    {
        StreamWriter? writer = _streamWriter
            ?? throw new IOException("OpenVPN management interface is not connected");

        await writer.WriteLineAsync(message.ToCharArray(), cancellationToken);
    }

    private async Task<string?> ReadLineInternalAsync(CancellationToken cancellationToken)
    {
        StreamReader reader = _streamReader
            ?? throw new IOException("OpenVPN management interface is not connected");

        string? message;
        try
        {
            message = await reader.ReadLineAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null;
        }

        if (message == null)
        {
            _logger.Info<DisconnectLog>("Disconnected from OpenVPN management interface");
        }

        return message;
    }

    private void SafeDispose<T>(ref T? disposable) where T: class, IDisposable
    {
        T? item = disposable;
        disposable = null;
        try
        {
            item?.Dispose();
        }
        catch (InvalidOperationException ex)
        {
            _logger.Error<DisconnectLog>($"Failed to disconnect from OpenVPN management interface: {ex.Message}");
        }
    }
}