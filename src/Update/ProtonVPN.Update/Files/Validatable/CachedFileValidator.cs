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
using System.Threading.Tasks;

namespace ProtonVPN.Update.Files.Validatable
{
    /// <summary>
    /// Caches positive file validation result while file length and modification date has not changed.
    /// </summary>
    public class CachedFileValidator : IFileValidator
    {
        private readonly IFileValidator _origin;

        private string _filename;
        private string _checkSum;
        private long _fileLength;
        private DateTime _modifiedAt;

        public CachedFileValidator(IFileValidator origin)
        {
            _origin = origin;
        }

        public async Task<bool> Valid(string filename, string checkSum)
        {
            if (CacheContains(filename, checkSum) && !FileChanged(filename))
            {
                return true;
            }

            ClearCache();

            bool valid = await _origin.Valid(filename, checkSum);

            if (valid)
            {
                AddToCache(filename, checkSum);
            }

            return valid;
        }

        private bool CacheContains(string filename, string checkSum)
        {
            return _filename == filename && _checkSum == checkSum;
        }

        private bool FileChanged(string filename)
        {
            var fileInfo = new FileInfo(filename);

            return !fileInfo.Exists ||
                   fileInfo.LastWriteTimeUtc != _modifiedAt ||
                   fileInfo.Length != _fileLength;
        }

        private void ClearCache()
        {
            _filename = null;
            _checkSum = null;
        }

        private void AddToCache(string filename, string checkSum)
        {
            var fileInfo = new FileInfo(filename);

            _modifiedAt = fileInfo.LastWriteTimeUtc;
            _fileLength = fileInfo.Length;

            _filename = filename;
            _checkSum = checkSum;
        }
    }
}
