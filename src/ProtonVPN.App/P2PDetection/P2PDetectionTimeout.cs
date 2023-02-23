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
using ProtonVPN.Common.Configuration;

namespace ProtonVPN.P2PDetection
{
    /// <summary>
    /// Calculates P2P detection timeout value from P2P detection interval.
    /// </summary>
    public class P2PDetectionTimeout : IP2PDetectionTimeout
    {
        private readonly TimeSpan _interval;

        public P2PDetectionTimeout(IConfiguration config)
        {
            _interval = config.P2PCheckInterval;
        }

        /// <summary>
        /// P2P detection timeout value.
        /// P2P detection interval should fit two detection actions.
        /// </summary>
        public TimeSpan GetTimeoutValue()
        {
            return new TimeSpan(_interval.Ticks / 2);
        }
    }
}