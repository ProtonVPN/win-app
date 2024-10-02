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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Common;
using ProtonVPN.ProcessCommunication.Common.Channels;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Communication;
using ProtonVPN.ProcessCommunication.Contracts.Registration;

namespace ProtonVPN.ProcessCommunication.App
{
    public class AppGrpcClient : GrpcClientBase, IAppGrpcClient
    {
        private readonly IServiceServerPortRegister _serviceServerPortRegister;
        private readonly IGrpcServer _grpcServer;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public IVpnController VpnController { get; private set; }
        public IUpdateController UpdateController { get; private set; }

        public AppGrpcClient(ILogger logger,
            IServiceServerPortRegister serviceServerPortRegister,
            IGrpcServer grpcServer,
            IGrpcChannelWrapperFactory grpcChannelWrapperFactory)
            : base(logger, grpcChannelWrapperFactory)
        {
            _serviceServerPortRegister = serviceServerPortRegister;
            _grpcServer = grpcServer;
        }

        public async Task CreateAsync()
        {
            await SafeWrapperAsync(async () =>
            {
                if (VpnController is null || UpdateController is null)
                {
                    await CreateInternalAsync();
                }
            });
        }

        private async Task SafeWrapperAsync(Func<Task> function)
        {
            bool isToEnter = await _semaphore.WaitAsync(0);
            if (isToEnter)
            {
                try
                {
                    await function();
                }
                catch
                {
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        private async Task CreateInternalAsync()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            int serviceServerPort = await _serviceServerPortRegister.ReadAsync(cts.Token);
            await CreateWithPortAsync(serviceServerPort);
            int? appServerPort = _grpcServer?.Port;
            if (VpnController is not null && appServerPort is not null)
            {
                await RegisterStateConsumerAsync(appServerPort.Value);
            }
        }

        private async Task RegisterStateConsumerAsync(int appServerPort)
        {
            Logger.Info<ProcessCommunicationLog>($"Sending the app gRPC server port {appServerPort} to service.");
            try
            {
                await VpnController.RegisterStateConsumer(new StateConsumerIpcEntity
                {
                    ServerPort = appServerPort
                });
            }
            catch (Exception e)
            {
                Logger.Error<ProcessCommunicationErrorLog>($"An error occurred when " +
                    $"sending the app gRPC server port {appServerPort} to service.", e);
            }
        }

        public async Task RecreateAsync()
        {
            await SafeWrapperAsync(async () =>
            {
                VpnController = null;
                UpdateController = null;
                await CreateInternalAsync();
            });
        }

        protected override void RegisterServices(IGrpcChannelWrapper channel)
        {
            VpnController = channel.CreateService<IVpnController>();
            UpdateController = channel.CreateService<IUpdateController>();
        }

        public async Task<T> GetServiceControllerOrThrowAsync<T>(TimeSpan timeout) where T : IServiceController
        {
            CancellationTokenSource cts = new(timeout);
            IServiceController serviceController = GetServiceController<T>();

            try
            {
                while (serviceController is null && !cts.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cts.Token);
                    serviceController = GetServiceController<T>();
                }
            }
            catch
            {
            }

            if (serviceController is null)
            {
                string errorMessage = $"Failed to get the Service Controller within the allotted time '{timeout}'.";
                Logger.Error<ProcessCommunicationErrorLog>(errorMessage);
                throw new TimeoutException(errorMessage);
            }

            return (T)serviceController;
        }

        public IServiceController GetServiceController<T>()
        {
            if (typeof(T).IsAssignableFrom(typeof(IVpnController)))
            {
                return VpnController;
            }

            if (typeof(T).IsAssignableFrom(typeof(IUpdateController)))
            {
                return UpdateController;
            }

            throw new NotImplementedException($"Controller of type {typeof(T)} is not supported.");
        }
    }
}