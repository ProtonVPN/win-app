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

using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Users
{
    // The following route is used to retrieve VPN server information,
    // including scores for the best server to connect to depending on
    // a user's proximity to a server and its load. To provide relevant
    // scores even when connected to VPN, we send a truncated version
    // of the user's public IP address. In keeping with our no-logs policy,
    // this partial IP address is not stored on the server and is only used
    // to fulfill this one-off API request.
    public class TruncatedLocation
    {
        private readonly IUserStorage _userStorage;

        public TruncatedLocation(IUserStorage userStorage)
        {
            _userStorage = userStorage;
        }

        public string Ip()
        {
            string ip = _userStorage.GetLocation().Ip;
            if (string.IsNullOrEmpty(ip))
            {
                return string.Empty;
            }

            string[] parts = ip.Split('.');
            if (parts.Length >= 3)
            {
                return string.Join(".", parts[0], parts[1], parts[2], 0);
            }

            return string.Empty;
        }
    }
}
