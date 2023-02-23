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

using System.Linq;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Storage;

namespace ProtonVPN.Settings.Migrations.v1_27_1
{
    internal class AppSettingsMigration : BaseAppSettingsMigration
    {
        private const string UserAutoConnectKey = "UserAutoConnect";
        private const string StartOnStartupKey = "StartOnStartup";

        private readonly InitialAppSettingsMigration _initialAppSettingsMigration;

        public AppSettingsMigration(ISettingsStorage appSettings,
            InitialAppSettingsMigration initialAppSettingsMigration) : base(appSettings, "1.27.1")
        {
            _initialAppSettingsMigration = initialAppSettingsMigration;
        }

        protected override void Migrate()
        {
            MigrateAutoConnect();
            MigrateStartOnStartup();
            MigrateStartMinimized();
        }

        private void MigrateAutoConnect()
        {
            PerUser<string>[] autoConnectSettings = Settings.Get<PerUser<string>[]>(UserAutoConnectKey);
            if (autoConnectSettings != null)
            {
                bool autoConnect = autoConnectSettings.Any(setting => !setting.Value.IsNullOrEmpty());
                Settings.Set(nameof(IAppSettings.ConnectOnAppStart), _initialAppSettingsMigration.IsCleanInstall || autoConnect);
            }
        }

        private void MigrateStartOnStartup()
        {
            bool startOnStartup = Settings.Get<bool>(StartOnStartupKey);
            bool startOnBoot = _initialAppSettingsMigration.IsCleanInstall || startOnStartup;
            Settings.Set(nameof(IAppSettings.StartOnBoot), startOnBoot);
        }

        private void MigrateStartMinimized()
        {
            StartMinimizedMode startMinimized = StartMinimizedMode.ToTaskbar;
            if (!_initialAppSettingsMigration.IsCleanInstall)
            {
                startMinimized = Settings.Get<StartMinimizedMode>(nameof(IAppSettings.StartMinimized));
            }

            Settings.Set(nameof(IAppSettings.StartMinimized), startMinimized);
        }
    }
}