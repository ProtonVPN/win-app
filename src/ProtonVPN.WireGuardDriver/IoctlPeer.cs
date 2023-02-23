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

using System.Runtime.InteropServices;
using ProtonVPN.WireGuardDriver.Structs;

namespace ProtonVPN.WireGuardDriver
{
    [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 136)]
    public unsafe struct IoctlPeer
    {
        public IoctlPeerFlags Flags;
        public uint Reserved;
        public fixed byte PublicKey[32];
        public fixed byte PresharedKey[32];
        public ushort PersistentKeepalive;
        public SockAddrInet Endpoint;
        public ulong TxBytes, RxBytes;
        public ulong LastHandshake;
        public uint AllowedIPsCount;
    }
}