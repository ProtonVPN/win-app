/*
 * Copyright (c) 2025 Proton AG
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

using System.ServiceProcess;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.OperatingSystems.Services.Contracts;

namespace ProtonVPN.Client.Logic.Services;

public class ServiceManager : IServiceManager
{
    private readonly IService _service;
    private readonly IServiceEnabler _serviceEnabler;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ServiceManager(
        IServiceFactory serviceFactory,
        IStaticConfiguration configuration,
        IServiceEnabler serviceEnabler)
    {
        _service = serviceFactory.Get(configuration.ServiceName);
        _serviceEnabler = serviceEnabler;
    }

    public ServiceControllerStatus? GetStatus()
    {
        return _service.GetStatus();
    }

    public async Task StartAsync()
    {
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        await _serviceEnabler.EnableAsync(_service);

        _service.Start();
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _service.Stop();
    }
}