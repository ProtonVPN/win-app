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

using System.Net.Http;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Net.Http;

namespace ProtonVPN.Api.Handlers.Retries
{
    public class RetryCountProvider : IRetryCountProvider
    {
        private readonly IConfiguration _config;

        public RetryCountProvider(IConfiguration config)
        {
            _config = config;
        }

        public int GetRetryCount(HttpRequestMessage request)
        {
            int? customRetryCount = request.GetRetryCount();
            return customRetryCount is > 0 ? customRetryCount.Value : _config.ApiRetries;
        }
    }
}