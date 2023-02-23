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
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Storage;

namespace ProtonVPN.Settings.Migrations
{
    public abstract class BaseSettingsMigration : IMigration
    {
        private const string SettingsVersionKey = "SettingsVersion";

        protected readonly ISettingsStorage Settings;

        protected BaseSettingsMigration(ISettingsStorage settings, string toVersion)
        {
            Settings = settings;
            ToVersion = Version.Parse(toVersion);
        }

        public Version ToVersion { get; }

        public void Apply()
        {
            if (SettingsVersion >= ToVersion)
                return;

            Migrate();

            SettingsVersion = ToVersion;
        }

        protected abstract void Migrate();

        protected Version SettingsVersion
        {
            get => GetVersion(Settings.Get<string>(SettingsVersionKey));
            set => Settings.Set(SettingsVersionKey, value.ToString());
        }

        private static Version GetVersion(string value)
        {
            return string.IsNullOrEmpty(value) ? new Version() : Version.Parse(value);
        }
    }
}