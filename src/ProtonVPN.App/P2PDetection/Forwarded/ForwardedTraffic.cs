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

using ProtonVPN.Config;
using ProtonVPN.Core.User;
using System.Linq;
using System.Threading.Tasks;

namespace ProtonVPN.P2PDetection.Forwarded
{
    /// <summary>
    ///     Detects forwarded traffic on not free servers.
    /// </summary>
    /// <remarks>
    ///     If P2P traffic is detected on not free VPN server, on which internet provider does not support P2P,
    ///     all traffic is forwarded to another exit server, where internet provider supports P2P.
    ///     Checking is done by comparing current user ip address to the predefined black hole exit ips.
    /// </remarks>
    internal class ForwardedTraffic : IForwardedTraffic
    {
        private readonly IVpnConfig _config;
        private readonly IUserLocationService _userLocationService;

        public ForwardedTraffic(IUserLocationService userLocationService, IVpnConfig config)
        {
            _config = config;
            _userLocationService = userLocationService;
        }

        public async Task<ForwardedTrafficResult> Value()
        {
            var response = await _userLocationService.LocationAsync();
            return response.Failure
                ? new ForwardedTrafficResult(false, false, string.Empty)
                : new ForwardedTrafficResult(true, _config.BlackHoleIps().Contains(response.Value.Ip),
                    response.Value.Ip);
        }
    }
}