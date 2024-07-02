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
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Api;

public class ApiHostProvider : IApiHostProvider
{
    private readonly ISettings _settings;
    private readonly IConfiguration _config;
    private readonly IConnectionManager _connectionManager;

    public ApiHostProvider(ISettings settings, IConfiguration config, IConnectionManager connectionManager)
    {
        _settings = settings;
        _config = config;
        _connectionManager = connectionManager;
    }

    public Uri GetBaseUri()
    {
        string host = IsProxyActive()
            ? _settings.ActiveAlternativeApiBaseUrl
            : new Uri(_config.Urls.ApiUrl).Host;

        return new Uri($"https://{host}");
    }

    public bool IsProxyActive()
    {
        return _settings.IsAlternativeRoutingEnabled &&
               _connectionManager.IsDisconnected &&
               !string.IsNullOrEmpty(_settings.ActiveAlternativeApiBaseUrl);
    }
}