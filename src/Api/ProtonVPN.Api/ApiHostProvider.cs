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
using ProtonVPN.Api.Contracts;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Api
{
    public class ApiHostProvider : IApiHostProvider
    {
        private const int MAX_HOURS_WITH_PROXY = 24;

        private readonly string _apiHost;
        private readonly IAppSettings _appSettings;
        private bool _isDisconnected = true;

        public ApiHostProvider(IAppSettings appSettings, IConfiguration config, IVpnStatusNotifier vpnStatusNotifier)
        {
            _appSettings = appSettings;
            _apiHost = new Uri(config.Urls.ApiUrl).Host;

            vpnStatusNotifier.VpnStatusChanged += OnVpnStatusChanged;
        }

        private void OnVpnStatusChanged(object sender, VpnStatus vpnStatus)
        {
            _isDisconnected = vpnStatus == VpnStatus.Disconnected;
        }

        public string GetHost()
        {
            return IsProxyActive() ? _appSettings.ActiveAlternativeApiBaseUrl : _apiHost;
        }

        public bool IsProxyActive()
        {
            return _appSettings.DoHEnabled &&
                   _isDisconnected &&
                   DateTime.UtcNow.Subtract(_appSettings.LastPrimaryApiFailDateUtc).TotalHours < MAX_HOURS_WITH_PROXY &&
                   !string.IsNullOrEmpty(_appSettings.ActiveAlternativeApiBaseUrl);
        }
    }
}