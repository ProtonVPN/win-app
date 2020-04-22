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

using System;
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
            return $"{_appConfig.ApiClientId}_{GetVersion()}";
        }

        public string UserAgent()
        {
            return $"{_appConfig.UserAgent}/{GetVersion()} ({Environment.OSVersion})";
        }

        private string GetVersion()
        {
            return _appSettings.EarlyAccess ? $"{_appConfig.AppVersion}-early" : _appConfig.AppVersion;
        }
    }
}
