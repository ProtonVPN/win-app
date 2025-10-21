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

using System.Text.RegularExpressions;
using Microsoft.Win32;
using ProtonVPN.Client.Settings.Contracts.Models;

namespace ProtonVPN.Client.Settings.Contracts.Helpers;

public static class DefaultAppsHelper
{
    private static readonly Regex _filenameRegex = new(@"^\s*""?([^""]*)""?");

    public static IEnumerable<SplitTunnelingApp> GetBrowserApps()
    {
        return GetInternetExplorer()
            .Union(GetMicrosoftEdge())
            .Union(GetClientApps(Registry.LocalMachine, @"SOFTWARE\Clients\StartMenuInternet"))
            .Union(GetClientApps(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Clients\StartMenuInternet"))
            .Union(GetClientApps(Registry.CurrentUser, @"SOFTWARE\Clients\StartMenuInternet"))
            .Distinct();
    }

    private static IEnumerable<SplitTunnelingApp> GetMicrosoftEdge()
    {
        const string microsoftEdgeFolder = @"%WinDir%\SystemApps\Microsoft.MicrosoftEdge_8wekyb3d8bbwe";

        string path = Environment.ExpandEnvironmentVariables(Path.Combine(microsoftEdgeFolder, "GetMicrosoftEdge.exe"));
        if (!IsValidPath(path))
        {
            yield break;
        }

        SplitTunnelingApp app = new(path, false);

        string altPath1 = Environment.ExpandEnvironmentVariables(Path.Combine(microsoftEdgeFolder, "MicrosoftEdgeCP.exe"));
        if (IsValidPath(altPath1))
        {
            app.AlternateAppFilePaths.Add(altPath1);
        }

        string altPath2 = Environment.ExpandEnvironmentVariables(@"%WinDir%\System32\MicrosoftEdgeCP.exe");
        if (IsValidPath(altPath2))
        {
            app.AlternateAppFilePaths.Add(altPath2);
        }

        yield return app;
    }

    private static IEnumerable<SplitTunnelingApp> GetInternetExplorer()
    {
        string? path = GetMachineAppPath(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\IEXPLORE.EXE");
        if (!IsValidPath(path))
        {
            yield break;
        }

        SplitTunnelingApp app = new(path, false);

        string? x86Path = GetMachineAppPath(@"SOFTWARE\Microsoft\Internet Explorer\Main", "x86AppPath");
        if (IsValidPath(x86Path))
        {
            app.AlternateAppFilePaths.Add(x86Path);
        }

        yield return app;
    }

    private static IEnumerable<SplitTunnelingApp> GetClientApps(RegistryKey? parentKey, string subKeyName)
    {
        return FailSafeRegistryAccess(() =>
        {
            using (RegistryKey? subKey = parentKey?.OpenSubKey(subKeyName))
            {
                return GetClientApps(subKey);
            }
        }, Enumerable.Empty<SplitTunnelingApp>());
    }

    private static List<SplitTunnelingApp> GetClientApps(RegistryKey? parentKey)
    {
        List<SplitTunnelingApp> list = [];
        if (parentKey == null)
        {
            return list;
        }

        string[] subKeys = parentKey.GetSubKeyNames();
        foreach (string subKey in subKeys)
        {
            SplitTunnelingApp? app = GetClientApp(parentKey, subKey);
            if (app != null)
            {
                list.Add(app.Value);
            }
        }

        return list;
    }

    private static SplitTunnelingApp? GetClientApp(RegistryKey parentKey, string subKeyName)
    {
        return FailSafeRegistryAccess(() =>
        {
            using (RegistryKey? clientKey = parentKey?.OpenSubKey(subKeyName))
            {
                return GetClientApp(clientKey);
            }
        }, null);
    }

    private static SplitTunnelingApp? GetClientApp(RegistryKey? key)
    {
        if (key == null)
        {
            return null;
        }

        string path = GetClientAppPath(key);
        return IsValidPath(path)
            ? new SplitTunnelingApp(path, false)
            : null;
    }

    private static string GetClientAppPath(RegistryKey? key)
    {
        using (RegistryKey? commandKey = key?.OpenSubKey(@"shell\open\command"))
        {
            string? command = commandKey?.GetValue(null)?.ToString();
            string filename = ParseFilename(command);
            return Environment.ExpandEnvironmentVariables(filename);
        }
    }

    private static string ParseFilename(string? command)
    {
        if (string.IsNullOrEmpty(command))
        {
            return string.Empty;
        }

        Match match = _filenameRegex.Match(command);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    private static bool IsValidPath(string? path)
    {
        return !string.IsNullOrEmpty(path) && File.Exists(path);
    }

    private static string GetMachineAppPath(string subKeyName, string? valueName = null)
    {
        string? path = SafeGetRegistryValue(Registry.LocalMachine, subKeyName, valueName);
        return !string.IsNullOrEmpty(path) && File.Exists(path)
            ? path
            : string.Empty;
    }

    private static string? SafeGetRegistryValue(RegistryKey key, string subKeyName, string? valueName = null)
    {
        return FailSafeRegistryAccess(() => GetRegistryValue(key, subKeyName, valueName), null);
    }

    private static string? GetRegistryValue(RegistryKey key, string subKeyName, string? valueName = null)
    {
        using (RegistryKey? subKey = key.OpenSubKey(subKeyName))
        {
            return subKey?.GetValue(valueName)?.ToString();
        }
    }

    private static T FailSafeRegistryAccess<T>(Func<T> function, T defaultResult)
    {
        try
        {
            return function();
        }
        catch (Exception) { }

        return defaultResult;
    }
}
