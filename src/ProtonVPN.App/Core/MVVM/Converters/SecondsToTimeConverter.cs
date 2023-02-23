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
using System.Globalization;
using System.Windows.Data;

namespace ProtonVPN.Core.MVVM.Converters
{
    class SecondsToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "SecondsToTimeConverter requires seconds to be passed in.");
            }

            var seconds = (int )value;
            var ts = TimeSpan.FromSeconds(seconds);

            if (seconds < 60)
            {
                return $"{ts:%s}s";
            }

            if (seconds < 3600)
            {
                return $"{ts:%m}m {ts:%s}s";
            }

            if (seconds < 86400)
            {
                return $"{ts:%h}h {ts:%m}m {ts:%s}s";
            }

            return $"{ts:%d}d {ts:%h}h {ts:%m}m {ts:%s}s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
