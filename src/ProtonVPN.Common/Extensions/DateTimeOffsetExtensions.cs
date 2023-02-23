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
using Newtonsoft.Json;

namespace ProtonVPN.Common.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset? FromJsonDateTimeOffset(this string jsonDateTime)
        {
            DateTimeOffset? dateTime;
            try
            {
                dateTime = jsonDateTime.IsNullOrEmpty()
                    ? null
                    : JsonConvert.DeserializeObject<DateTimeOffset>(jsonDateTime);
            }
            catch
            {
                dateTime = null;
            }
            return dateTime;
        }

        public static string ToJsonDateTimeOffset(this DateTimeOffset? date)
        {
            return date.HasValue ? JsonConvert.SerializeObject(date) : null;
        }

        public static DateTimeOffset TruncateToSeconds(this DateTimeOffset date)
        {
            return new(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Offset);
        }
    }
}
