/*
 * Copyright (c) 2026 Proton AG
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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ProtonVPN.Common.Legacy.OS.Processes;

public static class CommandLineArgumentsParser
{
    private const string PROGRAM_NAME_PLACEHOLDER = "placeholder";

    public static IReadOnlyList<string> Parse(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
        {
            return [];
        }

        // CommandLineToArgvW parses a whole command line, whose first token is the program name and
        // follows different rules than the rest. A throwaway name fills that slot and is skipped below.
        IntPtr argumentsPointer = CommandLineToArgvW($"{PROGRAM_NAME_PLACEHOLDER} {commandLine}", out int count);
        if (argumentsPointer == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        try
        {
            List<string> arguments = [];

            for (int i = 1; i < count; i++)
            {
                IntPtr argumentPointer = Marshal.ReadIntPtr(argumentsPointer, i * IntPtr.Size);
                arguments.Add(Marshal.PtrToStringUni(argumentPointer) ?? string.Empty);
            }

            return arguments;
        }
        finally
        {
            LocalFree(argumentsPointer);
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr CommandLineToArgvW(string commandLine, out int argumentCount);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LocalFree(IntPtr memoryHandle);
}