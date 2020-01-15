/*
 * Copyright (c) 2020 Proton Technologies AG
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

using ProtonVPN.Update.Files.Launchable;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace ProtonVPN.UpdateService
{
    internal class ElevatedProcess : ILaunchableFile
    {
        private const int TokenDuplicate = 0x0002;
        private const uint MaximumAllowed = 0x2000000;
        private const int CreateNewConsole = 0x00000010;
        private const int NormalPriorityClass = 0x20;

        public void Launch(string path, string args)
        {
            var dwSessionId = WTSGetActiveConsoleSessionId();
            var logonProcessId = GetWinlogonProcessId(dwSessionId);
            var hProcess = OpenProcess(MaximumAllowed, false, logonProcessId);
            var hPToken = IntPtr.Zero;

            if (!OpenProcessToken(hProcess, TokenDuplicate, ref hPToken))
            {
                CloseHandle(hProcess);
                return;
            }

            var sa = GetSecurityAttributes();
            var hUserTokenDup = IntPtr.Zero;

            if (!DuplicateTokenEx(
                hPToken,
                MaximumAllowed,
                ref sa,
                (int)SecurityImpersonationLevel.SecurityIdentification,
                (int)TokenType.TokenPrimary,
                ref hUserTokenDup))
            {
                CloseHandle(hProcess);
                CloseHandle(hPToken);
                return;
            }

            var commandLine = $"{path} {args}";
            var startupInfo = GetStartupInfo();
            CreateProcessAsUser(
                hUserTokenDup,
                null,
                commandLine,
                ref sa,
                ref sa,
                false,
                NormalPriorityClass | CreateNewConsole,
                IntPtr.Zero,
                null,
                ref startupInfo,
                out _);

            CloseHandle(hProcess);
            CloseHandle(hPToken);
            CloseHandle(hUserTokenDup);
        }

        private Startupinfo GetStartupInfo()
        {
            var si = new Startupinfo();
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = @"winsta0\default";

            return si;
        }

        private SecurityAttributes GetSecurityAttributes()
        {
            var sa = new SecurityAttributes();
            sa.Length = Marshal.SizeOf(sa);

            return sa;
        }

        private uint GetWinlogonProcessId(uint currentSessionId)
        {
            uint id = 0;

            var processes = Process.GetProcessesByName("winlogon");
            foreach (var p in processes)
            {
                if ((uint)p.SessionId == currentSessionId)
                {
                    id = (uint)p.Id;
                }
            }

            return id;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SecurityAttributes
        {
            public int Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Startupinfo
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
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

        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessInformation
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        private enum TokenType
        {
            TokenPrimary = 1
        }

        private enum SecurityImpersonationLevel
        {
            SecurityIdentification = 1
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hSnapshot);

        [DllImport("kernel32.dll")]
        private static extern uint WTSGetActiveConsoleSessionId();

        [DllImport(
            "advapi32.dll",
            EntryPoint = "CreateProcessAsUser",
            SetLastError = true,
            CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SecurityAttributes lpProcessAttributes,
            ref SecurityAttributes lpThreadAttributes,
            bool bInheritHandle,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref Startupinfo lpStartupInfo,
            out ProcessInformation lpProcessInformation);

        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
        private static extern bool DuplicateTokenEx(
            IntPtr hExistingToken,
            uint dwDesiredAccess,
            ref SecurityAttributes lpThreadAttributes,
            int tokenType,
            int impersonationLevel,
            ref IntPtr phNewToken);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("advapi32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, ref IntPtr tokenHandle);
    }
}
