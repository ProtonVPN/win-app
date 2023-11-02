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

using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.UserCertificateLogs;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;

namespace ProtonVPN.Client.Logic.Auth;

public class AuthCredentialManager : IAuthCredentialManager
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IAuthKeyManager _authKeyManager;
    private readonly IAuthCertificateManager _authCertificateManager;

    public AuthCredentialManager(ILogger logger,
        ISettings settings,
        IAuthKeyManager authKeyManager, 
        IAuthCertificateManager authCertificateManager)
    {
        _logger = logger;
        _settings = settings;
        _authKeyManager = authKeyManager;
        _authCertificateManager = authCertificateManager;
    }

    public async Task<AuthCredential> GenerateAsync()
    {
        AsymmetricKeyPair keyPair = _authKeyManager.GetKeyPairOrNull();
        string certificatePem = _settings.AuthenticationCertificatePem;
            
        if (keyPair == null)
        {
            _logger.Info<UserCertificateLog>("Missing auth key pair, requesting new keys and certificate.");
            await _authCertificateManager.ForceRequestNewKeyPairAndCertificateAsync();
            keyPair = _authKeyManager.GetKeyPairOrNull();
            certificatePem = _settings.AuthenticationCertificatePem;
        }
        else if (certificatePem == null)
        {
            _logger.Info<UserCertificateLog>("Auth keys are present but certificate is missing, requesting new certificate.");
            await _authCertificateManager.ForceRequestNewCertificateAsync();
            keyPair = _authKeyManager.GetKeyPairOrNull();
            certificatePem = _settings.AuthenticationCertificatePem;
        }

        return new AuthCredential(keyPair, certificatePem);
    }
}