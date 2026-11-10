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
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class InstallerHelper
{
    private static string? _installerPath;
    private static readonly string _installerDir = Path.Combine(TestConstants.SolutionRoot, @"Setup\Installers");
    private static string InstallerPath => _installerPath ??= Directory.GetFiles(_installerDir, "ProtonVPN_*.exe").FirstOrDefault() ?? throw new FileNotFoundException($"No installer found in: {_installerDir}");

    static InstallerHelper()
    {
        WindowHelper.EnableDpiAwareness();
    }

    public static Process StartInstaller(string? customInstallerPath = null, string? arguments = null)
    {
        string path = customInstallerPath ?? InstallerPath;

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Installer was not found: {path}");
        }

        ProcessStartInfo processStartInfo = new()
        {
            FileName = path,
            Arguments = arguments ?? string.Empty,
            UseShellExecute = true
        };

        return Process.Start(processStartInfo) ?? throw new InvalidOperationException($"Could not start installer: {path}");
    }

    public static void WaitForWindowToDisappear(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = timeout ?? TestConstants.TwoMinutesTimeout;
        DateTime timeoutDate = DateTime.UtcNow + effectiveTimeout;

        while (DateTime.UtcNow < timeoutDate)
        {
            if (!Process.GetProcesses().Any(process =>
            {
                try
                {
                    return process.MainModule?.FileVersionInfo.FileDescription is "Proton VPN Setup" or "Setup/Uninstall";
                }
                catch
                {
                    return false;
                }
            }))
            {
                return;
            }

            Thread.Sleep(TestConstants.RetryInterval);
        }

        throw new TimeoutException($"Installer window did not disappear within {effectiveTimeout.TotalSeconds} seconds");
    }
}
