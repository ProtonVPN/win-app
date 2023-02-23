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
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.BugReporting.Diagnostic
{
    public class InstalledAppsLog : BaseLog
    {
        private const string UninstallRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string WowUninstallRegistryPath = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string WindowsAppsRegistryPath = @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages";

        private const string ProgramTag = "[Program]";
        private const string WindowsAppTag = "[WindowsApp]";

        public InstalledAppsLog(IConfiguration configuration) 
            : base(configuration.DiagnosticsLogFolder, "Apps.txt")
        {
        }

        public override void Write()
        {
            File.WriteAllText(Path, GenerateContent());
        }

        private string GenerateContent()
        {
            List<string> appNames = GetAppNames();
            StringBuilder result = new StringBuilder();

            appNames.Distinct()
                .OrderBy(appName => appName)
                .ForEach((appName) => result.AppendLine(appName));

            return result.ToString();
        }

        private List<string> GetAppNames()
        {
            List<string> appNames = new List<string>();
            RegistryKey registryFolder;

            registryFolder = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(UninstallRegistryPath);
            appNames.AddRange(GetInstalledNames(registryFolder, ParseProgramNameIfExists));

            registryFolder = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(UninstallRegistryPath);
            appNames.AddRange(GetInstalledNames(registryFolder, ParseProgramNameIfExists));

            registryFolder = Registry.LocalMachine.OpenSubKey(WowUninstallRegistryPath);
            appNames.AddRange(GetInstalledNames(registryFolder, ParseProgramNameIfExists));

            registryFolder = Registry.CurrentUser.OpenSubKey(UninstallRegistryPath);
            appNames.AddRange(GetInstalledNames(registryFolder, ParseProgramNameIfExists));

            registryFolder = Registry.CurrentUser.OpenSubKey(WindowsAppsRegistryPath);
            appNames.AddRange(GetInstalledNames(registryFolder, ParseWindowsAppNameIfExists));

            return appNames;
        }

        private IList<string> GetInstalledNames(RegistryKey registryFolder, Func<RegistryKey, string> installedNameParser)
        {
            IList<string> names = new List<string>();

            if (registryFolder == null)
            {
                return names;
            }

            foreach (string registryFolderChild in registryFolder.GetSubKeyNames())
            {
                RegistryKey subKey = registryFolder.OpenSubKey(registryFolderChild);
                string name = installedNameParser(subKey);
                if (name != null)
                {
                    names.Add(name);
                }
            }

            return names;
        }

        private string ParseProgramNameIfExists(RegistryKey key)
        {
            string displayName = $"{key.GetValue("DisplayName")}";
            if (string.IsNullOrEmpty(displayName))
            {
                return null;
            }

            return displayName + ParseProgramVersion(key) + $" {ProgramTag}";
        }

        private string ParseProgramVersion(RegistryKey key)
        {
            string displayVersion = $"{key.GetValue("DisplayVersion")}";

            if (string.IsNullOrEmpty(displayVersion))
            {
                return string.Empty;
            }

            return $" [Version {displayVersion}]";
        }

        private string ParseWindowsAppNameIfExists(RegistryKey key)
        {
            string displayName = $"{key.GetValue("DisplayName")}";
            string packageId = $"{key.GetValue("PackageID")}";

            if (string.IsNullOrEmpty(displayName) && string.IsNullOrEmpty(packageId))
            {
                return null;
            }

            return ParseWindowsAppName(key, displayName, packageId) + $" {WindowsAppTag}";
        }

        private string ParseWindowsAppName(RegistryKey key, string displayName, string packageId)
        {
            if (string.IsNullOrEmpty(packageId))
            {
                return $"{displayName} [Only DisplayName available]";
            }

            if (string.IsNullOrEmpty(displayName))
            {
                return $"{packageId} [Only PackageID available]";
            }

            return ParseWindowsAppNameWithCompleteData(key, displayName, packageId);
        }

        private string ParseWindowsAppNameWithCompleteData(RegistryKey key, string displayName, string packageId)
        {
            string packageRootFolder = $"{key.GetValue("PackageRootFolder")}";
            string packageRootFolderTag = string.IsNullOrEmpty(packageRootFolder) ? string.Empty : $" [PackageRootFolder {packageRootFolder}]";

            if (displayName.StartsWith("@{"))
            {
                try
                {
                    string name = GetCleanWindowsAppName(displayName: displayName, packageRootFolder: packageRootFolder);
                    return $"{name} [DisplayName {displayName}]{packageRootFolderTag}";
                }
                catch
                {
                }
            }

            return $"{displayName} [PackageID {packageId}]{packageRootFolderTag}";
        }

        private string GetCleanWindowsAppName(string displayName, string packageRootFolder)
        {
            string name = GetCleanMicrosoftResourceDisplayName(displayName);
            bool isValidGuid = Guid.TryParse(name, out _);
            if (isValidGuid && !string.IsNullOrEmpty(packageRootFolder))
            {
                name = System.IO.Path.GetFileName(packageRootFolder);
            }

            return name;
        }

        private string GetCleanMicrosoftResourceDisplayName(string displayName)
        {
            const string msResourceString = "ms-resource://";
            int nameStartIndex = displayName.IndexOf(msResourceString, StringComparison.InvariantCultureIgnoreCase) + msResourceString.Length;
            string name = displayName.Substring(nameStartIndex);
            int nameEndIndex = name.IndexOf("/", StringComparison.InvariantCultureIgnoreCase);
            return name.Substring(0, nameEndIndex);
        }
    }
}
