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

using ProtonVPN.Common.Abstract;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace ProtonVPN.Core.Network
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class WlanApi
    {
        public const int ERROR_SUCCESS = 0;

        public static Result<WlanConnectionAttributes> GetInterfaceInfo(IntPtr client, Guid guid)
        {
            var code = WlanQueryInterface(client, guid, WlanIntfOpcode.CurrentConnection, IntPtr.Zero, out int num, out IntPtr ptr, out _);
            //if (code != 0x0000139F)
            if (code == ERROR_SUCCESS)
            {
                try
                {
                    return Result.Ok((WlanConnectionAttributes)Marshal.PtrToStructure(ptr, typeof(WlanConnectionAttributes)));
                }
                finally
                {
                    WlanFreeMemory(ptr);
                }
            }

            return Result.Fail<WlanConnectionAttributes>();
        }

        public enum Dot11AuthAlgorithm : uint
        {
            IEEE80211_Open = 1,
            IEEE80211_SharedKey = 2,
            IHV_End = 0xffffffff,
            IHV_Start = 0x80000000,
            RSNA = 6,
            RSNA_PSK = 7,
            WPA = 3,
            WPA_None = 5,
            WPA_PSK = 4
        }

        public enum Dot11BssType
        {
            Any = 3,
            Independent = 2,
            Infrastructure = 1
        }

        public enum Dot11CipherAlgorithm : uint
        {
            CCMP = 4,
            IHV_End = 0xffffffff,
            IHV_Start = 0x80000000,
            None = 0,
            RSN_UseGroup = 0x100,
            TKIP = 2,
            WEP = 0x101,
            WEP104 = 5,
            WEP40 = 1,
            WPA_UseGroup = 0x100
        }

        public enum Dot11PhyType : uint
        {
            Any = 0,
            Dsss = 2,
            Erp = 6,
            Fhss = 1,
            Hrdsss = 5,
            Ht = 7,
            IhvEnd = 0xffffffff,
            IhvStart = 0x80000000,
            IrBaseband = 3,
            Ofdm = 4,
            Unknown = 0
        }

        public enum WlanConnectionMode
        {
            Profile,
            TemporaryProfile,
            DiscoverySecure,
            DiscoveryUnsecure,
            Auto,
            Invalid
        }

        public enum WlanInterfaceState
        {
            NotReady,
            Connected,
            AdHocNetworkFormed,
            Disconnecting,
            Disconnected,
            Associating,
            Discovering,
            Authenticating
        }

        public enum WlanIntfOpcode
        {
            AutoconfEnabled = 1,
            BackgroundScanEnabled = 2,
            BssType = 5,
            ChannelNumber = 8,
            CurrentConnection = 7,
            CurrentOperationMode = 12,
            IhvEnd = 0x3fffffff,
            IhvStart = 0x30000000,
            InterfaceState = 6,
            MediaStreamingMode = 3,
            RadioState = 4,
            Rssi = 0x10000102,
            SecurityEnd = 0x2fffffff,
            SecurityStart = 0x20010000,
            Statistics = 0x10000101,
            SupportedAdhocAuthCipherPairs = 10,
            SupportedCountryOrRegionStringList = 11,
            SupportedInfrastructureAuthCipherPairs = 9
        }

        public enum WlanOpcodeValueType
        {
            QueryOnly,
            SetByGroupPolicy,
            SetByUser,
            Invalid
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Dot11Ssid
        {
            public readonly uint SSIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public readonly byte[] SSID;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WlanAssociationAttributes
        {
            private readonly Dot11Ssid dot11Ssid;
            private readonly Dot11BssType dot11BssType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            private readonly byte[] dot11Bssid;
            private readonly Dot11PhyType dot11PhyType;
            private readonly uint dot11PhyIndex;
            private readonly uint wlanSignalQuality;
            private readonly uint rxRate;
            private readonly uint txRate;

            public PhysicalAddress Dot11Bssid
            {
                get
                {
                    return dot11Bssid != null
                               ? new PhysicalAddress(dot11Bssid)
                               : new PhysicalAddress(new byte[] { 0, 0, 0, 0, 0, 0 });
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WlanConnectionAttributes
        {
            public readonly WlanInterfaceState isState;
            public readonly WlanConnectionMode wlanConnectionMode;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
            public readonly string profileName;
            public readonly WlanAssociationAttributes wlanAssociationAttributes;
            public readonly WlanSecurityAttributes wlanSecurityAttributes;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WlanInterfaceInfo
        {
            public Guid interfaceGuid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
            public readonly string interfaceDescription;
            private readonly WlanInterfaceState isState;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WlanInterfaceInfoListHeader
        {
            public readonly uint numberOfItems;
            private readonly uint index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WlanSecurityAttributes
        {
            [MarshalAs(UnmanagedType.Bool)]
            public readonly bool securityEnabled;
            [MarshalAs(UnmanagedType.Bool)]
            private readonly bool oneXEnabled;
            private readonly Dot11AuthAlgorithm dot11AuthAlgorithm;
            private readonly Dot11CipherAlgorithm dot11CipherAlgorithm;
        }

        [DllImport("wlanapi.dll")]
        public static extern int WlanEnumInterfaces([In] IntPtr clientHandle, [In, Out] IntPtr pReserved, out IntPtr ppInterfaceList);

        [DllImport("wlanapi.dll")]
        public static extern void WlanFreeMemory(IntPtr pMemory);

        [DllImport("wlanapi.dll")]
        public static extern int WlanOpenHandle([In] uint clientVersion, [In, Out] IntPtr pReserved, out uint negotiatedVersion, out IntPtr clientHandle);

        [DllImport("wlanapi.dll")]
        public static extern int WlanCloseHandle([In] IntPtr clientHandle, [In, Out] IntPtr pReserved);

        [DllImport("wlanapi.dll")]
        public static extern int WlanQueryInterface([In] IntPtr clientHandle, [In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid, [In] WlanIntfOpcode opCode, [In, Out] IntPtr pReserved, out int dataSize, out IntPtr ppData, out WlanOpcodeValueType wlanOpcodeValueType);
    }
}
