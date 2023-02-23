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
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace ProtonVPN.Api.Handlers.Retries
{
    public class RetryingHandler : RetryingHandlerBase
    {
        private readonly IRetryPolicyProvider _retryPolicyProvider;

        public RetryingHandler(IRetryPolicyProvider retryPolicyProvider)
        {
            _retryPolicyProvider = retryPolicyProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            AsyncPolicy<HttpResponseMessage> retryPolicy = _retryPolicyProvider.GetRetryPolicy(request);
            return await retryPolicy.ExecuteAsync(async ct => await base.SendAsync(request, ct), cancellationToken);
        }
    }
}