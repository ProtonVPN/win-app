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
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals;

namespace ProtonVPN.Core.Service.Vpn
{
    public class ServiceStartDecorator : IVpnServiceManager
    {
        private readonly ILogger _logger;
        private readonly IVpnServiceManager _decorated;
        private readonly IModals _modals;

        public ServiceStartDecorator(ILogger logger, IVpnServiceManager decorated, IModals modals)
        {
            _logger = logger;
            _modals = modals;
            _decorated = decorated;
        }

        public async Task Connect(VpnConnectionRequest connectionRequest)
        {
            await InvokeAction(() => _decorated.Connect(connectionRequest));
        }

        public async Task UpdateServers(IReadOnlyList<VpnHost> servers, VpnConfig config)
        {
            await InvokeAction(() => _decorated.UpdateServers(servers, config));
        }

        public async Task Disconnect(VpnError error)
        {
            await InvokeAction(() => _decorated.Disconnect(error));
        }

        public async Task<InOutBytes> Total()
        {
            return await _decorated.Total();
        }

        public async Task RepeatState()
        {
            await _decorated.RepeatState();
        }

        public void RegisterCallback(Action<VpnStateChangedEventArgs> onVpnStateChanged)
        {
            _decorated.RegisterCallback(onVpnStateChanged);
        }

        private async Task InvokeAction(Func<Task> action)
        {
            while (true)
            {
                try
                {
                    await action();

                    break;
                }
                catch (EndpointNotFoundException)
                {
                    var result = _modals.Show<ServiceStartModalViewModel>();
                    if (result == null || !result.Value)
                    {
                        break;
                    }
                }
                catch (Exception e) when (IsConnectionException(e))
                {
                    _logger.Error(e);
                }
            }
        }

        private bool IsConnectionException(Exception ex) =>
            ex is CommunicationException ||
            ex is TimeoutException;
    }
}
