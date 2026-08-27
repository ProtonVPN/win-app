/*
 * Copyright (c) 2026 Proton AG
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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProtocolLogs;
using ProtonVPN.ProTun.Generated;

namespace ProtonVPN.ProTun.StateChanges;

public class PersistentCacheHandler : IPersistentCacheHandler
{
    private readonly ILogger _logger;

    public PersistentCacheHandler(ILogger logger)
    {
        _logger = logger;
    }

    public void ClearAll()
    {
        _logger.Info<ProTunProtocolLog>("ProTUN persistent cache cleared");
    }

    public byte[]? Get(CacheKey key)
    {
        _logger.Debug<ProTunProtocolLog>($"Get '{key}' from persistent cache");
        return [];
    }

    public void Put(CacheKey key, byte[] bytes)
    {
        _logger.Debug<ProTunProtocolLog>($"Write '{key}' to persistent cache");
    }

    public void Remove(CacheKey key)
    {
        _logger.Debug<ProTunProtocolLog>($"Delete '{key}' from persistent cache");
    }
}