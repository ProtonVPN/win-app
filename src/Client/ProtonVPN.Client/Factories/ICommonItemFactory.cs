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

using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Models;
using ProtonVPN.Client.Models.Settings;
using ProtonVPN.Client.Models.Profiles;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Factories;

public interface ICommonItemFactory
{
    ProtocolItem GetProtocol(VpnProtocol protocol);

    NetShieldModeItem GetNetShieldMode(NetShieldMode? netShieldMode);

    NetShieldModeItem GetNetShieldMode(bool isEnabled, NetShieldMode netShieldMode);

    NatTypeItem GetNatType(NatType natType);

    PortForwardingItem GetPortForwardingMode(bool isEnabled);

    FeatureItem GetFeature(Feature feature);

    ConnectAndGoModeItem GetConnectAndGoMode(ConnectAndGoMode? connectAndGoMode);

    ConnectAndGoModeItem GetConnectAndGoMode(bool isEnabled, ConnectAndGoMode connectAndGoMode);
}
