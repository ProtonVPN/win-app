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

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ProtonVPN.Common.PortForwarding;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Vpn
{
    public interface IVpnServiceManager
    {
        Task Connect(VpnConnectionRequest connectionRequest);
        Task UpdateAuthCertificate(string certificate);
        Task Disconnect(VpnError vpnError,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0);
        Task<InOutBytes> Total();
        Task RepeatState();
        void RegisterVpnStateCallback(Action<VpnStateChangedEventArgs> onVpnStateChanged);
        void RegisterServiceSettingsStateCallback(Action<ServiceSettingsStateChangedEventArgs> onServiceSettingsStateChanged);
        void RegisterPortForwardingStateCallback(Action<PortForwardingState> callback);
        void RegisterConnectionDetailsChangeCallback(Action<ConnectionDetails> callback);
    }
}