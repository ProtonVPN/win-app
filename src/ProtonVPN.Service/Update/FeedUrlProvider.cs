/*
 * Copyright (c) 2024 Proton AG
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
using ProtonVPN.Builds.Variables;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Update.Contracts;
using ProtonVPN.Update.Contracts.Config;

namespace ProtonVPN.Service.Update;

public class FeedUrlProvider : IFeedUrlProvider
{
    private readonly IConfiguration _config;
    private FeedType _feedType = FeedType.Public;

    public FeedUrlProvider(IConfiguration config)
    {
        _config = config;
    }

    public Uri GetFeedUrl()
    {
        string url = _feedType is FeedType.Internal
            ? GlobalConfig.InternalReleaseUpdateUrl
            : _config.Urls.UpdateUrl;
        string architecture = GetArchitecture();

        return new Uri(string.Format(url, architecture));
    }

    public void SetFeedType(FeedType feedType)
    {
        _feedType = feedType;
    }

    private string GetArchitecture()
    {
        Architecture osArchitecture = OSArchitecture.Value;

        return osArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new NotSupportedException($"Unsupported OS architecture: {osArchitecture}")
        };
    }
}