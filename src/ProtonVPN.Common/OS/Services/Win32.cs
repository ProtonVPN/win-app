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
using System.Runtime.InteropServices;

namespace ProtonVPN.Common.OS.Services
{
    public static class Win32
    {
        [Flags]
        public enum ScmAccessRights
        {
            Connect = 0x0001,
            CreateService = 0x0002,
            EnumerateService = 0x0004,
            Lock = 0x0008,
            QueryLockStatus = 0x0010,
            ModifyBootConfig = 0x0020,
            StandardRightsRequired = 0xF0000,

            AllAccess = StandardRightsRequired | Connect | CreateService | EnumerateService | Lock | QueryLockStatus |
                        ModifyBootConfig
        }

        [Flags]
        public enum ServiceAccessRights
        {
            QueryConfig = 0x1,
            ChangeConfig = 0x2,
            QueryStatus = 0x4,
            EnumerateDependants = 0x8,
            Start = 0x10,
            Stop = 0x20,
            PauseContinue = 0x40,
            Interrogate = 0x80,
            UserDefinedControl = 0x100,
            Delete = 0x00010000,
            StandardRightsRequired = 0xF0000,

            AllAccess = (StandardRightsRequired | QueryConfig | ChangeConfig | QueryStatus | EnumerateDependants |
                         Start | Stop | PauseContinue | Interrogate | UserDefinedControl)
        }

        [Flags]
        public enum ServiceStartType
        {
            Boot = 0x00000000,
            System = 0x00000001,
            Auto = 0x00000002,
            Demand = 0x00000003,
            Disabled = 0x00000004
        }

        [Flags]
        public enum ServiceError
        {
            Ignore = 0x00000000,
            Normal = 0x00000001,
            Severe = 0x00000002,
            Critical = 0x00000003
        }

        [Flags]
        public enum ServiceSidType
        {
            None = 0x00000000,
            Unrestricted = 0x00000001,
            Restricted = 0x00000003
        }

        [Flags]
        public enum ServiceType
        {
            KernelDriver = 0x00000001,
            FileSystemDriver = 0x00000002,
            Win32OwnProcess = 0x00000010,
            Win32ShareProcess = 0x00000020,
            InteractiveProcess = 0x00000100
        }

        public enum ServiceConfigType
        {
            Description = 1,
            SidInfo = 5
        }

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode,
            SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName,
            ScmAccessRights dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenService(IntPtr hScManager, string lpServiceName,
            ServiceAccessRights dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseServiceHandle(IntPtr hScObject);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateService(IntPtr hScManager, string lpServiceName, string lpDisplayName,
            ServiceAccessRights dwDesiredAccess, ServiceType dwServiceType, ServiceStartType dwStartType,
            ServiceError dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId,
            string lpDependencies, string lp, string lpPassword);

        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2", CharSet = CharSet.Unicode, SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeServiceConfig2(IntPtr hService, ServiceConfigType dwInfoLevel,
            ref ServiceSidType lpInfo);
    }
}