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
using ProtonVPN.Core.Settings.Contracts;
using ProtonVPN.Core.Storage;

namespace ProtonVPN.Settings.Migrations.v1_18_3
{
    internal class AppSettingsMigration : BaseAppSettingsMigration
    {
        private const string UserSettingsVersionKey = "UserSettingsVersion";
        private const string CustomDnsEnabledKey = "CustomDnsEnabled";
        private const string CustomDnsIpsKey = "CustomDnsIps";
        private const string UserCustomDnsEnabledKey = "UserCustomDnsEnabled";
        private const string UserCustomDnsIpsKey = "UserCustomDnsIps";

        public AppSettingsMigration(ISettingsStorage appSettings)
            : base(appSettings, "1.18.3")
        {
        }

        protected override void Migrate()
        {
            string[] usernames = Settings?.Get<PerUser<string>[]>(UserSettingsVersionKey)?.Select(pu => pu.User).ToArray();
            if (usernames == null)
            {
                return;
            }

            bool isCustomDnsEnabled = Settings.Get<bool>(CustomDnsEnabledKey);
            IpContract[] customDnsIps = Settings.Get<IpContract[]>(CustomDnsIpsKey);

            IList<PerUser<bool>> userIsCustomDnsEnabled = CreateUserSettings(usernames, isCustomDnsEnabled);
            IList<PerUser<IpContract[]>> userCustomDnsIps = CreateUserSettings(usernames, customDnsIps);

            Settings.Set(UserCustomDnsEnabledKey, userIsCustomDnsEnabled);
            Settings.Set(UserCustomDnsIpsKey, userCustomDnsIps);
        }

        private IList<PerUser<T>> CreateUserSettings<T>(IEnumerable<string> usernames, T value)
        {
            IList<PerUser<T>> result = new List<PerUser<T>>();
            foreach (string username in usernames)
            {
                result.Add(new PerUser<T>() { User = username, Value = value });
            }

            return result;
        }
    }
}
