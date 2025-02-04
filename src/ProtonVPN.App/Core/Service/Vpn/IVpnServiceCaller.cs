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

using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public interface IVpnServiceCaller
    {
        Task ApplySettings(MainSettingsIpcEntity settings);
        Task Connect(ConnectionRequestIpcEntity connectionRequest);
        Task UpdateConnectionCertificate(ConnectionCertificateIpcEntity certificate);
        Task Disconnect(DisconnectionRequestIpcEntity disconnectionRequest);
        Task RepeatState();
        Task<Result<TrafficBytesIpcEntity>> GetTrafficBytes();
        Task RequestNetShieldStats();
    }
}