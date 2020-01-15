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

using Microsoft.Win32;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Settings.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;

namespace ProtonVPN.Settings.SplitTunneling
{
    public class InstalledApps
    {
        private const string MicrosoftEdgeName = "Microsoft Edge";
        private const string InternetExplorerName = "Internet Explorer";
        private static readonly string[] BrowsersToInclude =
        {
            MicrosoftEdgeName,
            InternetExplorerName,
            "Google Chrome",
            "Mozilla Firefox"
        };
        private static readonly Regex FilenameRegex = new Regex(@"^\s*""?([^""]*)""?");
        private const string MicrosoftEdgeFolder = @"%WinDir%\SystemApps\Microsoft.MicrosoftEdge_8wekyb3d8bbwe";

        private readonly ILogger _logger;

        public InstalledApps(ILogger logger)
        {
            _logger = logger;
        }

        public SplitTunnelingApp[] All()
        {
            return InternetBrowsers()
                .Where(a => BrowsersToInclude.ContainsIgnoringCase(a.Name))
                .ToArray();
        }

        public SplitTunnelingApp SelectApp()
        {
            var path = SelectAppFile();
            if (!IsValidAppPath(path))
                return null;

            var name = AppName(path);
            if (string.IsNullOrEmpty(name))
                return null;

            return new SplitTunnelingApp { Name = name, Path = path };
        }

        private IEnumerable<SplitTunnelingApp> InternetBrowsers()
        {
            var apps = InternetExplorer()
                .Union(MicrosoftEdge())
                .Union(ClientApps(Registry.LocalMachine, @"SOFTWARE\Clients\StartMenuInternet"))
                .Union(ClientApps(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Clients\StartMenuInternet"))
                .Union(ClientApps(Registry.CurrentUser, @"SOFTWARE\Clients\StartMenuInternet"));

            return apps;
        }

        private IEnumerable<SplitTunnelingApp> ClientApps(RegistryKey parentKey, string subKeyName)
        {
            return FailSafeRegistryAccess(() =>
            {
                var subKey = parentKey.OpenSubKey(subKeyName);
                return ClientApps(subKey);
            },
            Enumerable.Empty<SplitTunnelingApp>);
        }

        private IEnumerable<SplitTunnelingApp> ClientApps(RegistryKey parentKey)
        {
            if (parentKey == null)
                yield break;

            var subKeys = parentKey.GetSubKeyNames();
            foreach (var subKey in subKeys)
            {
                var app = ClientApp(parentKey, subKey);
                if (app != null)
                    yield return app;
            }
        }

        private SplitTunnelingApp ClientApp(RegistryKey parentKey, string subKeyName)
        {
            return FailSafeRegistryAccess(() =>
            {
                using (var clientKey = parentKey.OpenSubKey(subKeyName))
                {
                    return ClientApp(clientKey);
                }
            });
        }

        private SplitTunnelingApp ClientApp(RegistryKey key)
        {
            if (key == null)
                return null;

            var name = ClientAppName(key);
            var path = ClientAppPath(key);

            if (IsValidAppPath(path))
            {
                return new SplitTunnelingApp
                {
                    Name = name,
                    Path = path
                };
            }

            return null;
        }

        private string ClientAppName(RegistryKey key)
        {
            return key?.GetValue(null)?.ToString();
        }

        private string ClientAppPath(RegistryKey key)
        {
            using (var commandKey = key.OpenSubKey(@"shell\open\command"))
            {
                var command = commandKey?.GetValue(null)?.ToString();
                var filename = ParsedFilename(command);
                return Environment.ExpandEnvironmentVariables(filename);
            }
        }

        private IEnumerable<SplitTunnelingApp> MicrosoftEdge()
        {
            var path = Environment.ExpandEnvironmentVariables(Path.Combine(MicrosoftEdgeFolder, "MicrosoftEdge.exe"));

            if (!File.Exists(path))
                yield break;

            var app = new SplitTunnelingApp
            {
                Name = MicrosoftEdgeName,
                Path = path
            };

            var paths = new List<string>();
            path = Environment.ExpandEnvironmentVariables(Path.Combine(MicrosoftEdgeFolder, "MicrosoftEdgeCP.exe"));
            if (File.Exists(path))
                paths.Add(path);

            path = Environment.ExpandEnvironmentVariables(@"%WinDir%\System32\MicrosoftEdgeCP.exe");
            if (File.Exists(path))
                paths.Add(path);

            app.AdditionalPaths = paths.Any() ? paths.ToArray() : null;
            yield return app;
        }

        private IEnumerable<SplitTunnelingApp> InternetExplorer()
        {
            var path = MachineAppPath(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\IEXPLORE.EXE");
            if (string.IsNullOrEmpty(path))
                yield break;

            var x86Path = MachineAppPath(@"SOFTWARE\Microsoft\Internet Explorer\Main", "x86AppPath");

            var app = new SplitTunnelingApp
            {
                Name = InternetExplorerName,
                Path = path,
                AdditionalPaths = !string.IsNullOrEmpty(x86Path) ? new [] {x86Path} : null
            };

            yield return app;
        }

        private string MachineAppPath(string subKeyName, string valueName = null)
        {
            var path = SafeRegistryValue(Registry.LocalMachine, subKeyName, valueName);
            return File.Exists(path) ? path : "";
        }

        private string SafeRegistryValue(RegistryKey key, string subKeyName, string valueName = null)
        {
            return FailSafeRegistryAccess(() => RegistryValue(key, subKeyName, valueName));
        }

        private string RegistryValue(RegistryKey key, string subKeyName, string valueName = null)
        {
            using (var subKey = key.OpenSubKey(subKeyName))
            {
                return subKey?.GetValue(valueName)?.ToString();
            }
        }

        private string ParsedFilename(string command)
        {
            if (string.IsNullOrEmpty(command))
                return string.Empty;

            var match = FilenameRegex.Match(command);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return string.Empty;
        }

        private bool IsValidAppPath(string path)
        {
            try
            {
                return !string.IsNullOrEmpty(path)
                    && Path.IsPathRooted(path)
                    && string.Equals(Path.GetExtension(path), ".exe", StringComparison.OrdinalIgnoreCase)
                    && File.Exists(path);
            }
            catch (ArgumentException) { }

            return false;
        }

        private static T FailSafeRegistryAccess<T>(Func<T> function, Func<T> defaultResult = null)
        {
            try
            {
                return function();
            }
            catch (SecurityException) { }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }

            return defaultResult != null ? defaultResult() : default;
        }

        private string SelectAppFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "App files (*.exe)|*.exe",
                Multiselect = false,
                RestoreDirectory = true
            };
            try
            {
                if (dialog.ShowDialog() == true)
                {
                    return dialog.FileName;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to add custom app file: {ex.Message}");
            }

            return string.Empty;
        }

        private string AppName(string appPath)
        {
            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(appPath);
                var name = versionInfo.FileDescription?.Trim();

                if (string.IsNullOrEmpty(name))
                    name = versionInfo.ProductName?.Trim();

                if (string.IsNullOrEmpty(name))
                    name = Path.GetFileNameWithoutExtension(appPath).Trim();

                return name.LimitLength(200);
            }
            catch (FileNotFoundException) { }
            catch (ArgumentException) { }

            return string.Empty;
        }
    }
}
