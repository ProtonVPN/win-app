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

using System.Collections.Generic;
using ProtonVPN.StatisticalEvents.Contracts.Models;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers;

namespace ProtonVPN.StatisticalEvents.Dimensions.Builders;

public class VpnConnectionDimensionsBuilder : IVpnConnectionDimensionsBuilder
{
    private readonly IVpnProtocolDimensionMapper _vpnProtocolDimensionMapper;
    private readonly IOutcomeDimensionMapper _outcomeDimensionMapper;
    private readonly IVpnStatusDimensionMapper _vpnStatusDimensionMapper;
    private readonly IVpnTriggerDimensionMapper _vpnTriggerDimensionMapper;
    private readonly INetworkConnectionTypeDimensionMapper _networkConnectionTypeDimensionMapper;
    private readonly IVpnFeatureIntentDimensionMapper _vpnFeatureIntentDimensionMapper;
    private readonly IVpnPlanTierDimensionMapper _vpnPlanDimensionMapper;
    private readonly IServerFeaturesDimensionMapper _serverDetailsDimensionMapper;
    private readonly IPortDimensionMapper _portDimensionMapper;
    private readonly IStringDimensionMapper _stringDimensionMapper;

    public VpnConnectionDimensionsBuilder(
        IVpnProtocolDimensionMapper vpnProtocolDimensionMapper,
        IOutcomeDimensionMapper outcomeDimensionMapper,
        IVpnStatusDimensionMapper vpnStatusDimensionMapper,
        IVpnTriggerDimensionMapper vpnTriggerDimensionMapper,
        INetworkConnectionTypeDimensionMapper networkConnectionTypeDimensionMapper,
        IVpnFeatureIntentDimensionMapper vpnFeatureIntentDimensionMapper,
        IVpnPlanTierDimensionMapper vpnPlanDimensionMapper,
        IServerFeaturesDimensionMapper serverDetailsDimensionMapper,
        IPortDimensionMapper portDimensionMapper,
        IStringDimensionMapper stringDimensionMapper)
    {
        _vpnProtocolDimensionMapper = vpnProtocolDimensionMapper;
        _outcomeDimensionMapper = outcomeDimensionMapper;
        _vpnStatusDimensionMapper = vpnStatusDimensionMapper;
        _vpnTriggerDimensionMapper = vpnTriggerDimensionMapper;
        _networkConnectionTypeDimensionMapper = networkConnectionTypeDimensionMapper;
        _vpnFeatureIntentDimensionMapper = vpnFeatureIntentDimensionMapper;
        _vpnPlanDimensionMapper = vpnPlanDimensionMapper;
        _serverDetailsDimensionMapper = serverDetailsDimensionMapper;
        _portDimensionMapper = portDimensionMapper;
        _stringDimensionMapper = stringDimensionMapper;
    }

    public Dictionary<string, string> Build(VpnConnectionEventData eventData)
    {
        return new Dictionary<string, string>
        {
            { "outcome", _outcomeDimensionMapper.Map(eventData.Outcome) },
            { "user_tier", _vpnPlanDimensionMapper.Map(eventData.VpnPlan) },
            { "vpn_status", _vpnStatusDimensionMapper.Map(eventData.VpnStatus) },
            { "vpn_trigger", _vpnTriggerDimensionMapper.Map(eventData.VpnTrigger) },
            { "network_type", _networkConnectionTypeDimensionMapper.Map(eventData.NetworkConnectionType) },
            { "server_features", _serverDetailsDimensionMapper.Map(eventData.Server) },
            { "vpn_country", _stringDimensionMapper.Map(eventData.VpnCountry) },
            { "user_country",  _stringDimensionMapper.Map(eventData.UserCountry) },
            { "protocol", _vpnProtocolDimensionMapper.Map(eventData.Protocol) },
            { "server",  _stringDimensionMapper.Map(eventData.Server?.Name) },
            { "entry_ip", _stringDimensionMapper.Map(eventData.Server?.EntryIp) },
            { "port", _portDimensionMapper.Map(eventData.Port) },
            { "isp",  _stringDimensionMapper.Map(eventData.Isp) },
            { "vpn_feature_intent", _vpnFeatureIntentDimensionMapper.Map(eventData.VpnFeatureIntent) },
        };
    }
}