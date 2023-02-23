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

using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.UserCertificateLogs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Crypto;

namespace ProtonVPN.Core.Auth
{
    public class AuthCredentialManager : IAuthCredentialManager
    {
        private readonly ILogger _logger;
        private readonly IAuthKeyManager _authKeyManager;
        private readonly IAuthCertificateManager _authCertificateManager;
        private readonly IAppSettings _appSettings;

        public AuthCredentialManager(ILogger logger,
            IAuthKeyManager authKeyManager, 
            IAuthCertificateManager authCertificateManager, 
            IAppSettings appSettings)
        {
            _logger = logger;
            _authKeyManager = authKeyManager;
            _authCertificateManager = authCertificateManager;
            _appSettings = appSettings;
        }

        public async Task<AuthCredential> GenerateAsync()
        {
            AsymmetricKeyPair keyPair = _authKeyManager.GetKeyPairOrNull();
            string certificatePem = _appSettings.AuthenticationCertificatePem;
            
            if (keyPair == null)
            {
                _logger.Info<UserCertificateLog>("Missing auth key pair, requesting new keys and certificate.");
                await _authCertificateManager.ForceRequestNewKeyPairAndCertificateAsync();
                keyPair = _authKeyManager.GetKeyPairOrNull();
                certificatePem = _appSettings.AuthenticationCertificatePem;
            }
            else if (certificatePem == null)
            {
                _logger.Info<UserCertificateLog>("Auth keys are present but certificate is missing, requesting new certificate.");
                await _authCertificateManager.ForceRequestNewCertificateAsync();
                keyPair = _authKeyManager.GetKeyPairOrNull();
                certificatePem = _appSettings.AuthenticationCertificatePem;
            }

            return new AuthCredential(keyPair, certificatePem);
        }
    }
}