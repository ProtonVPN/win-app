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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using PInvoke;

namespace ProtonVPN.Core.Startup
{
    public class ElevatedApplication
    {
        public static void LaunchAsUser()
        {
            var fileName = AppExePath();
            if (string.IsNullOrEmpty(fileName))
                return;

            try
            {
                LaunchAsUser(fileName, "");
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        public static bool RunningAsSystem()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
         
                return identity.IsSystem;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        private static string AppExePath()
        {
            return Assembly.GetEntryAssembly()?.Location;
        }

        private static void LaunchAsUser(string path, string args)
        {
            int sessionId;
            try
            {
                sessionId = Process.GetCurrentProcess().SessionId;
            }
            catch (Exception ex) when (ex is NullReferenceException || ex is InvalidOperationException)
            {
                return;
            }

            var logonProcessId = GetProcessId("explorer", sessionId);
            if (logonProcessId == 0)
                return;

            var processHandle = Kernel32.OpenProcess(
                (uint)Kernel32.ACCESS_MASK.SpecialRight.MAXIMUM_ALLOWED,
                false,
                logonProcessId);

            if (!OpenProcessToken(
                processHandle.DangerousGetHandle(),
                (uint)Kernel32.ACCESS_MASK.DesktopSpecificRight.DESKTOP_CREATEWINDOW,
                out var processTokenHandle))
            {
                processHandle.Close();
                return;
            }

            processHandle.Close();

            var sa = Kernel32.SECURITY_ATTRIBUTES.Create();

            if (!DuplicateTokenEx(
                processTokenHandle,
                (uint)Kernel32.ACCESS_MASK.SpecialRight.MAXIMUM_ALLOWED,
                ref sa,
                Kernel32.SECURITY_IMPERSONATION_LEVEL.SecurityIdentification,
                TOKEN_TYPE.TokenPrimary,
                out var userTokenHandle))
            {
                processTokenHandle.Close();
                return;
            }

            processTokenHandle.Close();

            var creationFlags = Kernel32.CreateProcessFlags.NORMAL_PRIORITY_CLASS | Kernel32.CreateProcessFlags.CREATE_NEW_CONSOLE;

            var env = IntPtr.Zero;
            if (!CreateEnvironmentBlock(ref env, userTokenHandle.DangerousGetHandle(), false))
            {
                userTokenHandle.Close();
                return;
            }

            creationFlags |= Kernel32.CreateProcessFlags.CREATE_UNICODE_ENVIRONMENT;

            var startupInfo = StartupInfo();

            if (!CreateProcessAsUser(
                userTokenHandle.DangerousGetHandle(),
                path,
                args,
                ref sa,
                ref sa,
                false,
                creationFlags,
                env,
                null,
                ref startupInfo,
                out _))
            {
                Console.WriteLine(Marshal.GetLastWin32Error());
            }

            DestroyEnvironmentBlock(env);
            userTokenHandle.Close();
        }

        private static STARTUPINFO StartupInfo()
        {
            return new STARTUPINFO
            {
                lpDesktop = @"winsta0\default"
            };
        }

        private static int GetProcessId(string processName, int sessionId)
        {
            var processes = Process.GetProcessesByName(processName);

            return processes
                .Where(p => p.SessionId == sessionId)
                .Select(p => p.Id)
                .FirstOrDefault();
        }

        #region PInvoke

        // ReSharper disable IdentifierTypo
        // ReSharper disable InconsistentNaming
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable FieldCanBeMadeReadOnly.Local

        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
        {
            public int cb;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpReserved;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpDesktop;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        private enum TOKEN_TYPE
        {
            TokenPrimary = 1,
        }

        [DllImport("advapi32.dll")]
        private static extern bool OpenProcessToken(
            IntPtr processHandle,
            Kernel32.ACCESS_MASK desiredAccess,
            out Kernel32.SafeObjectHandle tokenHandle);

        [DllImport("advapi32.dll")]
        private static extern bool DuplicateTokenEx(
            Kernel32.SafeObjectHandle hExistingToken,
            Kernel32.ACCESS_MASK dwDesiredAccess,
            ref Kernel32.SECURITY_ATTRIBUTES lpTokenAttributes,
            Kernel32.SECURITY_IMPERSONATION_LEVEL impersonationLevel,
            TOKEN_TYPE tokenType,
            out Kernel32.SafeObjectHandle phNewToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref Kernel32.SECURITY_ATTRIBUTES lpProcessAttributes,
            ref Kernel32.SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandle,
            Kernel32.CreateProcessFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out Kernel32.PROCESS_INFORMATION lpProcessInformation);

        [DllImport("userenv.dll", SetLastError = true)]
        private static extern bool CreateEnvironmentBlock(ref IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        [DllImport("userenv.dll", SetLastError = true)]
        private static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

        // ReSharper restore InconsistentNaming
        // ReSharper restore IdentifierTypo
        // ReSharper restore MemberCanBePrivate.Local
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        #endregion
    }
}
