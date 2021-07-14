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

using ProtonVPN.Common.OS.Net;
using ProtonVPN.Service.Settings;

namespace ProtonVPN.Service.Network
{
    public class CurrentNetworkInterface : ICurrentNetworkInterface
    {
        private readonly Common.Configuration.Config _config;
        private readonly IServiceSettings _serviceSettings;
        private readonly INetworkInterfaceLoader _networkInterfaceLoader;

        public CurrentNetworkInterface(
            Common.Configuration.Config config,
            IServiceSettings serviceSettings,
            INetworkInterfaceLoader networkInterfaceLoader)
        {
            _config = config;
            _serviceSettings = serviceSettings;
            _networkInterfaceLoader = networkInterfaceLoader;
        }

        public uint Index
        {
            get
            {
                if (_serviceSettings.UseTunAdapter)
                {
                    return _networkInterfaceLoader.GetTunInterface()?.Index ?? 0;
                }

                return _networkInterfaceLoader.GetTapInterface()?.Index ?? 0;
            }
        }

        public string HardwareId =>
            _serviceSettings.UseTunAdapter ? _config.OpenVpn.TunAdapterId : _config.OpenVpn.TapAdapterId;
    }
}