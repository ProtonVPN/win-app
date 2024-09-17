/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using System;

namespace ProtonVPN.Core.Service.Vpn
{
    public class ClientControllerEventHandler : IClientControllerEventHandler
    {
        public event EventHandler<VpnStateIpcEntity> OnVpnStateChanged;
        public event EventHandler<PortForwardingStateIpcEntity> OnPortForwardingStateChanged;
        public event EventHandler<ConnectionDetailsIpcEntity> OnConnectionDetailsChanged;
        public event EventHandler<NetShieldStatisticIpcEntity> OnNetShieldStatisticChanged;
        public event EventHandler<UpdateStateIpcEntity> OnUpdateStateChanged;
        public event EventHandler OnOpenWindowInvoked;

        public void InvokeVpnStateChanged(VpnStateIpcEntity entity)
        {
            OnVpnStateChanged?.Invoke(this, entity);
        }

        public void InvokePortForwardingStateChanged(PortForwardingStateIpcEntity entity)
        {
            OnPortForwardingStateChanged?.Invoke(this, entity);
        }

        public void InvokeConnectionDetailsChanged(ConnectionDetailsIpcEntity entity)
        {
            OnConnectionDetailsChanged?.Invoke(this, entity);
        }

        public void InvokeNetShieldStatisticChanged(NetShieldStatisticIpcEntity entity)
        {
            OnNetShieldStatisticChanged?.Invoke(this, entity);
        }

        public void InvokeUpdateStateChanged(UpdateStateIpcEntity entity)
        {
            OnUpdateStateChanged?.Invoke(this, entity);
        }

        public void InvokeOpenWindowInvoked()
        {
            OnOpenWindowInvoked?.Invoke(this, null);
        }
    }
}
