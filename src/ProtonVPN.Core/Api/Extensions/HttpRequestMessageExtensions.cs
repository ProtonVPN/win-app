/*
 * Copyright (c) 2022 Proton Technologies AG
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

using System.Linq;
using System.Net.Http;

namespace ProtonVPN.Core.Api.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static bool AuthHeadersInvalid(this HttpRequestMessage request)
        {
            return request.AuthHeaderSet() && !request.UserIdHeaderSet();
        }

        private static bool AuthHeaderSet(this HttpRequestMessage request)
        {
            return !string.IsNullOrEmpty(request.Headers.Authorization?.Parameter);
        }

        private static bool UserIdHeaderSet(this HttpRequestMessage request)
        {
            var result = request.Headers.TryGetValues("x-pm-uid", out var value);
            if (!result)
            {
                return false;
            }

            return !string.IsNullOrEmpty(value.FirstOrDefault());
        }
    }
}
