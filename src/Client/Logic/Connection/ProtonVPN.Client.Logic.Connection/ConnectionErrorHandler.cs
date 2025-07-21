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
using ProtonVPN.Client.Logic.Users.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Logic.Connection;

public class ConnectionErrorHandler : IConnectionErrorHandler
{
    private readonly ILogger _logger;
    private readonly IEntityMapper _entityMapper;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IConnectionCertificateManager _connectionCertificateManager;
    private readonly INetworkAdapterValidator _networkAdapterValidator;
    private readonly IConnectionManager _connectionManager;
    private readonly IVpnPlanUpdater _vpnPlanUpdater;

    private VpnErrorTypeIpcEntity _error = VpnErrorTypeIpcEntity.None;
    private VpnStatusIpcEntity _status = VpnStatusIpcEntity.Disconnected;

    public ConnectionErrorHandler(
        ILogger logger,
        IEntityMapper entityMapper,
        IEventMessageSender eventMessageSender,
        IConnectionCertificateManager connectionCertificateManager,
        INetworkAdapterValidator networkAdapterValidator,
        IConnectionManager connectionManager,
        IVpnPlanUpdater vpnPlanUpdater)
    {
        _logger = logger;
        _entityMapper = entityMapper;
        _eventMessageSender = eventMessageSender;
        _connectionCertificateManager = connectionCertificateManager;
        _networkAdapterValidator = networkAdapterValidator;
        _connectionManager = connectionManager;
        _vpnPlanUpdater = vpnPlanUpdater;
    }

    public async Task<ConnectionErrorHandlerResult> HandleAsync(VpnStateIpcEntity vpnState)
    {
        VpnError error = _entityMapper.Map<VpnErrorTypeIpcEntity, VpnError>(vpnState.Error);

        DeleteCertificateIfRequired(error, vpnState.ConnectionCertificatePem);

        if (_error == vpnState.Error && _status == vpnState.Status)
        {
            // If there is a certificate error, check if the certificate is old and refresh if old, regardless if the error is repeating.
            // And always send the certificate to the service to ensure it is up-to-date with the most recent certificate, regardless if updated or not.
            if (error == VpnError.CertificateExpired ||
                error == VpnError.PlanNeedsToBeUpgraded ||
                error.RequiresCertificateUpdate())
            {
                await _connectionCertificateManager.RequestNewCertificateAsync(
                    CancellationToken.None,
                    vpnState.ConnectionCertificatePem);
            }
            return ConnectionErrorHandlerResult.SameAsLast;
        }

        // If the service disconnects and it is not the user intent, reconnect.
        if (vpnState.Status == VpnStatusIpcEntity.Disconnected &&
            vpnState.Error == VpnErrorTypeIpcEntity.None &&
            !_connectionManager.IsDisconnected &&
            _connectionManager.CurrentConnectionIntent is not null)
        {
            return await ReconnectAsync();
        }

        // For the lines below this if-block, only address the error if the error is not the same, regardless if the status changed or not.
        if (_error == vpnState.Error)
        {
            return ConnectionErrorHandlerResult.SameAsLast;
        }

        _error = vpnState.Error;
        _status = vpnState.Status;

        if (error == VpnError.BaseFilteringEngineServiceNotRunning)
        {
            return HandleBaseFilteringEngineServiceNotRunning();
        }

        if (error == VpnError.NoTapAdaptersError)
        {
            return await HandleNoTapAdaptersErrorAsync();
        }

        if (error == VpnError.CertificateExpired)
        {
            await _connectionCertificateManager.RequestNewCertificateAsync(
                CancellationToken.None,
                vpnState.ConnectionCertificatePem);
            // No need to reconnect, if the certificate was updated successfully,
            // ConnectionManager will inform the service about updated certificate.
            return ConnectionErrorHandlerResult.NoAction;
        }

        if (error == VpnError.PlanNeedsToBeUpgraded)
        {
            await _vpnPlanUpdater.ForceUpdateAsync();
            // No reconnect is asked directly here. If the VPN Plan is updated due to the line above,
            // a VpnPlanChangedMessage should be triggered by it and handled by a class that reconnects.
            return ConnectionErrorHandlerResult.NoAction;
        }

        if (error.RequiresCertificateUpdate())
        {
            await _connectionCertificateManager.ForceRequestNewKeyPairAndCertificateAsync();
            return await ReconnectAsync();
        }

        if (error.RequiresInformingUser())
        {
            return SendConnectionErrorMessage(error);
        }

        if (error.RequiresReconnectWithoutLastServer())
        {
            // VPNWIN-2103 - Either 
            // (1) Reconnect without last server, or 
            // (2) Delete this separation between RequiresReconnectWithoutLastServer and RequiresReconnect
            return await ReconnectAsync();
        }

        if (error.RequiresReconnect())
        {
            return await ReconnectAsync();
        }

        return ConnectionErrorHandlerResult.NoAction;
    }

    private void DeleteCertificateIfRequired(VpnError error, string connectionCertificatePem)
    {
        if (error.RequiresCertificateDeletion())
        {
            _connectionCertificateManager.DeleteKeyPairAndCertificateIfMatches(connectionCertificatePem);
        }
    }

    private ConnectionErrorHandlerResult HandleBaseFilteringEngineServiceNotRunning()
    {
        return SendConnectionErrorMessage(VpnError.BaseFilteringEngineServiceNotRunning);
    }

    private async Task<ConnectionErrorHandlerResult> HandleNoTapAdaptersErrorAsync()
    {
        if (_networkAdapterValidator.IsOpenVpnAdapterAvailable())
        {
            _logger.Info<ConnectTriggerLog>("Disconnected with NoTapAdaptersError " +
                                            "but currently an OpenVPN adapter is available. Requesting a reconnection.");
            return await ReconnectAsync();
        }

        _logger.Warn<DisconnectLog>("Disconnected with NoTapAdaptersError and no OpenVPN adapter is available. Showing error modal.");
        return SendConnectionErrorMessage(VpnError.NoTapAdaptersError);
    }

    private ConnectionErrorHandlerResult SendConnectionErrorMessage(VpnError error)
    {
        _eventMessageSender.Send(new ConnectionErrorMessage { VpnError = error });
        return ConnectionErrorHandlerResult.NoAction;
    }

    private async Task<ConnectionErrorHandlerResult> ReconnectAsync()
    {
        return await _connectionManager.ReconnectAsync(VpnTriggerDimension.Auto)
            ? ConnectionErrorHandlerResult.Reconnecting
            : ConnectionErrorHandlerResult.NoAction;
    }
}