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
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ProtonVPN.Update.Files.Validatable
{
    /// <summary>
    /// Calculates SHA512 checksum of file.
    /// </summary>
    public class FileCheckSum
    {
        private const int FileBufferSize = 16768;
        private readonly string _filename;

        public FileCheckSum(string filename)
        {
            _filename = filename;
        }

        public async Task<string> Value()
        {
            using var stream = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read, FileBufferSize, true);
            using var sha512 = new SHA512CryptoServiceProvider();

            var buffer = new byte[FileBufferSize];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, FileBufferSize)) > 0)
            {
                sha512.TransformBlock(buffer, 0, bytesRead, null, 0);
            }
            sha512.TransformFinalBlock(buffer, 0, 0);

            return BitConverter.ToString(sha512.Hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
