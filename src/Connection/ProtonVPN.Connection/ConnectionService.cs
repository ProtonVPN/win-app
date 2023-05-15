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

using CommunityToolkit.Mvvm.Messaging;
using ProtonVPN.Common.Core.Contracts;
using ProtonVPN.Connection.Contracts;
using ProtonVPN.Connection.Contracts.Enums;
using ProtonVPN.Connection.Contracts.Messages;
using ProtonVPN.Connection.Contracts.Models;
using ProtonVPN.Connection.Contracts.Models.Intents;

namespace ProtonVPN.Connection;

public class ConnectionService : ServiceRecipient, IConnectionService
{
    private readonly SemaphoreSlim _connectionSemaphore;
    private CancellationTokenSource? _connectionCancellationTokenSource = null;

    private ConnectionDetails? _connectionDetails;

    public ConnectionStatus ConnectionStatus { get; private set; }

    public ConnectionService()
    {
        _connectionSemaphore = new SemaphoreSlim(1);
    }

    public async Task ConnectAsync(IConnectionIntent? connectionIntent)
    {
        // Cancel previous connection attempt if it is still ongoing.
        await CancelConnectionAsync();

        _connectionCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await _connectionSemaphore.WaitAsync(_connectionCancellationTokenSource.Token);

            _connectionCancellationTokenSource.Token.ThrowIfCancellationRequested();

            if (connectionIntent == null)
            {
                // Default connection intent is Fastest country, no feature
                connectionIntent = ConnectionIntent.Default;
            }

            _connectionDetails = new ConnectionDetails(connectionIntent);

            SetConnectionStatus(ConnectionStatus.Connecting);

            await Task.Delay(TimeSpan.FromSeconds(2), _connectionCancellationTokenSource.Token);

            SetConnectionStatus(ConnectionStatus.Connected);
        }
        catch (OperationCanceledException)
        {
            _connectionDetails = null;

            SetConnectionStatus(ConnectionStatus.Disconnected);
        }
        finally
        {
            _connectionSemaphore.Release();

            _connectionCancellationTokenSource?.Dispose();
            _connectionCancellationTokenSource = null;
        }
    }

    public Task CancelConnectionAsync()
    {
        if (_connectionCancellationTokenSource != null && !_connectionCancellationTokenSource.IsCancellationRequested)
        {
            _connectionCancellationTokenSource.Cancel();
        }

        return Task.CompletedTask;
    }

    public async Task DisconnectAsync()
    {
        // Cancel previous connection attempt if it is still ongoing.
        await CancelConnectionAsync();

        _connectionDetails = null;

        SetConnectionStatus(ConnectionStatus.Disconnected);
    }

    public ConnectionDetails? GetConnectionDetails()
    {
        return _connectionDetails;
    }

    private void SetConnectionStatus(ConnectionStatus connectionStatus)
    {
        if (ConnectionStatus == connectionStatus)
        {
            return;
        }

        ConnectionStatus = connectionStatus;
        Messenger.Send(new ConnectionStatusChanged(ConnectionStatus));
    }
}