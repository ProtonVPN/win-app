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
using ProtonVPN.Common.Extensions;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnServiceCaller : ServiceCallerBase<IVpnController>, IVpnServiceCaller
    {
        public VpnServiceCaller(IServiceControllerCaller serviceControllerCaller)
            : base(serviceControllerCaller)
        {
        }

        public Task ApplySettings(MainSettingsIpcEntity settings)
        {
            return InvokeAsync((c, ct) => c.ApplySettings(settings, ct).Wrap());
        }

        public Task Connect(ConnectionRequestIpcEntity connectionRequest)
        {
            return InvokeAsync((c, ct) => c.Connect(connectionRequest, ct).Wrap());
        }

        public Task UpdateConnectionCertificate(ConnectionCertificateIpcEntity certificate)
        {
            return InvokeAsync((c, ct) => c.UpdateConnectionCertificate(certificate, ct).Wrap());
        }

        public Task Disconnect(DisconnectionRequestIpcEntity disconnectionRequest)
        {
            return InvokeAsync((c, ct) => c.Disconnect(disconnectionRequest, ct).Wrap());
        }

        public Task RepeatState()
        {
            return InvokeAsync((c, ct) => c.RepeatState(ct).Wrap());
        }

        public Task<Result<TrafficBytesIpcEntity>> GetTrafficBytes()
        {
            return InvokeAsync((c, ct) => c.GetTrafficBytes(ct));
        }

        public Task RequestNetShieldStats()
        {
            return InvokeAsync((c, ct) => c.RequestNetShieldStats(ct).Wrap());
        }
    }
}