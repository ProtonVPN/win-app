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

using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Service.Settings;

namespace ProtonVPN.Service.Network
{
    internal class CurrentNetworkAdapter : ICurrentNetworkAdapter
    {
        private readonly IServiceSettings _serviceSettings;
        private readonly INetworkInterfaces _networkInterfaces;
        private readonly Common.Configuration.Config _config;

        public CurrentNetworkAdapter(
            Common.Configuration.Config config,
            IServiceSettings serviceSettings,
            INetworkInterfaces networkInterfaces)
        {
            _config = config;
            _networkInterfaces = networkInterfaces;
            _serviceSettings = serviceSettings;
        }

        public uint Index
        {
            get
            {
                if (_serviceSettings.UseTunAdapter)
                {
                    return _networkInterfaces.InterfaceIndex(_config.OpenVpn.TunAdapterDescription,
                        _config.OpenVpn.TunAdapterId);
                }

                return _networkInterfaces.InterfaceIndex(_config.OpenVpn.TapAdapterDescription,
                    _config.OpenVpn.TapAdapterId);
            }
        }

        public string HardwareId =>
            _serviceSettings.UseTunAdapter ? _config.OpenVpn.TunAdapterId : _config.OpenVpn.TapAdapterId;
    }
}