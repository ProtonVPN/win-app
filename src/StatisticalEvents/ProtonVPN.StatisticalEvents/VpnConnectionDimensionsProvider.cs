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
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;
using ProtonVPN.StatisticalEvents.Contracts.Models;
using ProtonVPN.StatisticalEvents.DimensionMapping;

namespace ProtonVPN.StatisticalEvents;

public class VpnConnectionDimensionsProvider : IVpnConnectionDimensionsProvider
{
    private readonly IDimensionMapper<VpnProtocol?> _vpnProtocolMapper;
    private readonly IDimensionMapper<OutcomeDimension?> _outcomeMapper;
    private readonly IDimensionMapper<VpnStatusDimension?> _vpnStatusMapper;
    private readonly IDimensionMapper<VpnTriggerDimension?> _vpnTriggerMapper;
    private readonly IDimensionMapper<NetworkConnectionType?> _networkConnectionTypeMapper;
    private readonly IDimensionMapper<VpnFeatureIntent?> _vpnFeatureIntentMapper;
    private readonly IDimensionMapper<VpnPlan?> _vpnPlanMapper;
    private readonly IDimensionMapper<ServerDetailsEventData> _serverDetailsMapper;

    public VpnConnectionDimensionsProvider(
        IDimensionMapper<VpnProtocol?> vpnProtocolMapper,
        IDimensionMapper<OutcomeDimension?> outcomeMapper,
        IDimensionMapper<VpnStatusDimension?> vpnStatusMapper,
        IDimensionMapper<VpnTriggerDimension?> vpnTriggerMapper,
        IDimensionMapper<NetworkConnectionType?> networkConnectionTypeMapper,
        IDimensionMapper<VpnFeatureIntent?> vpnFeatureIntentMapper,
        IDimensionMapper<VpnPlan?> vpnPlanMapper,
        IDimensionMapper<ServerDetailsEventData> serverDetailsMapper)
    {
        _vpnProtocolMapper = vpnProtocolMapper;
        _outcomeMapper = outcomeMapper;
        _vpnStatusMapper = vpnStatusMapper;
        _vpnTriggerMapper = vpnTriggerMapper;
        _networkConnectionTypeMapper = networkConnectionTypeMapper;
        _vpnFeatureIntentMapper = vpnFeatureIntentMapper;
        _vpnPlanMapper = vpnPlanMapper;
        _serverDetailsMapper = serverDetailsMapper;
    }

    public Dictionary<string, string> GetDimensions(VpnConnectionEventData eventData)
    {
        return new Dictionary<string, string>
        {
            { "outcome", _outcomeMapper.Map(eventData.Outcome) },
            { "user_tier", _vpnPlanMapper.Map(eventData.VpnPlan) },
            { "vpn_status", _vpnStatusMapper.Map(eventData.VpnStatus) },
            { "vpn_trigger", _vpnTriggerMapper.Map(eventData.VpnTrigger) },
            { "network_type", _networkConnectionTypeMapper.Map(eventData.NetworkConnectionType) },
            { "server_features", _serverDetailsMapper.Map(eventData.Server) },
            { "vpn_country", eventData.VpnCountry ?? DimensionMapperBase.NOT_AVAILABLE },
            { "user_country", eventData.UserCountry ?? DimensionMapperBase.NOT_AVAILABLE },
            { "protocol", _vpnProtocolMapper.Map(eventData.Protocol) },
            { "server", eventData.Server?.Name ?? DimensionMapperBase.NOT_AVAILABLE },
            { "entry_ip", eventData.Server?.EntryIp ?? DimensionMapperBase.NOT_AVAILABLE },
            { "port", eventData.Port > 0 ? eventData.Port.ToString() : DimensionMapperBase.NOT_AVAILABLE },
            { "isp", string.IsNullOrWhiteSpace(eventData.Isp) ? DimensionMapperBase.NOT_AVAILABLE : eventData.Isp },
            { "vpn_feature_intent", _vpnFeatureIntentMapper.Map(eventData.VpnFeatureIntent)  },
        };
    }
}