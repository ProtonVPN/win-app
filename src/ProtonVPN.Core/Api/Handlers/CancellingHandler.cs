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

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Threading;

namespace ProtonVPN.Core.Api.Handlers
{
    /// <summary>
    /// Cancels all not finished requests on user logout.
    /// </summary>
    public class CancellingHandler : DelegatingHandler
    {
        private readonly CancellationHandle _cancellationHandle = new CancellationHandle();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (IsLogout(request))
            {
                _cancellationHandle.Cancel();
            }

            return await base.SendAsync(request,
                HasAuthorization(request) ? _cancellationHandle.Token : cancellationToken);
        }

        private static bool IsLogout(HttpRequestMessage request)
        {
            return request.Method == HttpMethod.Delete && request.RequestUri.LocalPath == "/auth";
        }

        private static bool HasAuthorization(HttpRequestMessage request)
        {
            return request.Headers.Authorization?.Scheme == "Bearer" && !string.IsNullOrEmpty(request.Headers.Authorization.Parameter);
        }
    }
}
