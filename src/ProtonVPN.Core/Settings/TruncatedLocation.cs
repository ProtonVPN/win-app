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

namespace ProtonVPN.Core.Settings
{
    public class TruncatedLocation
    {
        private readonly string _ip;

        public TruncatedLocation(string ip)
        {
            _ip = ip;
        }

        public string Value()
        {
            if (string.IsNullOrEmpty(_ip))
            {
                return string.Empty;
            }

            var parts = _ip.Split('.');
            if (parts.Length >= 3)
            {
                return string.Join(".", parts[0], parts[1], parts[2], 0);
            }

            return string.Empty;
        }
    }
}
