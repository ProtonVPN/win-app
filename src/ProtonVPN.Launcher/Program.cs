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
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ProtonVPN.Launcher;

internal class Program
{
    private const string CLIENT_PROCESS_NAME = "ProtonVPN.Client";
    private const string CLIENT_EXE_NAME = "ProtonVPN.Client.exe";

    static void Main(string[] args)
    {
        string? clientExePath = IsClientInstanceAlive()
            ? GetRunningClientExePath() ?? GetLatestClientExePath()
            : GetLatestClientExePath();

        if (!string.IsNullOrEmpty(clientExePath))
        {
            Process.Start(clientExePath, args);
        }
    }

    private static bool IsClientInstanceAlive()
    {
        Mutex? mutex = null;
        try
        {
            return Mutex.TryOpenExisting(AppInstanceConstants.SINGLE_INSTANCE_MUTEX_NAME, out mutex);
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            mutex?.Dispose();
        }
    }

    private static string? GetRunningClientExePath()
    {
        foreach (Process process in Process.GetProcessesByName(CLIENT_PROCESS_NAME))
        {
            try
            {
                return process.MainModule?.FileName;
            }
            catch
            {
            }
            finally
            {
                process.Dispose();
            }
        }

        return null;
    }

    private static string? GetLatestClientExePath()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string[] folders = Directory.GetDirectories(baseDirectory, "v*", SearchOption.TopDirectoryOnly);
        Version latestVersion = new(0, 0, 0);
        foreach (string path in folders)
        {
            string versionString = new DirectoryInfo(path).Name.Replace("v", string.Empty);
            if (Version.TryParse(versionString, out Version? version))
            {
                if (version > latestVersion)
                {
                    latestVersion = version;
                }
            }
        }

        return latestVersion > new Version(0, 0, 0)
            ? Path.Combine(baseDirectory, $"v{latestVersion}", CLIENT_EXE_NAME)
            : null;
    }
}