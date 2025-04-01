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
using ProtonVPN.Common.Legacy.OS.DeviceIds;
using ProtonVPN.Common.Legacy.OS.Net.Http;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Crypto;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.AppUpdateLogs;
using ProtonVPN.Update.Config;
using ProtonVPN.Update.Releases;
using ProtonVPN.Update.Responses;

namespace ProtonVPN.Update.Storage;

/// <summary>
/// Reads app release data from provided URL and converts it into sequence of app releases.
/// </summary>
public class WebReleaseStorage : IReleaseStorage
{
    private static readonly JsonSerializer _jsonSerializer = new();

    private readonly IAppUpdateConfig _config;
    private readonly ILogger _logger;
    private readonly IDeviceIdCache _deviceIdCache;
    private readonly IConfiguration _configuration;
    private readonly Lazy<decimal> _deviceRolloutProportion;

    public WebReleaseStorage(IAppUpdateConfig config,
        ILogger logger,
        IDeviceIdCache deviceIdCache,
        IConfiguration configuration)
    {
        _config = config;
        _logger = logger;
        _deviceIdCache = deviceIdCache;
        _configuration = configuration;
        _deviceRolloutProportion = new(CreateDeviceRolloutProportion);
    }

    private decimal CreateDeviceRolloutProportion()
    {
        decimal deviceRolloutProportion;
        if (_configuration.DeviceRolloutProportion.HasValue)
        {
            deviceRolloutProportion = _configuration.DeviceRolloutProportion.Value;
            _logger.Info<AppUpdateCheckLog>($"Using device rollout proportion {deviceRolloutProportion} " +
                $"from configuration.");
        }
        else
        {
            deviceRolloutProportion = HashGenerator.HashToPercentage(_deviceIdCache.GetDeviceId() + _config.CurrentVersion);
            _logger.Info<AppUpdateCheckLog>($"Generated device rollout proportion {deviceRolloutProportion} " +
                $"for device Id '{_deviceIdCache.GetDeviceId()}' and version '{_config.CurrentVersion}'.");
        }
        return deviceRolloutProportion;
    }

    public async Task<IEnumerable<Release>> GetReleasesAsync()
    {
        Uri feedUrl = _config.FeedUriProvider.GetFeedUrl();
        IEnumerable<ReleaseResponse> releases = (await GetAsync(feedUrl)).Releases.Where(ReleaseFilter);
        return new Releases.Releases(_logger, releases, _config.CurrentVersion, _config.EarlyAccessCategoryName);
    }

    private bool ReleaseFilter(ReleaseResponse r)
    {
        if (Version.TryParse(r.Version, out Version version) && _config.CurrentVersion >= version)
        {
            return true;
        }

        return (r.ReleaseDate is null || r.ReleaseDate <= DateTime.UtcNow) &&
               (r.SystemVersion?.Minimum is null ||
                !Version.TryParse(r.SystemVersion.Minimum, out Version minimumOsVersion) ||
                Environment.OSVersion.Version >= minimumOsVersion) &&
               IsCoveredByRollout(r.RolloutProportion);
    }

    private bool IsCoveredByRollout(decimal? rolloutProportion)
    {
        if (rolloutProportion is null || rolloutProportion.Value >= 1M)
        {
            return true;
        }
        if (rolloutProportion.Value <= 0M)
        {
            return false;
        }
        try
        {   // deviceRolloutProportion <= rolloutProportion
            return decimal.Compare(_deviceRolloutProportion.Value, rolloutProportion.Value) <= 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<ReleasesResponse> GetAsync(Uri feedUrl)
    {
        try
        {
            using IHttpResponseMessage response = await _config.FeedHttpClient.GetAsync(feedUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error<AppLog>($"Response status code of feed {feedUrl} is not success.");
                return null;
            }

            using Stream stream = await response.Content.ReadAsStreamAsync();
            return ResponseStreamResult<ReleasesResponse>(stream);
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
        T result = _jsonSerializer.Deserialize<T>(jsonTextReader);
        return result == null ? throw new JsonException() : result;
    }
}