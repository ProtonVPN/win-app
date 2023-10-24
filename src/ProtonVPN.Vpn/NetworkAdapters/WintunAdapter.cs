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
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;

namespace ProtonVPN.Vpn.NetworkAdapters;

public class WintunAdapter
{
    private const string WINTUN_DLL = "wintun.dll";
    private const string WINTUN_GUID = "344dba6b-6834-489e-a556-22e36b502107";

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfig;
    private readonly WintunLogger _wintunLogger;

    private IntPtr _adapterHandle;
    private IntPtr _libraryHandle;

    public WintunAdapter(ILogger logger, IStaticConfiguration staticConfig)
    {
        _logger = logger;
        _staticConfig = staticConfig;
        _wintunLogger = LogWintunMessage;
    }

    public bool Create()
    {
        InitializeLibrary();
        if (_libraryHandle == IntPtr.Zero)
        {
            _logger.Error<OperatingSystemLog>($"Failed to initialize wintun library. Error code: {GetLastError()}");
            return false;
        }

        WintunSetLogger(_wintunLogger);

        _adapterHandle = WintunOpenAdapter(_staticConfig.WintunAdapterName);
        if (_adapterHandle == IntPtr.Zero)
        {
            Guid guid = Guid.Parse(WINTUN_GUID);
            _adapterHandle = WintunCreateAdapter("ProtonVPN TUN", "ProtonVPN TUN", ref guid);
            if (_adapterHandle == IntPtr.Zero)
            {
                _logger.Error<OperatingSystemLog>($"Failed to create wintun adapter. Error code: {GetLastError()}");
            }
        }

        return _adapterHandle != IntPtr.Zero;
    }

    public void Close()
    {
        if (_adapterHandle != IntPtr.Zero)
        {
            WintunCloseAdapter(_adapterHandle);
            _adapterHandle = IntPtr.Zero;
        }
    }

    private void InitializeLibrary()
    {
        if (_libraryHandle == IntPtr.Zero)
        {
            _libraryHandle = LoadLibrary(_staticConfig.WintunDriverPath);
        }
    }

    private delegate void WintunLogger(int level, ulong timestamp,
        [MarshalAs(UnmanagedType.LPWStr)] string message);

    private void LogWintunMessage(int level, ulong timestamp, string message)
    {
        string fullMessage = $"Wintun: {message}";
        switch (level)
        {
            case 0:
                _logger.Info<NetworkLog>(fullMessage);
                break;
            case 1:
                _logger.Warn<NetworkLog>(fullMessage);
                break;
            case 2:
                _logger.Error<NetworkLog>(fullMessage);
                break;
            default:
                _logger.Debug<NetworkLog>(fullMessage);
                break;
        }
    }

    [DllImport(WINTUN_DLL, EntryPoint = "WintunCreateAdapter", SetLastError = true)]
    private static extern IntPtr WintunCreateAdapter([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.LPWStr)] string type, ref Guid guid);

    [DllImport(WINTUN_DLL, EntryPoint = "WintunOpenAdapter", SetLastError = true)]
    private static extern IntPtr WintunOpenAdapter(string name);

    [DllImport(WINTUN_DLL, EntryPoint = "WintunCloseAdapter", SetLastError = true)]
    private static extern void WintunCloseAdapter(IntPtr adapterHandle);

    [DllImport(WINTUN_DLL, EntryPoint = "WintunSetLogger", SetLastError = true)]
    private static extern void WintunSetLogger(WintunLogger logger);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string path);

    [DllImport("kernel32.dll")]
    public static extern uint GetLastError();
}