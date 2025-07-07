/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;

namespace ProtonVPN.Client.Logic.Servers.Cache;

public class ServerCountCache : IServerCountCache
{
    private const int SERVER_ROUND_DOWN_THRESHOLD = 100;

    private readonly IApiClient _apiClient;
    private readonly ILogger _logger;
    private readonly ISettings _settings;

    private int _serverCount;
    private int _countryCount;

    public ServerCountCache(
        IApiClient apiClient,
        ILogger logger,
        ISettings settings)
    {
        _apiClient = apiClient;
        _logger = logger;
        _settings = settings;

        _serverCount = _settings.TotalServerCount;
        _countryCount = _settings.TotalCountryCount;
    }

    public async Task UpdateAsync()
    {
        try
        {
            ApiResponseResult<ServerCountResponse> response = await _apiClient.GetServersCountAsync();
            if (response.Success && response.Value != null)
            {
                if (response.Value.Servers > 0)
                {
                    _serverCount = response.Value.Servers;
                    _settings.TotalServerCount = _serverCount;
                }

                if (response.Value.Countries > 0)
                {
                    _countryCount = response.Value.Countries;
                    _settings.TotalCountryCount = _countryCount;
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error<ApiErrorLog>("API: Get servers count failed", e);
        }
    }

    public int GetServerCount()
    {
        int serverCount = Math.Max(DefaultSettings.TotalServerCount, _serverCount);
        int roundedServerCount = serverCount - (serverCount % SERVER_ROUND_DOWN_THRESHOLD);

        return roundedServerCount;
    }

    public int GetCountryCount()
    {
        return Math.Max(DefaultSettings.TotalCountryCount, _countryCount);
    }
}