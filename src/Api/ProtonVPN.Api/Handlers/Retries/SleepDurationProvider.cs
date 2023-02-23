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
using System.Net.Http;
using Polly;
using ProtonVPN.Api.Extensions;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Api.Handlers.Retries
{
    public class SleepDurationProvider
    {
        private const int MAX_RETRY_DELAY_IN_SECONDS = 128;

        public TimeSpan ResponseMessageDurationFunction(int retryCount, DelegateResult<HttpResponseMessage> response,
            Context context)
        {
            double retryAfterSeconds = response.Result.RetryAfterInSeconds();
            TimeSpan retryInterval = retryAfterSeconds > 0
                ? TimeSpan.FromSeconds(retryAfterSeconds)
                : RequestExceptionDurationFunction(retryCount);

            return retryInterval;
        }

        public TimeSpan RequestExceptionDurationFunction(int retryCount)
        {
            return TimeSpan.FromSeconds(Math.Min(MAX_RETRY_DELAY_IN_SECONDS, Math.Pow(2, retryCount - 1)))
                .RandomizedWithDeviation(0.2);
        }
    }
}