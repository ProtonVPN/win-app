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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Validators;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection;

public class ConnectionErrorHandler : IConnectionErrorHandler
{
    private readonly ILogger _logger;
    private readonly IEntityMapper _entityMapper;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IAuthCertificateManager _authCertificateManager;
    private readonly INetworkAdapterValidator _networkAdapterValidator;
    private readonly IConnectionManager _connectionManager;

    private VpnErrorTypeIpcEntity _error = VpnErrorTypeIpcEntity.None;

    public ConnectionErrorHandler(
        ILogger logger,
        IEntityMapper entityMapper,
        IEventMessageSender eventMessageSender,
        IAuthCertificateManager authCertificateManager,
        INetworkAdapterValidator networkAdapterValidator,
        IConnectionManager connectionManager)
    {
        _logger = logger;
        _entityMapper = entityMapper;
        _eventMessageSender = eventMessageSender;
        _authCertificateManager = authCertificateManager;
        _networkAdapterValidator = networkAdapterValidator;
        _connectionManager = connectionManager;
    }

    public async Task<ConnectionErrorHandlerResult> HandleAsync(VpnErrorTypeIpcEntity ipcError)
    {
        if (_error == ipcError)
        {
            return ConnectionErrorHandlerResult.SameAsLast;
        }

        ConnectionErrorHandlerResult response = ConnectionErrorHandlerResult.NoAction;
        VpnError error = _entityMapper.Map<VpnErrorTypeIpcEntity, VpnError>(ipcError);
        if (error == VpnError.NoTapAdaptersError)
        {
            response = await HandleNoTapAdaptersErrorAsync();
        }
        else if (error == VpnError.CertificateExpired)
        {
            await _authCertificateManager.ForceRequestNewCertificateAsync();
            // No need to reconnect, if the certificate was updated successfully,
            // ConnectionManager will inform the service about updated certificate.
            return ConnectionErrorHandlerResult.NoAction;
        }
        else if (error.RequiresCertificateUpdate())
        {
            await _authCertificateManager.ForceRequestNewKeyPairAndCertificateAsync();
            response = await ReconnectAsync();
        }
        else if (error.RequiresInformingUser())
        {
            response = SendConnectionErrorMessage(error);
        }
        else if (error.RequiresReconnectWithoutLastServer())
        {
            // TODO: Either (1) Reconnect without last server, or (2) Delete this separation between RequiresReconnectWithoutLastServer and RequiresReconnect
            response = await ReconnectAsync();
        }
        else if (error.RequiresReconnect())
        {
            response = await ReconnectAsync();
        }

        _error = ipcError;
        return response;
    }

    private async Task<ConnectionErrorHandlerResult> HandleNoTapAdaptersErrorAsync()
    {
        if (_networkAdapterValidator.IsOpenVpnAdapterAvailable())
        {
            _logger.Info<ConnectTriggerLog>("Disconnected with NoTapAdaptersError " +
                                            "but currently an OpenVPN adapter is available. Requesting a reconnection.");
            return await ReconnectAsync();
        }
        else
        {
            _logger.Warn<DisconnectLog>("Disconnected with NoTapAdaptersError and no OpenVPN adapter is available. Showing error modal.");
            return SendConnectionErrorMessage(VpnError.NoTapAdaptersError);
        }
    }

    private ConnectionErrorHandlerResult SendConnectionErrorMessage(VpnError error)
    {
        _eventMessageSender.Send(new ConnectionErrorMessage { VpnError = error });
        return ConnectionErrorHandlerResult.NoAction;
    }

    private async Task<ConnectionErrorHandlerResult> ReconnectAsync()
    {
        return await _connectionManager.ReconnectAsync()
            ? ConnectionErrorHandlerResult.Reconnecting
            : ConnectionErrorHandlerResult.NoAction;
    }
}