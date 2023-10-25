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

using System;
using Newtonsoft.Json;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;

namespace ProtonVPN.Vpn.ServerValidation;

public class ServerValidator : IServerValidator
{
    private const int SERVER_PUBLIC_KEY_LENGTH = 44;

    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly IEd25519SignatureValidator _ed25519SignatureValidator;

    public ServerValidator(ILogger logger,
        IConfiguration config,
        IEd25519SignatureValidator ed25519SignatureValidator)
    {
        _logger = logger;
        _config = config;
        _ed25519SignatureValidator = ed25519SignatureValidator;
    }

    public VpnError Validate(VpnHost server)
    {
        VpnError error;
        try
        {
            error = ValidatePublicKey(server);
            if (error == VpnError.None)
            {
                error = ValidateSignature(server);
            }
        }
        catch (Exception ex)
        {
            _logger.Error<DisconnectTriggerLog>(ex.Message);
            return VpnError.Unknown;
        }
        return error;
    }

    private VpnError ValidatePublicKey(VpnHost host)
    {
        if (host.X25519PublicKey is null)
        {
            return VpnError.None;
        }
        if (string.IsNullOrWhiteSpace(host.X25519PublicKey.Base64))
        {
            _logger.Error<DisconnectTriggerLog>($"The server with name '{host.Name}' and IP '{host.Ip}' " +
                $"has an empty public key.");
            return VpnError.ServerValidationError;
        }
        if (host.X25519PublicKey.Base64.Length != SERVER_PUBLIC_KEY_LENGTH)
        {
            _logger.Error<DisconnectTriggerLog>($"Incorrect server public key length " +
                $"{host.X25519PublicKey.Base64.Length} when it should be {SERVER_PUBLIC_KEY_LENGTH}, " +
                $"for the server with name '{host.Name}' and IP '{host.Ip}'.");
            return VpnError.ServerValidationError;
        }
        if (!host.X25519PublicKey.Base64.IsValidBase64Key())
        {
            _logger.Error<DisconnectTriggerLog>($"The server with name '{host.Name}' and IP '{host.Ip}' " +
                $"has a public key that is not base64: {host.X25519PublicKey.Base64}");
            return VpnError.ServerValidationError;
        }
        return VpnError.None;
    }

    private VpnError ValidateSignature(VpnHost host)
    {
        if (string.IsNullOrWhiteSpace(host.Signature))
        {
            _logger.Error<DisconnectTriggerLog>($"The server with name '{host.Name}' and IP '{host.Ip}' " +
                $"is missing its signature.");
            return VpnError.ServerValidationError;
        }

        string base64publicKey = _config.ServerValidationPublicKey;
        if (string.IsNullOrWhiteSpace(base64publicKey))
        {
            _logger.Error<DisconnectTriggerLog>($"The {nameof(_config.ServerValidationPublicKey)} " +
                $"configuration is empty and it is therefore impossible to validate servers.");
            return VpnError.NoServerValidationPublicKey;
        }

        string serverValidationData = GetServerValidationData(host);
        if (!_ed25519SignatureValidator.IsValid(serverValidationData, host.Signature, base64publicKey))
        {
            _logger.Error<DisconnectTriggerLog>($"The server with name '{host.Name}' and IP '{host.Ip}' " +
                $"has an incorrect signature '{host.Signature}'.");
            return VpnError.ServerValidationError;
        }
        return VpnError.None;
    }

    private string GetServerValidationData(VpnHost host)
    {
        try
        {
            ServerValidationObject server = new()
            {
                Server = new PhysicalServerValidationObject
                {
                    EntryIP = host.Ip,
                    Label = host.Label
                }
            };
            return JsonConvert.SerializeObject(server);
        }
        catch (Exception)
        {
            throw new Exception($"Failed to generate the JSON validation object " +
                $"for the server with name '{host.Name}' and IP '{host.Ip}'.");
        }
    }
}