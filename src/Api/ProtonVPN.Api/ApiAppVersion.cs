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
using System.Text;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Api;

public class ApiAppVersion : IApiAppVersion
{
    private const string DEVELOPMENT_SUFFIX = "-dev";

    private readonly IConfiguration _config;
    private readonly Lazy<string> _appVersion;
    private readonly Lazy<string> _userAgent;

    public string AppVersion => _appVersion.Value;
    public string UserAgent => _userAgent.Value;

    public ApiAppVersion(IConfiguration appConfig)
    {
        _config = appConfig;

        _appVersion = new(CalculateAppVersion);
        _userAgent = new(CalculateUserAgent);
    }

    public string CalculateAppVersion()
    {
        StringBuilder sb = new();
        sb.Append($"{_config.ApiClientId}@{GetVersion()}");
#if DEBUG
        sb.Append(DEVELOPMENT_SUFFIX);
#endif
        sb.Append($"+{OSArchitecture.StringValue}");
        return sb.ToString();
    }

    public string CalculateUserAgent()
    {
        return $"{_config.UserAgent}/{GetVersion()} ({Environment.OSVersion}; {OSArchitecture.StringValue})";
    }

    private string GetVersion()
    {
        return _config.ClientVersion;
    }
}