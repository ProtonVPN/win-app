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

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProtonVPN.Client.Services.Bootstrapping.Helpers;

public static class AppInstanceHelper
{
    private const string APPLICATION_NAME = "Proton VPN";
    private const int SW_SHOW = 5;

    private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    public static void BringToForeground()
    {
        Process currentProcess = Process.GetCurrentProcess();
        List<Process> processes = Process.GetProcessesByName(currentProcess.ProcessName)
            .Where(p => p.Id != currentProcess.Id).ToList();

        foreach (Process process in processes)
        {
            BringProcessMainWindowToForeground(process);
        }
    }

    private static void BringProcessMainWindowToForeground(Process process)
    {
        try
        {
            nint mainWindowHandle = GetProcessMainWindowHandle(process);
            if (mainWindowHandle != nint.Zero)
            {
                ShowWindow(mainWindowHandle, SW_SHOW);
                SwitchToThisWindow(mainWindowHandle, true);
            }
        }
        catch
        { }
    }

    private static nint GetProcessMainWindowHandle(Process process)
    {
        return process.MainWindowHandle == nint.Zero
            ? GetHiddenMainWindowHandleByProcessId(process.Id)
            : process.MainWindowHandle;
    }

    private static nint GetHiddenMainWindowHandleByProcessId(int processId)
    {
        nint mainWindowHandle = nint.Zero;

        EnumWindows((hWnd, lParam) =>
        {
            GetWindowThreadProcessId(hWnd, out uint windowProcessId);

            if (windowProcessId == processId)
            {
                int length = GetWindowTextLength(hWnd);
                nint buffer = Marshal.AllocHGlobal(length + 1);
                GetWindowText(hWnd, buffer, length + 1);
                string? title = Marshal.PtrToStringUni(buffer);
                Marshal.FreeHGlobal(buffer);
                if (title is not null &&
                    title.Equals(APPLICATION_NAME, StringComparison.InvariantCulture))
                {
                    mainWindowHandle = hWnd;
                    return false;
                }
            }

            return true;
        }, nint.Zero);

        return mainWindowHandle;
    }

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, nint lParam);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowTextLength(nint hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(nint hWnd, nint lpString, int nMaxCount);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SwitchToThisWindow(nint hWnd, bool fAltTab);
}