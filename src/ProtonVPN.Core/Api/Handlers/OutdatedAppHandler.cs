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

using Newtonsoft.Json;
using ProtonVPN.Core.Api.Contracts;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ProtonVPN.Core.Api.Handlers
{
    /// <summary>
    /// Detects outdated app by checking response code.
    /// </summary>
    public class OutdatedAppHandler : DelegatingHandler
    {
        public event EventHandler<BaseResponse> ForcedLogout;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
                return response;

            try
            {
                var result = JsonConvert.DeserializeObject<BaseResponse>(content);
                if (ForceLogoutRequired(result.Code))
                {
                    ForcedLogout?.Invoke(this, result);
                }
            }
            catch (JsonException)
            {
            }

            return response;
        }

        private bool ForceLogoutRequired(int code)
        {
            return code == ResponseCodes.OutdatedApiResponse ||
                   code == ResponseCodes.OutdatedAppResponse;
        }
    }
}
