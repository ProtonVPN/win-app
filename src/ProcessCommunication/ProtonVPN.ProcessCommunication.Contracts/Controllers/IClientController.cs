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

using System.ServiceModel;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.Contracts.Controllers;

[ServiceContract]
public interface IClientController
{
    [OperationContract]
    IAsyncEnumerable<VpnStateIpcEntity> StreamVpnStateChangeAsync(CancellationToken cancelToken);

    [OperationContract]
    IAsyncEnumerable<PortForwardingStateIpcEntity> StreamPortForwardingStateChangeAsync(CancellationToken cancelToken);

    [OperationContract]
    IAsyncEnumerable<ConnectionDetailsIpcEntity> StreamConnectionDetailsChangeAsync(CancellationToken cancelToken);

    [OperationContract]
    IAsyncEnumerable<NetShieldStatisticIpcEntity> StreamNetShieldStatisticChangeAsync(CancellationToken cancelToken);

    [OperationContract]
    IAsyncEnumerable<UpdateStateIpcEntity> StreamUpdateStateChangeAsync(CancellationToken cancelToken);
}