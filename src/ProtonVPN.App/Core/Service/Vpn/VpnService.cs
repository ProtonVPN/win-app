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
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppServiceLogs;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Communication;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnService
    {
        private readonly ILogger _logger;
        private readonly IAppGrpcClient _grpcClient;
        private readonly VpnSystemService _vpnSystemService;

        public VpnService(ILogger logger, IAppGrpcClient grpcClient, VpnSystemService vpnSystemService)
        {
            _logger = logger;
            _grpcClient = grpcClient;
            _vpnSystemService = vpnSystemService;
            _grpcClient.CreateAsync();
        }

        public Task ApplySettings(MainSettingsIpcEntity settings)
        {
            return Invoke(c => c.ApplySettings(settings).Wrap());
        }

        public Task Connect(ConnectionRequestIpcEntity connectionRequest)
        {
            return Invoke(c => c.Connect(connectionRequest).Wrap());
        }

        public Task UpdateAuthCertificate(AuthCertificateIpcEntity certificate)
        {
            return Invoke(c => c.UpdateAuthCertificate(certificate).Wrap());
        }

        public Task Disconnect(DisconnectionRequestIpcEntity disconnectionRequest)
        {
            return Invoke(c => c.Disconnect(disconnectionRequest).Wrap());
        }

        public Task RepeatState()
        {
            return Invoke(c => c.RepeatState().Wrap());
        }

        public Task<TrafficBytesIpcEntity> Total()
        {
            return Invoke(c => c.GetTrafficBytes());
        }

        public Task RequestNetShieldStats()
        {
            return Invoke(c => c.RequestNetShieldStats().Wrap());
        }

        public Task RegisterVpnClient(int port)
        {
            return Invoke(c => c.RegisterStateConsumer(new StateConsumerIpcEntity() { ServerPort = port }).Wrap());
        }

        private async Task<T> Invoke<T>(Func<IServiceController, Task<T>> serviceCall,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            int retryCount = 5;
            while (true)
            {
                try
                {
                    IServiceController serviceController = await _grpcClient.GetServiceControllerOrThrowAsync(TimeSpan.FromSeconds(1));
                    T result = await serviceCall(serviceController);
                    if (result is Task task)
                    {
                        await task;
                    }
                    return result;
                }
                catch (Exception e)
                {
                    await StartServiceIfStoppedAsync();
                    if (retryCount <= 0)
                    {
                        LogError(e, memberName, isToRetry: false);
                        throw;
                    }
                    await _grpcClient.RecreateAsync();
                    LogError(e, memberName, isToRetry: true);
                }
                retryCount--;
            }
        }

        private async Task StartServiceIfStoppedAsync()
        {
            await _vpnSystemService.StartIfStoppedAsync();
        }

        private void LogError(Exception exception, string callerMemberName, bool isToRetry)
        {
            _logger.Error<AppServiceCommunicationFailedLog>(
                $"The invocation of '{callerMemberName}' on VPN Service channel returned an exception and will " +
                (isToRetry ? string.Empty : "not ") +
                $"be retried. Exception message: {exception.Message}");
        }
    }
}
