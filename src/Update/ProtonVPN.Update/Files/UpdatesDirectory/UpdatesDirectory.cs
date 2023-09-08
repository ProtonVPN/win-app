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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;
using SysPath = System.IO.Path;

namespace ProtonVPN.Update.Files.UpdatesDirectory
{
    /// <summary>
    /// Represents directory of downloaded updates, performs cleanup of outdated downloads.
    /// </summary>
    internal class UpdatesDirectory : IUpdatesDirectory
    {
        private const string LOCAL_APP_DATA_REGISTRY_KEY = "Local AppData";
        private const string LOCAL_APP_DATA_REGISTRY_PATH = @"HKEY_USERS\{0}\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";

        private readonly string _path;
        private readonly Version _currentVersion;

        public UpdatesDirectory(string path, Version currentVersion)
        {
            Ensure.NotEmpty(path, nameof(path));

            _path = path;
            _currentVersion = currentVersion;
        }

        public string Path
        {
            get
            {
                Directory.CreateDirectory(_path);
                return _path;
            }
        }

        public void Cleanup()
        {
            PathCleanup(_path);
            foreach (string localAppDataFolder in GetAllLocalAppDataPaths())
            {
                DeletePath(SysPath.Combine(localAppDataFolder, "ProtonVPN", "Updates"));
            }
        }

        public void PathCleanup(string path)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs)
                {
                    Task.Run(() => Directory.Delete(dir, true)).IgnoreExceptions();
                }
                DeleteOldFiles(path, _currentVersion);
            }
            catch
            {
            }
        }

        private static void DeleteOldFiles(string dir, Version currentVersion)
        {
            string[] filePaths = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
            foreach (string filePath in filePaths)
            {
                if (!IsExeFile(filePath) || FileVersion(filePath) < currentVersion)
                {
                    Task.Run(() => File.Delete(filePath)).IgnoreExceptions();
                }
            }
        }

        private static bool IsExeFile(string path)
        {
            return string.Equals(SysPath.GetExtension(path), ".exe", StringComparison.OrdinalIgnoreCase);
        }

        private static Version FileVersion(string path)
        {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
            bool hasVersion = Version.TryParse(info.FileVersion, out Version version);
            return hasVersion && version != null
                ? version.Normalized()
                : new Version(0, 0, 0);
        }

        private IEnumerable<string> GetAllLocalAppDataPaths()
        {
            List<string> localAppDataFolders = new();
            try
            {
                string[] systemUserIds = Registry.Users.GetSubKeyNames();
                foreach (string systemUserId in systemUserIds)
                {
                    string localAppDataFolder = GetUserLocalAppDataPath(systemUserId);
                    if (!string.IsNullOrWhiteSpace(localAppDataFolder))
                    {
                        localAppDataFolders.Add(localAppDataFolder);
                    }
                }
            }
            catch
            {
            }
            return localAppDataFolders;
        }

        private string GetUserLocalAppDataPath(string systemUserId)
        {
            try
            {
                return (string)Registry.GetValue(
                    string.Format(LOCAL_APP_DATA_REGISTRY_PATH, systemUserId), LOCAL_APP_DATA_REGISTRY_KEY, null);
            }
            catch
            {
                return null;
            }
        }

        private void DeletePath(string path)
        {
            try
            {
                bool pathExists = Directory.Exists(path);
                if (pathExists)
                {
                    Directory.Delete(path, recursive: true);
                }
            }
            catch
            {
            }
        }
    }
}