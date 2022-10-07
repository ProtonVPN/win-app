/*
 * Copyright (c) 2022 Proton Technologies AG
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

using Newtonsoft.Json;
using ProtonVPN.Update.Config;
using ProtonVPN.Update.Contracts;
using ProtonVPN.Update.Releases;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Common.OS.Net.Http;

namespace ProtonVPN.Update.Storage
{
    /// <summary>
    /// Reads app release data from provided URL and converts it into sequence of app releases.
    /// </summary>
    public class WebReleaseStorage : IReleaseStorage
    {
        private static readonly JsonSerializer JsonSerializer = new();

        private readonly IAppUpdateConfig _config;

        public WebReleaseStorage(IAppUpdateConfig config)
        {
            _config = config;
        }

        public async Task<IEnumerable<Release>> Releases()
        {
            CategoriesContract categories = await Categories();
            Releases.Releases releases = new(categories.Categories, _config.CurrentVersion, _config.EarlyAccessCategoryName);
            return releases;
        }

        private async Task<CategoriesContract> Categories()
        {
            using IHttpResponseMessage response = await _config.HttpClient.GetAsync(_config.FeedUriProvider.GetFeedUrl());
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Response status code is not success");
            }

            using Stream stream = await response.Content.ReadAsStreamAsync();
            return ResponseStreamResult<CategoriesContract>(stream);
        }

        private static T ResponseStreamResult<T>(Stream stream)
        {
            using StreamReader streamReader = new(stream);
            using JsonTextReader jsonTextReader = new(streamReader);
            T result = JsonSerializer.Deserialize<T>(jsonTextReader);
            if (result == null)
            {
                throw new JsonException();
            }

            return result;
        }
    }
}