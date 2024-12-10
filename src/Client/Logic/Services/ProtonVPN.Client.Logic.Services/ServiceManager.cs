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

using ProtonVPN.Client.Common.Messages;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.OperatingSystems.Services.Contracts;

namespace ProtonVPN.Client.Logic.Services;

public class ServiceManager : IServiceManager, IEventMessageReceiver<ApplicationStoppedMessage>
{
    private readonly IService _service;
    private readonly IServiceEnabler _serviceEnabler;
    private readonly IBaseFilteringEngineDialogHandler _baseFilteringEngineDialogHandler;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ServiceManager(
        IServiceFactory serviceFactory,
        IStaticConfiguration configuration,
        IServiceEnabler serviceEnabler,
        IBaseFilteringEngineDialogHandler baseFilteringEngineDialogHandler)
    {
        _service = serviceFactory.Get(configuration.ServiceName);
        _serviceEnabler = serviceEnabler;
        _baseFilteringEngineDialogHandler = baseFilteringEngineDialogHandler;
    }

    public void Receive(ApplicationStoppedMessage message)
    {
        _cancellationTokenSource.Cancel();
        Stop();
    }

    public async Task StartAsync()
    {
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        bool result = await _baseFilteringEngineDialogHandler.HandleAsync();
        if (!result)
        {
            return;
        }

        await _serviceEnabler.EnableAsync(_service);

        _service.Start();
    }

    public void Stop()
    {
        _service.Stop();
    }
}