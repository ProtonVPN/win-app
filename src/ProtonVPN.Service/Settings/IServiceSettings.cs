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

using System;
using System.Collections.Generic;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.KillSwitch;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;

namespace ProtonVPN.Service.Settings;

public interface IServiceSettings
{
    KillSwitchMode KillSwitchMode { get; }
    SplitTunnelSettingsIpcEntity SplitTunnelSettings { get; }
    bool Ipv6LeakProtection { get; }
    bool IsIpv6FeatureFlagEnabled { get; }
    List<string> Ipv6Fragments { get; }
    bool IsShareCrashReportsEnabled { get; }
    bool IsLocalAreaNetworkAccessEnabled { get; }
    VpnProtocol VpnProtocol { get; }
    OpenVpnAdapter OpenVpnAdapter { get; }

    event EventHandler<MainSettingsIpcEntity> SettingsChanged;

    void Apply(MainSettingsIpcEntity settings);
}