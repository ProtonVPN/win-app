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

using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.ProtocolLogs;

namespace ProtonVPN.Vpn.Management;

/// <summary>
/// Messaging wrapper over <see cref="IManagementChannel"/>. 
/// </summary>
internal class MessagingManagementChannel : IMessagingManagementChannel
{
    private readonly ILogger _logger;
    private readonly IConcurrentManagementChannel _managementChannel;

    public MessagingManagementChannel(ILogger logger, IConcurrentManagementChannel managementChannel)
    {
        _logger = logger;
        _managementChannel = managementChannel;

        Messages = new();
    }

    public ManagementMessages Messages { get; }

    public async Task ConnectAsync(int port, string password, CancellationToken cancellationToken)
    {
        await _managementChannel.Connect(port);
        _logger.Info<ConnectLog>("Management <- [management password]");
        await _managementChannel.WriteLineAsync(password, cancellationToken);
    }

    public async Task<ReceivedManagementMessage> ReadMessageAsync(CancellationToken cancellationToken)
    {
        string? messageText = await _managementChannel.ReadLineAsync(cancellationToken);
        ReceivedManagementMessage message = Messages.ReceivedMessage(messageText ?? "");
        Log(message);
        return message;
    }

    public Task WriteMessage(ManagementMessage message, CancellationToken cancellationToken)
    {
        Log(message);
        return _managementChannel.WriteLineAsync(message.ToString(), cancellationToken);
    }

    public void Disconnect()
    {
        _managementChannel.Disconnect();
    }

    private void Log(ReceivedManagementMessage message)
    {
        if (!message.IsByteCount)
        {
            _logger.Info<OpenVpnProtocolLog>($"Management -> {message}");
        }
    }

    private void Log(ManagementMessage message)
    {
        _logger.Info<OpenVpnProtocolLog>($"Management <- {message.LogText}");
    }
}