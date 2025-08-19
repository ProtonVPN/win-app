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

using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.StatisticalEvents.Contracts.Models;

public class VpnConnectionEventData
{
    public OutcomeDimension? Outcome { get; init; }
    public VpnStatusDimension? VpnStatus { get; init; }
    public VpnTriggerDimension? VpnTrigger { get; init; }
    public NetworkConnectionType? NetworkConnectionType { get; init; }
    public VpnProtocol? Protocol { get; init; }
    public VpnFeatureIntent? VpnFeatureIntent { get; init; }
    public VpnPlan VpnPlan { get; init; }
    public string? VpnCountry { get; init; }
    public string? UserCountry { get; init; }
    public required ServerDetailsEventData Server { get; init; }
    public int Port { get; init; }
    public string? Isp { get; init; }
    public bool IsIpv6Enabled { get; init; }
}