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

using ProtonVPN.Common;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Settings.Contracts;

namespace ProtonVPN.Settings.SplitTunneling
{
    public class SplitTunnelingIpListViewModel : IpListViewModel
    {
        private readonly IAppSettings _appSettings;

        public SplitTunnelingIpListViewModel(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        protected override bool SupportsIpRanges => true;

        protected override string GetSettingsPropertyName()
        {
          return nameof(IAppSettings.SplitTunnelMode);
        }

        protected override void SaveData(IpContract[] ips)
        {
            if (_appSettings.SplitTunnelMode == SplitTunnelMode.Block)
            {
                _appSettings.SplitTunnelExcludeIps = ips;
            }

            if (_appSettings.SplitTunnelMode == SplitTunnelMode.Permit)
            {
                _appSettings.SplitTunnelIncludeIps = ips;
            }
        }

        protected override IpContract[] GetItems()
        {
            if (_appSettings.SplitTunnelMode == SplitTunnelMode.Block)
            {
                return _appSettings.SplitTunnelExcludeIps;
            }

            return _appSettings.SplitTunnelIncludeIps;
        }
    }
}
