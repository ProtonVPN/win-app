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
using System.ServiceModel;
using System.Threading.Tasks;
using ProtonVPN.Common;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Service;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Service.Contract.Settings;
using ProtonVPN.Service.Contract.Vpn;

namespace ProtonVPN.UI.Tests.TestsHelper
{
    public class VPNServiceHelper
    {
        private ServiceChannelFactory _channelFactory = new ServiceChannelFactory();
        private VpnEvents _vpnEvents = new VpnEvents();

        private SettingsContract settings = new SettingsContract()
        {
            KillSwitchMode = Common.KillSwitch.KillSwitchMode.Off,
            SplitTunnel = new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Disabled,
                AppPaths = new string[] { },
                Ips = new string[] { },
            },
            NetShieldMode = 0,
            SplitTcp = true,
            Ipv6LeakProtection = true,
            VpnProtocol = VpnProtocol.Smart,
            OpenVpnAdapter = OpenVpnAdapter.Tun,
        };

        public async Task Disconnect()
        {
            try
            {
                await NewChannel().Proxy.Disconnect(settings, VpnErrorTypeContract.None);
            }
            catch (EndpointNotFoundException)
            {
                //Ignore, because user might be disconnect by UI before
            }
        }

        private ServiceChannel<IVpnConnectionContract> NewChannel()
        {
            ServiceChannel<IVpnConnectionContract> channel = _channelFactory.Create<IVpnConnectionContract>(
                "protonvpn-service/connection",
                _vpnEvents
            );
            RegisterCallback(channel);

            return channel;
        }

        private void RegisterCallback(ServiceChannel<IVpnConnectionContract> channel)
        {
            try
            {
                channel.Proxy.RegisterCallback();
            }
            catch (Exception)
            {
                channel.Dispose();
                throw;
            }
        }
    }
}
