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
        private readonly string _filename;

        public FileCheckSum(string filename)
        {
            _filename = filename;
        }

        public async Task<string> Value()
        {
            using FileStream stream = new(_filename, FileMode.Open, FileAccess.Read, FileShare.Read, Config.FILE_BUFFER_SIZE, true);
            using SHA512 sha512 = SHA512.Create();

            byte[] buffer = new byte[Config.FILE_BUFFER_SIZE];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, Config.FILE_BUFFER_SIZE)) > 0)
            {
                sha512.TransformBlock(buffer, 0, bytesRead, null, 0);
            }
            sha512.TransformFinalBlock(buffer, 0, 0);

            if (sha512.Hash != null)
            {
                return BitConverter.ToString(sha512.Hash).Replace("-", "").ToLowerInvariant();
            }

            return string.Empty;
        }
    }
}