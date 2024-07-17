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

using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace ProtonVPN.Api.Extensions;

public static class HttpResponseMessageExtensions
{
    private static readonly List<HttpStatusCode> _skipRetryStatusCodes =
    [
        HttpStatusCode.BadRequest,
        HttpStatusCode.Unauthorized,
        HttpStatusCode.Forbidden,
        HttpStatusCode.NotFound,
        HttpStatusCode.Conflict,
        HttpStatusCode.UnprocessableEntity,
        HttpStatusCode.NotModified
    ];

    public static bool IsToRetry(this HttpResponseMessage message)
    {
        double retryAfterSeconds = message.RetryAfterInSeconds();
        if (retryAfterSeconds > 0 &&
            message.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.TooManyRequests)
        {
            return true;
        }
        if (message.StatusCode is HttpStatusCode.TooManyRequests && retryAfterSeconds == 0)
        {
            return false;
        }
        if (message.StatusCode is HttpStatusCode.ServiceUnavailable)
        {
            return false;
        }

        return !message.IsSuccessStatusCode && !_skipRetryStatusCodes.Contains(message.StatusCode);
    }

    public static bool IsToRetryOnce(this HttpResponseMessage message)
    {
        return message.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.BadGateway;
    }

    public static double RetryAfterInSeconds(this HttpResponseMessage message)
    {
        return message.Headers.RetryAfter?.Delta?.TotalSeconds ?? 0;
    }
}