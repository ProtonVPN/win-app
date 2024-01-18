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

using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Communication;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Services;

public class VpnServiceCaller : ServiceCallerBase<IVpnController>, IVpnServiceCaller
{
    public VpnServiceCaller(ILogger logger, IAppGrpcClient grpcClient, IServiceManager serviceManager)
        : base(logger, grpcClient, serviceManager)
    { }

    public Task RegisterClientAsync(int appServerPort, CancellationToken cancellationToken)
    {
        return InvokeAsync(c => c.RegisterStateConsumer(new StateConsumerIpcEntity { ServerPort = appServerPort }).Wrap());
    }

    public Task ConnectAsync(ConnectionRequestIpcEntity connectionRequest)
    {
        return InvokeAsync(c => c.Connect(connectionRequest).Wrap());
    }

    public Task DisconnectAsync(DisconnectionRequestIpcEntity disconnectionRequest)
    {
        return InvokeAsync(c => c.Disconnect(disconnectionRequest).Wrap());
    }

    public Task<Result<TrafficBytesIpcEntity>> GetTrafficBytesAsync()
    {
         return InvokeAsync(c => c.GetTrafficBytes());
    }

    public Task UpdateAuthCertificateAsync(AuthCertificateIpcEntity certificate)
    {
        return InvokeAsync(c => c.UpdateAuthCertificate(certificate).Wrap());
    }

    public Task ApplySettingsAsync(MainSettingsIpcEntity settings)
    {
        return InvokeAsync(c => c.ApplySettings(settings).Wrap());
    }
}