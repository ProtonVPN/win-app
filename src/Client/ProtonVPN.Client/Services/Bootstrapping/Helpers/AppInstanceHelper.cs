/*
 * Copyright (c) 2025 Proton AG
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
using System.Security.Principal;
using Microsoft.Windows.AppLifecycle;

namespace ProtonVPN.Client.Services.Bootstrapping.Helpers;

/// <summary>
/// Documentation on how to create single-instanced application 
/// and why we should redirect protocol activation to the running instance can be found here:
/// https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/applifecycle/applifecycle-single-instance
/// </summary>
public static class AppInstanceHelper
{
    private const int SW_SHOW = 5;

    private static IntPtr _redirectEventHandle = IntPtr.Zero;

    private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    public static bool IsRunningAsAdmin()
    {
        try
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    public static void RedirectActivation()
    {
        AppInstance? instance = AppInstance.GetInstances().FirstOrDefault(i => !i.IsCurrent);
        if (instance is not null)
        {
            AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();

            RedirectActivationTo(instance, args);
        }
    }

    private static void RedirectActivationTo(AppInstance keyInstance, AppActivationArguments args)
    {
        _redirectEventHandle = CreateEvent(IntPtr.Zero, true, false, string.Empty);
        Task.Run(() =>
        {
            keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
            SetEvent(_redirectEventHandle);
        });

        uint CWMO_DEFAULT = 0;
        uint INFINITE = 0xFFFFFFFF;
        _ = CoWaitForMultipleObjects(
           CWMO_DEFAULT, INFINITE, 1,
           [_redirectEventHandle], out uint handleIndex);

        // Bring the window to the foreground
        BringToForeground();
    }

    private static void BringToForeground()
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
                    title.Equals(App.APPLICATION_NAME, StringComparison.InvariantCulture))
                {
                    mainWindowHandle = hWnd;
                    return false;
                }
            }

            return true;
        }, nint.Zero);

        return mainWindowHandle;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

    [DllImport("kernel32.dll")]
    private static extern bool SetEvent(IntPtr hEvent);

    [DllImport("ole32.dll")]
    private static extern uint CoWaitForMultipleObjects(uint dwFlags, uint dwMilliseconds, ulong nHandles, IntPtr[] pHandles, out uint dwIndex);

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