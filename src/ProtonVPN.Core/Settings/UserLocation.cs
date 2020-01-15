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
    public class UserLocation
    {
        public static UserLocation Empty { get; } = new UserLocation("", 0, 0, "", "");

        public string Ip { get; }

        public float Latitude { get; }

        public float Longitude { get; }

        public string Isp { get; }

        public string Country { get; }

        public UserLocation(string ip, float latitude, float longitude, string isp, string country)
        {
            Ip = ip ?? "";
            Latitude = latitude;
            Longitude = longitude;
            Isp = isp ?? "";
            Country = country ?? "";
        }
    }
}
