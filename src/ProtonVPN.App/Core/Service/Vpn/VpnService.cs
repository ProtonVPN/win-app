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
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppServiceLogs;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Service.Contract.PortForwarding;
using ProtonVPN.Service.Contract.Settings;
using ProtonVPN.Service.Contract.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnService
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

        public event EventHandler<ServiceSettingsStateContract> ServiceSettingsStateChanged
        {
            add => _vpnEvents.ServiceSettingsStateChanged += value;
            remove => _vpnEvents.ServiceSettingsStateChanged -= value;
        }

        public event EventHandler<PortForwardingStateContract> PortForwardingStateChanged
        {
            add => _vpnEvents.PortForwardingStateChanged += value;
            remove => _vpnEvents.PortForwardingStateChanged -= value;
        }

        public event EventHandler<ConnectionDetailsContract> ConnectionDetailsChanged
        {
            add => _vpnEvents.ConnectionDetailsChanged += value;
            remove => _vpnEvents.ConnectionDetailsChanged -= value;
        }

        public Task Connect(VpnConnectionRequestContract vpnConnectionRequest) =>
            Invoke(p => p.Connect(vpnConnectionRequest).Wrap());

        public Task UpdateAuthCertificate(string certificate) =>
            Invoke(p => p.UpdateAuthCertificate(certificate).Wrap());

        public Task Disconnect(SettingsContract settings, VpnErrorTypeContract vpnError) =>
            Invoke(p => p.Disconnect(settings, vpnError).Wrap());

        public Task RepeatState() =>
            Invoke(p => p.RepeatState().Wrap());

        public Task<InOutBytesContract> Total() =>
            Invoke(p => p.Total());

        private async Task<T> Invoke<T>(Func<IVpnConnectionContract, Task<T>> serviceCall,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            int retryCount = 1;
            while (true)
            {
                try
                {
                    ServiceChannel<IVpnConnectionContract> channel = GetChannel();
                    return await serviceCall(channel.Proxy);
                }
                catch (Exception exception) when (IsCommunicationException(exception))
                {
                    CloseChannel();
                    if (retryCount <= 0)
                    {
                        LogError(exception, memberName, isToRetry: false);
                        throw;
                    }
                    LogError(exception, memberName, isToRetry: true);
                }

                retryCount--;
            }
        }

        private ServiceChannel<IVpnConnectionContract> GetChannel()
        {
            return _channel ??= NewChannel();
        }

        private ServiceChannel<IVpnConnectionContract> NewChannel()
        {
            ServiceChannel<IVpnConnectionContract> channel = _channelFactory.Create<IVpnConnectionContract>(
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

        public void UnRegisterCallback()
        {
            try
            {
                _channel?.Proxy.UnRegisterCallback();
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

        private void LogError(Exception exception, string callerMemberName, bool isToRetry)
        {
            _logger.Error<AppServiceCommunicationFailedLog>(
                $"The invocation of '{callerMemberName}' on VPN Service channel returned an exception and will " +
                (isToRetry ? string.Empty : "not ") +
                $"be retried. Exception message: {exception.Message}");
        }
    }
}
