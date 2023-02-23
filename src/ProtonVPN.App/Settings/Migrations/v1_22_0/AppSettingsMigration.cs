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

using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Storage;

namespace ProtonVPN.Settings.Migrations.v1_22_0
{
    internal class AppSettingsMigration : BaseAppSettingsMigration
    {
        private const string UseTunAdapterKey = "UseTunAdapter";
        private const string NetworkAdapterTypeKey = "NetworkAdapterType";

        public AppSettingsMigration(ISettingsStorage appSettings) :
            base(appSettings, "1.22.0")
        {
        }

        protected override void Migrate()
        {
            bool isTunAdapterEnabled = Settings.Get<bool>(UseTunAdapterKey);
            Settings.Set(NetworkAdapterTypeKey, isTunAdapterEnabled ? OpenVpnAdapter.Tun : OpenVpnAdapter.Tap);
        }
    }
}