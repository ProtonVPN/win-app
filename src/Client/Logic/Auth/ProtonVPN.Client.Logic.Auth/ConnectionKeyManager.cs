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
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Auth;

public class ConnectionKeyManager : IConnectionKeyManager
{
    private const KeyAlgorithm ALGORITHM = KeyAlgorithm.Ed25519;

    private readonly IEd25519Asn1KeyGenerator _ed25519Asn1KeyGenerator;
    private readonly ILogger _logger;
    private readonly ISettings _settings;

    public ConnectionKeyManager(IEd25519Asn1KeyGenerator ed25519Asn1KeyGenerator, ILogger logger, ISettings settings)
    {
        _ed25519Asn1KeyGenerator = ed25519Asn1KeyGenerator;
        _logger = logger;
        _settings = settings;
    }

    public void RegenerateKeyPair()
    {
        AsymmetricKeyPair asymmetricKeyPair = _ed25519Asn1KeyGenerator.Generate();

        _settings.ConnectionKeyPair = new ConnectionAsymmetricKeyPair()
        {
            PublicKey = asymmetricKeyPair.PublicKey.Base64,
            SecretKey = asymmetricKeyPair.SecretKey.Base64,
        };
        _logger.Info<AppLog>("New connection key pair successfully generated and saved.");
    }

    public void DeleteKeyPair()
    {
        _settings.ConnectionKeyPair = null;
        _logger.Info<AppLog>("Connection key pair deleted.");
    }

    public AsymmetricKeyPair? GetKeyPairOrNull()
    {
        SecretKey? secretKey = GetSecretKey();
        PublicKey? publicKey = GetPublicKey();
        return secretKey == null || publicKey == null ? null : new(secretKey, publicKey);
    }

    public AsymmetricKeyPair GenerateTemporaryKeyPair()
    {
        return _ed25519Asn1KeyGenerator.Generate();
    }

    public SecretKey? GetSecretKey()
    {
        string? key = _settings.ConnectionKeyPair?.SecretKey;
        return string.IsNullOrEmpty(key) ? null : new SecretKey(key, ALGORITHM);
    }

    public PublicKey? GetPublicKey()
    {
        string? key = _settings.ConnectionKeyPair?.PublicKey;
        return string.IsNullOrEmpty(key) ? null : new PublicKey(key, ALGORITHM);
    }
}