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
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection;

public class ConnectionErrorHandler : IConnectionErrorHandler, IEventMessageReceiver<VpnStateIpcEntity>
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IEntityMapper _entityMapper;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IAuthCertificateManager _authCertificateManager;
    private readonly INetworkAdapterValidator _networkAdapterValidator;

    private VpnErrorTypeIpcEntity _error = VpnErrorTypeIpcEntity.None;
    private string? _lastAuthCertificate;

    public ConnectionErrorHandler(
        ILogger logger,
        ISettings settings,
        IEntityMapper entityMapper,
        IEventMessageSender eventMessageSender,
        IAuthCertificateManager authCertificateManager,
        INetworkAdapterValidator networkAdapterValidator)
    {
        _logger = logger;
        _settings = settings;
        _entityMapper = entityMapper;
        _eventMessageSender = eventMessageSender;
        _authCertificateManager = authCertificateManager;
        _networkAdapterValidator = networkAdapterValidator;
    }

    public async void Receive(VpnStateIpcEntity message)
    {
        if (_error == message.Error)
        {
            return;
        }

        VpnError error = _entityMapper.Map<VpnErrorTypeIpcEntity, VpnError>(message.Error);
        if (error == VpnError.NoTapAdaptersError)
        {
            HandleNoTapAdaptersError();
        }
        else if (error == VpnError.CertificateExpired)
        {
            await HandleExpiredCertificateAsync();
        }
        else if (error.RequiresCertificateUpdate())
        {
            await _authCertificateManager.ForceRequestNewKeyPairAndCertificateAsync();
            // TODO: reconnect
        }
        else if (error.RequiresInformingUser())
        {
            SendConnectionErrorMessage(error);
        }
        else if (error.RequiresReconnectWithoutLastServer())
        {
            // TODO: reconnect without last server
        }
        else if (error.RequiresReconnect())
        {
            // TODO: reconnect
        }

        _error = message.Error;
    }

    private void HandleNoTapAdaptersError()
    {
        if (_networkAdapterValidator.IsOpenVpnAdapterAvailable())
        {
            _logger.Info<ConnectTriggerLog>("Disconnected with NoTapAdaptersError " +
                                            "but currently an OpenVPN adapter is available. Requesting a reconnection.");
            // TODO: reconnect
        }
        else
        {
            _logger.Warn<DisconnectLog>("Disconnected with NoTapAdaptersError and no OpenVPN adapter is available. Showing error modal.");
            SendConnectionErrorMessage(VpnError.NoTapAdaptersError);
        }
    }

    private async Task HandleExpiredCertificateAsync()
    {
        _lastAuthCertificate = _settings.AuthenticationCertificatePem;
        await _authCertificateManager.ForceRequestNewCertificateAsync();
        if (FailedToUpdateAuthCert())
        {
            // TODO: reconnect
        }
    }

    private bool FailedToUpdateAuthCert()
    {
        return _lastAuthCertificate == _settings.AuthenticationCertificatePem;
    }

    private void SendConnectionErrorMessage(VpnError error)
    {
        _eventMessageSender.Send(new ConnectionErrorMessage { VpnError = error });
    }
}