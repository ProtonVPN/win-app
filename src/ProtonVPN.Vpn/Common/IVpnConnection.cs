﻿/*
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

using System;
using System.Collections.Generic;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.Vpn;

namespace ProtonVPN.Vpn.Common;

public interface IVpnConnection
{
    event EventHandler<EventArgs<VpnState>> StateChanged;
    event EventHandler<ConnectionDetails> ConnectionDetailsChanged;

    TrafficBytes Total { get; }

    void Connect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnCredentials credentials);
    void ResetConnection();
    void Disconnect(VpnError error = VpnError.None);
    void SetFeatures(VpnFeatures vpnFeatures);
    void RequestNetShieldStats();
}