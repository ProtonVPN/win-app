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

namespace ProtonVPN.Common.OS.Net.Http
{
    public static class HttpRequestMessageExtensions
    {
        private const string CUSTOM_TIMEOUT_PROPERTY_NAME = "CustomTimeout";
        private const string RETRY_COUNT_PROPERTY_NAME = "RetryCount";

        public static bool AuthHeadersInvalid(this HttpRequestMessage request)
        {
            return request.AuthHeaderSet() && !request.UserIdHeaderSet();
        }

        public static TimeSpan? GetCustomTimeout(this HttpRequestMessage request)
        {
            if (request.Properties.TryGetValue(CUSTOM_TIMEOUT_PROPERTY_NAME, out object timeout))
            {
                return (TimeSpan)timeout;
            }

            return null;
        }

        public static void SetCustomTimeout(this HttpRequestMessage request, TimeSpan timeout)
        {
            if (!request.Properties.ContainsKey(CUSTOM_TIMEOUT_PROPERTY_NAME))
            {
                request.Properties.Add(CUSTOM_TIMEOUT_PROPERTY_NAME, timeout);
            }
        }

        public static void SetRetryCount(this HttpRequestMessage request, int retryCount)
        {
            if (!request.Properties.ContainsKey(RETRY_COUNT_PROPERTY_NAME))
            {
                request.Properties.Add(RETRY_COUNT_PROPERTY_NAME, retryCount);
            }
        }

        public static int? GetRetryCount(this HttpRequestMessage request)
        {
            if (request.Properties.TryGetValue(RETRY_COUNT_PROPERTY_NAME, out object timeout))
            {
                return (int)timeout;
            }

            return null;
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
}