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
using System.Threading.Tasks;

namespace ProtonVPN.Update.Files.Downloadable
{
    /// <summary>
    /// Wraps expected exceptions of <see cref="DownloadableFile"/> into <see cref="AppUpdateException"/>.
    /// </summary>
    public class SafeDownloadableFile : IDownloadableFile
    {
        private readonly IDownloadableFile _origin;

        public SafeDownloadableFile(IDownloadableFile origin)
        {
            _origin = origin;
        }

        public async Task Download(string url, string filename)
        {
            try
            {
                await _origin.Download(url, filename);
            }
            catch (Exception e)
            {
                throw new AppUpdateException("Failed to download update", e);
            }
        }
    }
}