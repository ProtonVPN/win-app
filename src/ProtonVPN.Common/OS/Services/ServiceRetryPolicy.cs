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
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Timeout;
using ProtonVPN.Common.Abstract;

namespace ProtonVPN.Common.OS.Services
{
    public class ServiceRetryPolicy : IServiceRetryPolicy
    {
        private readonly AsyncPolicy<Result> _policy;

        public ServiceRetryPolicy(int retryCount, TimeSpan retryDelay)
        {
            _policy = Policy(retryCount, retryDelay);
        }

        public async Task<Result> ExecuteAsync(Func<CancellationToken, Task<Result>> action, CancellationToken cancellationToken)
        {
            try
            {
                return await _policy.ExecuteAsync(async ct => await action(ct), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return Result.Fail();
            }
        }

        private AsyncPolicy<Result> Policy(int retryCount, TimeSpan retryDelay)
        {
            var retryPolicy = HandleResult()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(retryCount, retryAttempt => retryDelay);

            return retryPolicy;
        }

        private PolicyBuilder<Result> HandleResult()
        {
            return Policy<Result>.HandleResult(r => r.Failure)
                .Or<OperationCanceledException>()
                .Or<TimeoutException>();
        }
    }
}
