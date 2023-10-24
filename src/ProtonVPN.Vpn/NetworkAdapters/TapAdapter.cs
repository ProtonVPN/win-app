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
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;

namespace ProtonVPN.Vpn.NetworkAdapters;

public class TapAdapter
{
    private const string INSTALL_ACTIONS_DLL = "ProtonVPN.InstallActions.dll";
    private IntPtr _libraryHandle;
    private readonly IStaticConfiguration _staticConfig;
    private readonly ILogger _logger;
    private readonly Logger _nativeLogger;

    public TapAdapter(IStaticConfiguration staticConfig, ILogger logger)
    {
        _staticConfig = staticConfig;
        _logger = logger;
        _nativeLogger = LogMessage;
    }

    public void Create()
    {
        if (InitializeLibrary())
        {
            ulong result = InstallTapAdapter(_staticConfig.OpenVpn.TapInstallerDir);
            if (result != 0)
            {
                _logger.Error<OperatingSystemLog>($"Failed to install TAP adapter. Error code: {result}");
            }
        }
    }

    private bool InitializeLibrary()
    {
        if (_libraryHandle == IntPtr.Zero)
        {
            _libraryHandle = LoadLibrary(_staticConfig.InstallActionsPath);
            if (_libraryHandle == IntPtr.Zero)
            {
                _logger.Error<OperatingSystemLog>($"Failed to initialize {_staticConfig.InstallActionsPath}. Error code: {GetLastError()}");
                return false;
            }

            InitLogger(_nativeLogger);
        }

        return true;
    }

    private void LogMessage(IntPtr ptr)
    {
        string message = Marshal.PtrToStringAuto(ptr);
        _logger.Info<OperatingSystemLog>(message);
    }

    private delegate void Logger(IntPtr messagePtr);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string path);

    [DllImport("kernel32.dll")]
    public static extern uint GetLastError();

    [DllImport(INSTALL_ACTIONS_DLL, EntryPoint = "InstallTapAdapter", SetLastError = true)]
    private static extern ulong InstallTapAdapter([MarshalAs(UnmanagedType.LPWStr)] string tapInstallerDir);

    [DllImport(INSTALL_ACTIONS_DLL, EntryPoint = "InitLogger", SetLastError = true)]
    private static extern ulong InitLogger(Logger logger);
}