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

using ProtonVPN.Common.Configuration;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Api
{
    public class ApiAppVersion : IApiAppVersion
    {
        private readonly IAppSettings _appSettings;
        private readonly Config _appConfig;

        public ApiAppVersion(IAppSettings appSettings, Config appConfig)
        {
            _appConfig = appConfig;
            _appSettings = appSettings;
        }

        public string Value()
        {
            var version = $"{_appConfig.ApiClientId}_{_appConfig.AppVersion}";
            return _appSettings.EarlyAccess ? $"{version}-early" : version;
        }
    }
}
