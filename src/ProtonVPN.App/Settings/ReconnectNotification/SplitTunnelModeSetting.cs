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
using ProtonVPN.Common;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Settings.ReconnectNotification
{
    public class SplitTunnelModeSetting : CompoundSetting
    {
        private readonly IAppSettings _appSettings;

        private readonly List<Setting> _includeModeSettings = new();
        private readonly List<Setting> _excludeModeSettings = new();

        public SplitTunnelModeSetting(string name, Setting parent, IAppSettings appSettings) : base(name, parent, appSettings)
        {
            _appSettings = appSettings;
            _includeModeSettings.Add(new SingleSetting(nameof(IAppSettings.SplitTunnelIncludeIps), this, _appSettings));
            _includeModeSettings.Add(new SingleSetting(nameof(IAppSettings.SplitTunnelingAllowApps), this, _appSettings));

            _excludeModeSettings.Add(new SingleSetting(nameof(IAppSettings.SplitTunnelExcludeIps), this, _appSettings));
            _excludeModeSettings.Add(new SingleSetting(nameof(IAppSettings.SplitTunnelingBlockApps), this, _appSettings));
        }

        public override void Add(Setting s) => throw new NotImplementedException();

        public override List<Setting> GetChildren()
        {
            if (_appSettings.SplitTunnelMode == SplitTunnelMode.Permit)
            {
                return _includeModeSettings;
            }

            return _excludeModeSettings;
        }
    }
}