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
using System.Threading.Tasks;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public interface IVpnConnector
    {
        Profile LastProfile { get; }
        Server LastServer { get; }
        VpnState State { get; }
        bool NetworkBlocked { get; }
        Task<IList<Server>> GetSortedAndValidQuickConnectServersAsync(int? maxServers = null);
        Task QuickConnectAsync(int? maxServers = null);
        Task ConnectToDifferentRandomFreeServerAsync();
        Task ConnectToBestProfileAsync(Profile profile, Profile fallbackProfile = null, int? maxServers = null);
        Task ConnectToPreSortedCandidatesAsync(IReadOnlyCollection<Server> sortedCandidates, VpnProtocol vpnProtocol);
        Task ConnectToProfileAsync(Profile profile, VpnProtocol? vpnProtocol = null);
        void OnVpnStateChanged(VpnStateChangedEventArgs e);
        event EventHandler<VpnStateChangedEventArgs> VpnStateChanged;
    }
}