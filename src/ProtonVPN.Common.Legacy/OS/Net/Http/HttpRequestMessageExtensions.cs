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
using System.Linq;
using System.Net.Http;

namespace ProtonVPN.Common.Legacy.OS.Net.Http;

public static class HttpRequestMessageExtensions
{
    private static readonly HttpRequestOptionsKey<TimeSpan> CustomTimeoutOptionsKey = new("CustomTimeout");
    private static readonly HttpRequestOptionsKey<int> RetryCountOptionsKey = new("RetryCount");

    public static bool AuthHeadersInvalid(this HttpRequestMessage request)
    {
        return request.AuthHeaderSet() && !request.UserIdHeaderSet();
    }

    public static TimeSpan? GetCustomTimeout(this HttpRequestMessage request)
    {
        return request.Options.TryGetValue(CustomTimeoutOptionsKey, out TimeSpan timeout) ? timeout : null;
    }

    public static void SetCustomTimeout(this HttpRequestMessage request, TimeSpan timeout)
    {
        if (!request.Options.TryGetValue(CustomTimeoutOptionsKey, out _))
        {
            request.Options.Set(CustomTimeoutOptionsKey, timeout);
        }
    }

    public static void SetRetryCount(this HttpRequestMessage request, int retryCount)
    {
        if (!request.Options.TryGetValue(RetryCountOptionsKey, out _))
        {
            request.Options.Set(RetryCountOptionsKey, retryCount);
        }
    }

    public static int? GetRetryCount(this HttpRequestMessage request)
    {
        return request.Options.TryGetValue(RetryCountOptionsKey, out int timeout) ? timeout : null;
    }

    private static bool AuthHeaderSet(this HttpRequestMessage request)
    {
        return !string.IsNullOrEmpty(request.Headers.Authorization?.Parameter);
    }

    private static bool UserIdHeaderSet(this HttpRequestMessage request)
    {
        bool result = request.Headers.TryGetValues("x-pm-uid", out IEnumerable<string> value);
        if (!result)
        {
            return false;
        }

        return !string.IsNullOrEmpty(value.FirstOrDefault());
    }
}