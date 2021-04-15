/*
 * Copyright (c) 2021 Proton Technologies AG
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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Api.Extensions;

namespace ProtonVPN.Core.Api.Handlers
{
    /// <summary>
    /// Detects outdated app by checking response code.
    /// </summary>
    public class OutdatedAppHandler : DelegatingHandler
    {
        public event EventHandler<BaseResponse> AppOutdated;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            BaseResponse baseResponse = await response.GetBaseResponseMessage();
            if (baseResponse == null)
            {
                return response;
            }

            if (ForceLogoutRequired(baseResponse.Code))
            {
                AppOutdated?.Invoke(this, baseResponse);
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