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

using ProtonVPN.Client.Contracts.ProcessCommunication;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Services;

public class VpnServiceCaller : ServiceCallerBase<IVpnController>, IVpnServiceCaller
{
    public VpnServiceCaller(ILogger logger, IGrpcClient grpcClient,
        Lazy<IServiceCommunicationErrorHandler> serviceCommunicationErrorHandler)
        : base(logger, grpcClient, serviceCommunicationErrorHandler)
    { }

    public Task ConnectAsync(ConnectionRequestIpcEntity connectionRequest)
    {
        return InvokeAsync((c, ct) => c.Connect(connectionRequest, ct).Wrap());
    }

    public Task DisconnectAsync(DisconnectionRequestIpcEntity disconnectionRequest)
    {
        return InvokeAsync((c, ct) => c.Disconnect(disconnectionRequest, ct).Wrap());
    }

    public Task<Result<NetworkTrafficIpcEntity>> GetNetworkTrafficAsync()
    {
         return InvokeAsync((c, ct) => c.GetNetworkTraffic(ct));
    }

    public Task RequestNetShieldStatsAsync()
    {
        return InvokeAsync((c, ct) => c.RequestNetShieldStats(ct).Wrap());
    }

    public Task RequestConnectionDetailsAsync()
    {
        return InvokeAsync((c, ct) => c.RequestConnectionDetails(ct).Wrap());
    }

    public Task UpdateConnectionCertificateAsync(ConnectionCertificateIpcEntity certificate)
    {
        return InvokeAsync((c, ct) => c.UpdateConnectionCertificate(certificate, ct).Wrap());
    }

    public Task ApplySettingsAsync(MainSettingsIpcEntity settings)
    {
        return InvokeAsync((c, ct) => c.ApplySettings(settings, ct).Wrap());
    }

    public Task RepeatStateAsync()
    {
        return InvokeAsync((c, ct) => c.RepeatState(ct).Wrap());
    }

    public Task RepeatPortForwardingStateAsync()
    {
        return InvokeAsync((c, ct) => c.RepeatPortForwardingState(ct).Wrap());
    }
}