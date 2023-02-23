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
using ProtonVPN.Common.Helpers;

namespace ProtonVPN.Common.Extensions
{
    public static class TimeSpanExtensions
    {
        private static readonly Random Random = new();

        public static TimeSpan RandomizedWithDeviation(this TimeSpan value, double deviation)
        {
            Ensure.IsTrue(value > TimeSpan.Zero, $"{nameof(value)} must be positive");
            Ensure.IsTrue(deviation is >= 0 and < 1, $"{nameof(deviation)} must be between zero and one");

            return value + TimeSpan.FromMilliseconds(value.TotalMilliseconds * deviation * (2.0 * Random.NextDouble() - 1.0));
        }
    }
}