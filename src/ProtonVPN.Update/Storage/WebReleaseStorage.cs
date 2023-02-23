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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Update.Config;
using ProtonVPN.Update.Releases;
using ProtonVPN.Update.Responses;

namespace ProtonVPN.Update.Storage
{
    /// <summary>
    /// Reads app release data from provided URL and converts it into sequence of app releases.
    /// </summary>
    public class WebReleaseStorage : IReleaseStorage
    {
        private static readonly JsonSerializer JsonSerializer = new();

        private readonly IAppUpdateConfig _config;
        private readonly ILogger _logger;

        public WebReleaseStorage(IAppUpdateConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<IEnumerable<Release>> Releases()
        {
            CategoriesResponse categories = await Categories();
            Releases.Releases releases = new(categories.Categories, _config.CurrentVersion, _config.EarlyAccessCategoryName);
            return releases;
        }

        private async Task<CategoriesResponse> Categories()
        {
            int numOfFeeds = 0;

            foreach (Uri feedUrl in _config.FeedUriProvider.GetFeedUrls())
            {
                numOfFeeds++;
                CategoriesResponse response = await GetFilteredAsync(feedUrl);
                if (DoesResponseContainReleases(response))
                {
                    return response;
                }

                _logger.Warn<AppLog>($"The feed '{feedUrl}' has no releases.");
            }

            string errorMessage = $"All feeds failed to return any release version. Called {numOfFeeds} feed(s).";
            _logger.Error<AppLog>(errorMessage);
            throw new Exception(errorMessage);
        }

        private async Task<CategoriesResponse> GetFilteredAsync(Uri feedUrl)
        {
            CategoriesResponse response = await GetAsync(feedUrl);
            foreach(CategoryResponse category in response.Categories)
            {
                category.Releases = category.Releases.Where(ReleaseFilter).ToList();
            }
            return response;
        }

        private bool ReleaseFilter(ReleaseResponse r)
        {
            return (r.ReleaseDate is null || r.ReleaseDate.Value <= DateTimeOffset.UtcNow) &&
                   (r.MinimumOsVersion is null ||
                    !Version.TryParse(r.MinimumOsVersion, out Version minimumOsVersion) ||
                    Environment.OSVersion.Version >= minimumOsVersion);
        }

        private async Task<CategoriesResponse> GetAsync(Uri feedUrl)
        {
            try
            {
                using IHttpResponseMessage response = await _config.HttpClient.GetAsync(feedUrl);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error<AppLog>($"Response status code of feed {feedUrl} is not success.");
                    return null;
                }

                using Stream stream = await response.Content.ReadAsStreamAsync();
                return ResponseStreamResult<CategoriesResponse>(stream);
            }
            catch (Exception ex)
            {
                _logger.Error<AppLog>($"An error occurred when obtaining the list of releases from feed '{feedUrl}'.", ex);
            }
            return null;
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

        private bool DoesResponseContainReleases(CategoriesResponse response)
        {
            return response is not null &&
                   response.Categories.Any() &&
                   response.Categories.SelectMany(c => c.Releases).Any();
        }
    }
}