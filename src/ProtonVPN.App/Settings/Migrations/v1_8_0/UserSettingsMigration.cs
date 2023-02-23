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

using ProtonVPN.Core.Storage;

namespace ProtonVPN.Settings.Migrations.v1_8_0
{
    public class UserSettingsMigration : BaseUserSettingsMigration
    {
        private ISettingsStorage AppSettings { get; }

        public UserSettingsMigration(ISettingsStorage appSettings, ISettingsStorage userSettings) :
            base(userSettings, "1.8.0")
        {
            AppSettings = appSettings;
        }

        protected override void Migrate()
        {
            MigrateSecureCoreMode();
        }

        private void MigrateSecureCoreMode()
        {
            MigrateToPerUser<bool>("SecureCore");
        }

        private void MigrateToPerUser<T>(string key)
        {
            Settings.Set(key, AppSettings.Get<T>(key));
        }
    }
}
