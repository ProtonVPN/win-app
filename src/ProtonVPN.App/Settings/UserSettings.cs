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

using System.Collections.Generic;
using System.Linq;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Logging.Categorization.Events.AppUpdateLogs;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Storage;

namespace ProtonVPN.Settings
{
    internal class UserSettings : ISettingsStorage, ISupportsMigration
    {
        private const string SettingsVersionKey = "SettingsVersion";
        private const string UserSettingsMigratedKey = "UserSettingsMigrated";

        private readonly IConfiguration _appConfig;
        private readonly List<IMigration> _migrations = new();
        private readonly ISettingsStorage _storage;
        private readonly ISettingsStorage _appSettings;
        private readonly ILogger _logger;
        private bool _isMigrating;

        public UserSettings(IConfiguration appConfig, PerUserSettings perUserSettings, 
            ISettingsStorage appSettings, ILogger logger)
        {
            _appConfig = appConfig;
            _storage = perUserSettings;
            _appSettings = appSettings;
            _logger = logger;
        }

        public T Get<T>(string key)
        {
            Migrate();

            return _storage.Get<T>(key);
        }

        public void Set<T>(string key, T value)
        {
            Migrate();

            _storage.Set(key, value);
        }

        public void RegisterMigration(IMigration migration)
        {
            _migrations.Add(migration);
        }

        private void Migrate()
        {
            if (_isMigrating)
            {
                return;
            }

            string version = Version;
            if (version == _appConfig.AppVersion)
            {
                return;
            }

            _logger.Info<AppUpdatedLog>("The app was updated from version " +
                $"'{version}' to version '{_appConfig.AppVersion}'.");

            if (MigrationRequired(version))
            {
                _logger.Info<AppLog>("Migrating user settings.");
                _isMigrating = true;

                foreach (IMigration migration in _migrations.OrderBy(m => m.ToVersion))
                {
                    migration.Apply();
                }
            }

            Version = _appConfig.AppVersion;

            _isMigrating = false;
        }

        private bool MigrationRequired(string version)
        {
            return !string.IsNullOrEmpty(version) && !UserSettingsMigrated;
        }

        private string Version
        {
            get => _storage.Get<string>(SettingsVersionKey);
            set => _storage.Set(SettingsVersionKey, value);
        }

        private bool UserSettingsMigrated => _appSettings.Get<bool>(UserSettingsMigratedKey);
    }
}
