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
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Settings.ReconnectNotification
{
    public class SettingsBuilder
    {
        private List<Setting> _settings;
        private readonly IAppSettings _appSettings;

        public SettingsBuilder(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public List<Setting> Build()
        {
            _settings = new List<Setting>
            {
                new SingleSetting(nameof(IAppSettings.OvpnProtocol), null, _appSettings),
                new OpenVpnDriverSetting(_appSettings),
                new CustomDnsSetting(nameof(IAppSettings.CustomDnsEnabled), null, _appSettings),
            };

            CompoundSetting st = new(nameof(IAppSettings.SplitTunnelingEnabled), null, _appSettings);
            st.Add(new SplitTunnelModeSetting(nameof(IAppSettings.SplitTunnelMode), st, _appSettings));

            _settings.Add(st);

            return _settings;
        }
    }
}