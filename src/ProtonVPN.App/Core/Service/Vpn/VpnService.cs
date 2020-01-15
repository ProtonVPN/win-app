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

using System;
using System.ServiceModel;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Service.Contract.Settings;
using ProtonVPN.Service.Contract.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnService : ILogoutAware
    {
        private readonly ServiceChannelFactory _channelFactory;
        private readonly ILogger _logger;
        private ServiceChannel<IVpnConnectionContract> _channel;
        private readonly VpnEvents _vpnEvents;

        public VpnService(
            ServiceChannelFactory channelFactory,
            VpnEvents vpnEvents,
            ILogger logger)
        {
            _channelFactory = channelFactory;
            _logger = logger;
            _vpnEvents = vpnEvents;
        }

        public event EventHandler<VpnStateContract> VpnStateChanged
        {
            add => _vpnEvents.VpnStateChanged += value;
            remove => _vpnEvents.VpnStateChanged -= value;
        }

        public Task Connect(VpnConnectionRequestContract vpnConnectionRequest) =>
            Invoke(p => p.Connect(vpnConnectionRequest).Wrap());

        public Task UpdateServers(VpnHostContract[] servers, VpnConfigContract config) =>
            Invoke(p => p.UpdateServers(servers, config).Wrap());

        public Task Disconnect(SettingsContract settings, VpnErrorTypeContract vpnError) =>
            Invoke(p => p.Disconnect(settings, vpnError).Wrap());

        public Task<VpnStateContract> State() =>
            Invoke(p => p.State());

        public Task<InOutBytesContract> Total() =>
            Invoke(p => p.Total());

        public void OnUserLoggedOut()
        {
            UnRegisterCallback(_channel);
            CloseChannel();
        }

        private async Task<T> Invoke<T>(Func<IVpnConnectionContract, Task<T>> serviceCall)
        {
            var retryCount = 1;
            while (true)
            {
                try
                {
                    var channel = GetChannel();
                    return await serviceCall(channel.Proxy);
                }
                catch (Exception ex) when (IsCommunicationException(ex))
                {
                    CloseChannel();
                    if (retryCount <= 0)
                    {
                        throw;
                    }
                    _logger.Error("Retrying Invoke due to: " + ex.Message);
                }

                retryCount--;
            }
        }

        private ServiceChannel<IVpnConnectionContract> GetChannel()
        {
            return _channel ?? (_channel = NewChannel());
        }

        private ServiceChannel<IVpnConnectionContract> NewChannel()
        {
            var channel = _channelFactory.Create<IVpnConnectionContract>(
                "protonvpn-service/connection",
                _vpnEvents);

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

        private void UnRegisterCallback(ServiceChannel<IVpnConnectionContract> channel)
        {
            try
            {
                channel?.Proxy.UnRegisterCallback();
            }
            catch (Exception e) when (IsCommunicationException(e))
            {
            }
        }

        private void CloseChannel()
        {
            _channel?.Dispose();
            _channel = null;
        }

        private static bool IsCommunicationException(Exception ex) =>
            ex is CommunicationException ||
            ex is TimeoutException ||
            ex is ObjectDisposedException ode && ode.ObjectName == "System.ServiceModel.Channels.ClientFramingDuplexSessionChannel";
    }
}
