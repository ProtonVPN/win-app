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

using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Storage;

namespace ProtonVPN.Settings.Migrations.v1_7_2
{
    internal class AppSettingsMigration : BaseAppSettingsMigration
    {
        public AppSettingsMigration(ISettingsStorage appSettings): base(appSettings, "1.7.2")
        {
        }

        protected override void Migrate()
        {
            MigrateStartMinimized();
        }

        private void MigrateStartMinimized()
        {
            const string key = nameof(IAppSettings.StartMinimized);
            var startMinimized = Settings.Get<string>(key);

            switch (startMinimized)
            {
                case "True":
                    Settings.Set(key, StartMinimizedMode.ToTaskbar);
                    break;
                case "False":
                    Settings.Set(key, StartMinimizedMode.Disabled);
                    break;
                case "1":
                    Settings.Set(key, StartMinimizedMode.ToTaskbar);
                    break;
                case "2":
                    Settings.Set(key, StartMinimizedMode.ToSystray);
                    break;
                default:
                    Settings.Set(key, StartMinimizedMode.Disabled);
                    break;
            }
        }
    }
}
