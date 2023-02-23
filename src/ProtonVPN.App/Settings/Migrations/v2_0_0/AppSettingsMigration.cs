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
using System.Runtime.InteropServices;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Storage;

namespace ProtonVPN.Settings.Migrations.v2_0_0
{
    internal class AppSettingsMigration : BaseAppSettingsMigration
    {
        private readonly InitialAppSettingsMigration _initialAppSettingsMigration;

        public AppSettingsMigration(ISettingsStorage appSettings, InitialAppSettingsMigration initialAppSettingsMigration): base(appSettings, "2.0.0")
        {
            _initialAppSettingsMigration = initialAppSettingsMigration;
        }

        protected override void Migrate()
        {
            if (!_initialAppSettingsMigration.IsCleanInstall)
            {
                ClearWindowsIconCache();
                Settings.Set(nameof(IAppSettings.IsToShowRebrandingPopup), true);
            }
        }

        private void ClearWindowsIconCache()
        {
            // https://docs.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shchangenotify
            SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("shell32.dll")]
        static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}