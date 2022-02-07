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

using System;
using System.Net.Http;
using Polly;

namespace ProtonVPN.Core.Api
{
    public class SleepDurationProvider
    {
        private readonly DelegateResult<HttpResponseMessage> _response;
        private const int MaxTimeout = 10;

        public SleepDurationProvider(DelegateResult<HttpResponseMessage> response)
        {
            _response = response;
        }

        public TimeSpan Value()
        {
            var retryAfter = _response?.Result?.Headers?.RetryAfter;
            if (retryAfter?.Delta != null && retryAfter.Delta.Value.Seconds > 0)
            {
                return TimeSpan.FromSeconds(Math.Min(retryAfter.Delta.Value.Seconds, MaxTimeout));
            }

            return TimeSpan.FromSeconds(MaxTimeout);
        }
    }
}
