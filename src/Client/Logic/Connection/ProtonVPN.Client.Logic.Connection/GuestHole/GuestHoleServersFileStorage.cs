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

using System.Security.Cryptography;
using ProtonVPN.Builds.Variables;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.GuestHoleLogs;
using ProtonVPN.Serialization.Contracts.Json;

namespace ProtonVPN.Client.Logic.Connection.GuestHole;

public class GuestHoleServersFileStorage: IGuestHoleServersFileStorage
{
    private const int TAG_SIZE_IN_BYTES = 16;

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _config;
    private readonly IJsonSerializer _jsonSerializer;

    private List<GuestHoleServerContract>? _cachedList;

    public GuestHoleServersFileStorage(ILogger logger, IStaticConfiguration config, IJsonSerializer jsonSerializer)
    {
        _logger = logger;
        _config = config;
        _jsonSerializer = jsonSerializer;
    }

    public async Task<List<GuestHoleServerContract>> GetAsync()
    {
        if (_cachedList != null)
        {
            return _cachedList;
        }

        try
        {
            MemoryStream json = await GetJsonAsync();

            _cachedList = _jsonSerializer.Deserialize<List<GuestHoleServerContract>>(json) ?? [];

            return _cachedList;
        }
        catch (Exception e)
        {
            _logger.Error<GuestHoleLog>("Failed to get guest hole servers.", e);
        }

        return [];
    }

    private async Task<MemoryStream> GetJsonAsync()
    {
        byte[] encryptedData = await File.ReadAllBytesAsync(_config.GuestHoleServersJsonFilePath);
        if (encryptedData.Length < TAG_SIZE_IN_BYTES)
        {
            throw new CryptographicException(
                $"Encrypted file '{_config.GuestHoleServersJsonFilePath}' is corrupted or truncated: " +
                $"expected at least {TAG_SIZE_IN_BYTES} bytes, got {encryptedData.Length}.");
        }

        byte[] key1 = HexStringToByteArray(GlobalConfig.GuestHoleKey1);
        byte[] key2 = HexStringToByteArray(GlobalConfig.GuestHoleKey2);
        byte[] combinedKey = InterleaveBytes(key1, key2);
        byte[] nonce = new byte[12];

        int cipherTextLength = encryptedData.Length - TAG_SIZE_IN_BYTES;
        byte[] cipherText = new byte[cipherTextLength];
        byte[] tag = new byte[TAG_SIZE_IN_BYTES];

        Array.Copy(encryptedData, 0, cipherText, 0, cipherTextLength);
        Array.Copy(encryptedData, cipherTextLength, tag, 0, TAG_SIZE_IN_BYTES);

        byte[] plainText = new byte[cipherTextLength];

        using (AesGcm aesGcm = new(combinedKey, TAG_SIZE_IN_BYTES))
        {
            aesGcm.Decrypt(nonce, cipherText, tag, plainText);
        }

        return new MemoryStream(plainText);
    }

    private byte[] InterleaveBytes(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
        {
            throw new ArgumentException(
                $"Cannot interleave arrays of different lengths: a.Length={a.Length}, b.Length={b.Length}",
                nameof(b));
        }

        byte[] result = new byte[a.Length + b.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[2 * i] = a[i];
            result[2 * i + 1] = b[i];
        }

        return result;
    }

    private byte[] HexStringToByteArray(string hex)
    {
        if (hex.Length % 2 != 0)
        {
            throw new ArgumentException("Hex string must have an even number of characters.", nameof(hex));
        }

        int length = hex.Length / 2;
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return bytes;
    }
}