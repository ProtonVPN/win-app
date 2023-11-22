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

using ProtonVPN.Builds.Variables;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.OperatingSystems.EnvironmentVariables;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Contracts.Entities;
using ProtonVPN.Configurations.Entities;

namespace ProtonVPN.Configurations.BigTestInfra;

public class BtiTlsPinningLoader
{
    public static ITlsPinningConfiguration Get(object? defaultValue)
    {
        TlsPinningConfiguration config = defaultValue is not null && defaultValue is TlsPinningConfiguration tpc ? tpc : new();
        SetConfigIfNotNull(config);
        return config;
    }

    private static void SetConfigIfNotNull(TlsPinningConfiguration config)
    {
        HashSet<string>? apiTlsPinningPublicKeyHashes = GetApiTlsPinningPublicKeyHashes();
        if (apiTlsPinningPublicKeyHashes is not null)
        {
            string btiApiDomain = BtiUrlsLoader.GetApiUri()?.Host ?? Constants.API_URL;
            SetConfig(config, btiApiDomain, apiTlsPinningPublicKeyHashes);
        }

        HashSet<string>? alternativeRoutingTlsPinningPublicKeyHashes = GetAlternativeRoutingTlsPinningPublicKeyHashes();
        if (alternativeRoutingTlsPinningPublicKeyHashes is not null)
        {
            SetConfig(config, Constants.ALTERNATIVE_ROUTING_HOSTNAME, alternativeRoutingTlsPinningPublicKeyHashes);
        }
    }

    private static HashSet<string>? GetApiTlsPinningPublicKeyHashes()
    {
        HashSet<string>? apiTlsPinningPublicKeyHashes =
            EnvironmentVariableLoader.GetOrNull("BTI_API_TLS_PINNINGS")?.SplitToHashSet(',');
        if (apiTlsPinningPublicKeyHashes is null || apiTlsPinningPublicKeyHashes.Count == 0)
        {
            apiTlsPinningPublicKeyHashes = GlobalConfig.BtiApiTlsPinningPublicKeyHashes.SplitToHashSet(',');
            if (apiTlsPinningPublicKeyHashes is not null && apiTlsPinningPublicKeyHashes.Count > 0)
            {
                return apiTlsPinningPublicKeyHashes;
            }
        }
        else
        {
            return apiTlsPinningPublicKeyHashes;
        }
        return null;
    }

    private static void SetConfig(TlsPinningConfiguration config, string domain, HashSet<string> tlsPinningPublicKeyHashes)
    {
        ITlsPinnedDomain? pinnedDomain = config.PinnedDomains.FirstOrDefault(pd => pd.Name == domain);
        if (pinnedDomain is not null && pinnedDomain is TlsPinnedDomain tlsPinnedDomain)
        {
            tlsPinnedDomain.PublicKeyHashes = tlsPinningPublicKeyHashes;
        }
        else
        {
            List<ITlsPinnedDomain> pinnedDomains = new()
            {
                new TlsPinnedDomain()
                {
                    Name = domain,
                    PublicKeyHashes = tlsPinningPublicKeyHashes,
                    Enforce = true,
                    SendReport = true,
                }
            };
            pinnedDomains.AddRange(config.PinnedDomains);
            config.PinnedDomains = pinnedDomains;
        }
    }

    private static HashSet<string>? GetAlternativeRoutingTlsPinningPublicKeyHashes()
    {
        HashSet<string>? alternativeRoutingTlsPinningPublicKeyHashes =
            EnvironmentVariableLoader.GetOrNull("BTI_ALT_ROUTE_TLS_PINNINGS")?.SplitToHashSet(',');
        if (alternativeRoutingTlsPinningPublicKeyHashes is null || alternativeRoutingTlsPinningPublicKeyHashes.Count == 0)
        {
            alternativeRoutingTlsPinningPublicKeyHashes = GlobalConfig.BtiAlternativeRoutingTlsPinningPublicKeyHashes.SplitToHashSet(',');
            if (alternativeRoutingTlsPinningPublicKeyHashes is not null && alternativeRoutingTlsPinningPublicKeyHashes.Count > 0)
            {
                return alternativeRoutingTlsPinningPublicKeyHashes;
            }
        }
        else
        {
            return alternativeRoutingTlsPinningPublicKeyHashes;
        }
        return null;
    }
}