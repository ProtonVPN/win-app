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

using Polly;
using ProtonVPN.Common.Abstract;
using System;

namespace ProtonVPN.Common.OS.Services
{
    public class ServiceRetryPolicy
    {
        private readonly int _retryCount;
        private readonly TimeSpan _retryDelay;

        public ServiceRetryPolicy(int retryCount, TimeSpan retryDelay)
        {
            _retryDelay = retryDelay;
            _retryCount = retryCount;
        }

        public Policy<Result> Value()
        {
            var policy = HandleResult()
                .WaitAndRetry(_retryCount, retryAttempt => _retryDelay);

            return policy;
        }

        private PolicyBuilder<Result> HandleResult()
        {
            return Policy<Result>.HandleResult(r => r.Failure);
        }
    }
}
