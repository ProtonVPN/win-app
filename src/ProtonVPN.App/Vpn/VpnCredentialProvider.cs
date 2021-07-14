/*
 * Copyright (c) 2020 Proton Technologies AG
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

using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Vpn
{
    public class VpnCredentialProvider
    {
        private readonly Common.Configuration.Config _config;
        private readonly IAppSettings _appSettings;
        private readonly IUserStorage _userStorage;

        public VpnCredentialProvider(Common.Configuration.Config config, IAppSettings appSettings,
            IUserStorage userStorage)
        {
            _config = config;
            _userStorage = userStorage;
            _appSettings = appSettings;
        }

        public VpnCredentials Credentials()
        {
            User user = _userStorage.User();

            return new VpnCredentials(AddSuffixToUsername(user.VpnUsername), user.VpnPassword);
        }

        private string AddSuffixToUsername(string username)
        {
            username += _config.VpnUsernameSuffix;

            if (_appSettings.IsNetShieldEnabled())
            {
                username += $"+f{_appSettings.NetShieldMode}";
            }

            if (_appSettings.IsPortForwardingEnabled())
            {
                username += "+pmp";
            }

            if (_appSettings.FeatureVpnAcceleratorEnabled && !_appSettings.VpnAcceleratorEnabled)
            {
                username += "+nst";
            }

            return username;
        }
    }
}