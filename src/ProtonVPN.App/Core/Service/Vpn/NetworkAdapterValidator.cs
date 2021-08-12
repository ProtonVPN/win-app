/*
 * Copyright (c) 2021 Proton Technologies AG
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

using ProtonVPN.Common.Networking;
using ProtonVPN.Common.OS.Net;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Core.Settings;
using Sentry;
using Sentry.Protocol;

namespace ProtonVPN.Core.Service.Vpn
{
    public class NetworkAdapterValidator : INetworkAdapterValidator
    {
        private readonly INetworkInterfaceLoader _networkInterfaceLoader;
        private readonly IAppSettings _appSettings;

        public NetworkAdapterValidator(INetworkInterfaceLoader networkInterfaceLoader, IAppSettings appSettings)
        {
            _networkInterfaceLoader = networkInterfaceLoader;
            _appSettings = appSettings;
        }

        public bool IsAdapterAvailable()
        {
            INetworkInterface openVpnTunInterface = _networkInterfaceLoader.GetOpenVpnTunInterface();
            INetworkInterface openVpnTapInterface = _networkInterfaceLoader.GetOpenVpnTapInterface();
            if (openVpnTunInterface == null && openVpnTapInterface == null)
            {
                return false;
            }

            if (openVpnTunInterface == null && _appSettings.NetworkAdapterType == OpenVpnAdapter.Tun)
            {
                _appSettings.NetworkAdapterType = OpenVpnAdapter.Tap;
                SendTunFallbackEvent();
            }

            return true;
        }

        private void SendTunFallbackEvent()
        {
            SentrySdk.CaptureEvent(new SentryEvent
            {
                Message = "TUN adapter not found. Adapter changed to TAP.",
                Level = SentryLevel.Info,
            });
        }
    }
}