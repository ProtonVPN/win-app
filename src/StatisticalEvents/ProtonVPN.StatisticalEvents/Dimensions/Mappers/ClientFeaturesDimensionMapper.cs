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

using System.Collections.Generic;
using System.Linq;
using ProtonVPN.StatisticalEvents.Contracts.Models;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers;

public class ClientFeaturesDimensionMapper : DimensionMapperBase, IClientFeaturesDimensionMapper
{
    private const string NONE = "none";

    private const string KILL_SWITCH = "kill_switch";
    private const string NETSHIELD = "netshield";
    private const string PORT_FORWARDING = "port_forwarding";
    private const string SPLIT_TUNNELING = "split_tunneling";
    private const string MODERATE_NAT = "moderate_nat";
    private const string CUSTOM_DNS = "custom_dns";
    private const string LAN_CONNECTIONS = "lan_connections";
    private const string CONNECTION_PREFERENCES = "connection_preferences";

    public string Map(ClientFeaturesEventData clientFeatures)
    {
        List<string> tokens = [];

        if (clientFeatures.IsKillSwitchEnabled)
        {
            tokens.Add(KILL_SWITCH);
        }

        if (clientFeatures.IsNetShieldEnabled)
        {
            tokens.Add(NETSHIELD);
        }

        if (clientFeatures.IsPortForwardingEnabled)
        {
            tokens.Add(PORT_FORWARDING);
        }

        if (clientFeatures.IsSplitTunnelingEnabled)
        {
            tokens.Add(SPLIT_TUNNELING);
        }

        if (clientFeatures.IsModerateNatEnabled)
        {
            tokens.Add(MODERATE_NAT);
        }

        if (clientFeatures.IsCustomDnsEnabled)
        {
            tokens.Add(CUSTOM_DNS);
        }

        if (clientFeatures.IsLanConnectionsEnabled)
        {
            tokens.Add(LAN_CONNECTIONS);
        }

        if (clientFeatures.IsConnectionPreferencesConfigured)
        {
            tokens.Add(CONNECTION_PREFERENCES);
        }

        return tokens.Count == 0 ? NONE : string.Join(",", tokens.OrderBy(t => t));
    }
}
