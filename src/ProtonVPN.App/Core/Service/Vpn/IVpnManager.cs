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
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public interface IVpnManager
    {
        Task ConnectAsync(Profile profile,  Profile fallbackProfile = null,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "", 
            [CallerLineNumber] int sourceLineNumber = 0);

        Task QuickConnectAsync(
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "", 
            [CallerLineNumber] int sourceLineNumber = 0);

        Task ReconnectAsync(VpnReconnectionSettings settings = null,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "", 
            [CallerLineNumber] int sourceLineNumber = 0);

        Task DisconnectAsync(VpnError vpnError = VpnError.None,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "", 
            [CallerLineNumber] int sourceLineNumber = 0);

        Task GetStateAsync();

        void OnVpnStateChanged(VpnStateChangedEventArgs e);

        event EventHandler<VpnStateChangedEventArgs> VpnStateChanged;
    }
}