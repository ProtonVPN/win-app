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

using ProtonVPN.Client.Logic.Servers.Contracts;

namespace ProtonVPN.Client.Logic.Servers;

public class ServerCountCache : IServerCountCache
{
    private const int SERVER_ROUND_DOWN_THRESHOLD = 100;
    private const int SERVER_FALLBACK_COUNT = 4000;
    private const int COUNTRY_FALLBACK_COUNT = 85;

    private readonly IServersCache _serversCache;

    public ServerCountCache(IServersCache serversCache)
    {
        _serversCache = serversCache;
    }

    public int GetServerCount()
    {
        int serverCount = _serversCache.Servers.Count;
        int roundedServerCount = serverCount - (serverCount % SERVER_ROUND_DOWN_THRESHOLD);

        return Math.Max(SERVER_FALLBACK_COUNT, roundedServerCount);
    }

    public int GetCountryCount()
    {
        int countryCount = _serversCache.CountryCodes.Count;

        return Math.Max(COUNTRY_FALLBACK_COUNT, countryCount);
    }
}