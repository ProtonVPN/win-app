/*
 * Copyright (c) 2025 Proton AG
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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;

namespace ProtonVPN.Client.Logic.Connection.EntityMapping.Extensions;

public static class SerializableLocationIntentExtensions
{
    // This method is made to ensure backward compatibility
    public static ServerInfo GetServer(this SerializableLocationIntent intent)
    {
        return intent.Server
            ?? ServerInfo.From(intent.Id, intent.Name);
    }

    // This method is made to ensure backward compatibility
    public static GatewayServerInfo GetGatewayServer(this SerializableLocationIntent intent)
    {
        return intent.GatewayServer
            ?? GatewayServerInfo.From(intent.Id, intent.Name, intent.CountryCode);
    }

    // This method is made to ensure backward compatibility
    public static ServerInfo GetServerToExclude(this SerializableLocationIntent intent)
    {
        return intent.ServerToExclude
            ?? ServerInfo.From(intent.FreeServerExcludedLogicalId, string.Empty);
    }

    // This method is made to ensure backward compatibility
    public static SelectionStrategy GetSelectionStrategy(this SerializableLocationIntent intent)
    {
        // Prefer the new strategy if present
        if (intent.Strategy.HasValue && Enum.IsDefined(typeof(SelectionStrategy), intent.Strategy.Value))
        {
            return intent.Strategy.Value;
        }

        // Fallback to old 'FreeServerType' value if available
        if (intent.FreeServerType.HasValue && Enum.IsDefined(typeof(SelectionStrategy), intent.FreeServerType.Value))
        {
            return (SelectionStrategy)intent.FreeServerType.Value;
        }

        // Fallback to old 'Kind' value if available
        if (!string.IsNullOrEmpty(intent.Kind))
        {
            if (Enum.TryParse(intent.Kind, ignoreCase: true, out SelectionStrategy parsedStrategy))
            {
                return parsedStrategy;
            }
        }

        // Default to fastest
        return SelectionStrategy.Fastest;
    }
}