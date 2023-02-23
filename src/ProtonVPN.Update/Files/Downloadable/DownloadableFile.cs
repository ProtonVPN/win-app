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

using System.IO;
using System.Threading.Tasks;
using ProtonVPN.Common.OS.Net.Http;

namespace ProtonVPN.Update.Files.Downloadable
{
    /// <summary>
    /// Downloads file from internet.
    /// </summary>
    public class DownloadableFile : IDownloadableFile
    {
        private const int FileBufferSize = 16768;

        private readonly IHttpClient _client;

        public DownloadableFile(IHttpClient client)
        {
            _client = client;
        }

        public async Task Download(string url, string filename)
        {
            using (IHttpResponseMessage response = await _client.GetAsync(url))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new AppUpdateException("Failed to download file");
                }

                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                using (FileStream fileStream = new(filename, FileMode.Create, FileAccess.Write, FileShare.None, FileBufferSize, true))
                {
                    await contentStream.CopyToAsync(fileStream);
                }
            }
        }
    }
}
