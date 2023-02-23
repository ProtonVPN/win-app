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

using ProtonVPN.Service.Contract.Vpn;
using System;
using System.ServiceModel;
using System.Windows;
using ProtonVPN.Service.Contract.PortForwarding;
using ProtonVPN.Service.Contract.Settings;

namespace ProtonVPN.Core.Service.Vpn
{
    [CallbackBehavior(
        ConcurrencyMode = ConcurrencyMode.Single,
        UseSynchronizationContext = false)]
    public class VpnEvents : IVpnEventsContract
    {
        public event EventHandler<VpnStateContract> VpnStateChanged;
        public event EventHandler<ServiceSettingsStateContract> ServiceSettingsStateChanged;
        public event EventHandler<PortForwardingStateContract> PortForwardingStateChanged;
        public event EventHandler<ConnectionDetailsContract> ConnectionDetailsChanged;

        public void OnStateChanged(VpnStateContract e)
        {
            Action action = () => VpnStateChanged?.Invoke(this, e);
            Application.Current?.Dispatcher?.BeginInvoke(action, null);
        }

        public void OnServiceSettingsStateChanged(ServiceSettingsStateContract e)
        {
            Action action = () => ServiceSettingsStateChanged?.Invoke(this, e);
            Application.Current?.Dispatcher?.BeginInvoke(action, null);
        }

        public void OnPortForwardingStateChanged(PortForwardingStateContract e)
        {
            Action action = () => PortForwardingStateChanged?.Invoke(this, e);
            Application.Current?.Dispatcher?.BeginInvoke(action, null);
        }

        public void OnConnectionDetailsChanged(ConnectionDetailsContract e)
        {
            Action action = () => ConnectionDetailsChanged?.Invoke(this, e);
            Application.Current?.Dispatcher?.BeginInvoke(action, null);
        }
    }
}