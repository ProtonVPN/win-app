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
using System.Configuration;
using System.IO;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Core.Storage;

namespace ProtonVPN.Settings.Migrations.v2_0_2
{
    internal class UserSettingsMigration : BaseUserSettingsMigration
    {
        private readonly ILogger _logger;

        public UserSettingsMigration(ILogger logger, ISettingsStorage userSettings) :
            base(userSettings, "2.0.2")
        {
            _logger = logger;
        }

        protected override void Migrate()
        {
            _logger.Info<AppLog>("Running user setting migration to 2.0.2.");
            try
            {
                RemoveVpnCredentials();
            }
            catch (Exception exception)
            {
                _logger.Error<AppLog>("User setting migration to 2.0.2 failed.", exception);
            }
        }

        private void RemoveVpnCredentials()
        {
            Configuration userConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            if (userConfig.HasFile)
            {
                IList<string> filePaths = GetFilePaths(userConfig.FilePath);
                foreach (string filePath in filePaths)
                {
                    using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    using (StreamReader sr = new(fs))
                    using (StreamWriter sw = new(fs))
                    {
                        string fileContents = sr.ReadToEnd();
                        bool hasDeletedUserVpnUsername = DeleteSettingFromText(ref fileContents, "UserVpnUsername");
                        bool hasDeletedUserVpnPassword = DeleteSettingFromText(ref fileContents, "UserVpnPassword");
                        bool hasDeletedVpnUsername = DeleteSettingFromText(ref fileContents, "VpnUsername");
                        bool hasDeletedVpnPassword = DeleteSettingFromText(ref fileContents, "VpnPassword");
                        if (hasDeletedUserVpnUsername || hasDeletedUserVpnPassword ||
                            hasDeletedVpnUsername || hasDeletedVpnPassword)
                        {
                            fs.SetLength(0);
                            sw.Write(fileContents);
                        }
                    }
                }
            }
        }

        private IList<string> GetFilePaths(string currentUserConfigFilePath)
        {
            string directoryName = Path.GetDirectoryName(currentUserConfigFilePath);
            string hashDirectoryName = Path.GetDirectoryName(directoryName);
            string mainDirectoryName = Path.GetDirectoryName(hashDirectoryName);
            return mainDirectoryName == null
                ? new List<string>()
                : Directory.GetFiles(mainDirectoryName, "user.config", SearchOption.AllDirectories);
        }

        private bool DeleteSettingFromText(ref string fileContents, string setting)
        {
            string startOfSettingName = $"<setting name=\"{setting}\"";
            int indexOfStartOfSettingName = fileContents.IndexOf(startOfSettingName,
                StringComparison.InvariantCultureIgnoreCase);
            if (indexOfStartOfSettingName <= 0)
            {
                return false;
            }

            string endOfSetting = "</setting>";
            int indexOfStartOfEndOfSetting = fileContents.IndexOf(endOfSetting, indexOfStartOfSettingName + 1,
                StringComparison.InvariantCultureIgnoreCase);
            if (indexOfStartOfEndOfSetting <= 0)
            {
                return false;
            }

            int indexOfEndOfSetting = indexOfStartOfEndOfSetting + endOfSetting.Length;
            int settingLength = indexOfEndOfSetting - indexOfStartOfSettingName;
            fileContents = fileContents.Remove(indexOfStartOfSettingName, settingLength);
            return true;
        }
    }
}
