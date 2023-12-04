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
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.OperatingSystems.Services.Contracts;

namespace ProtonVPN.Client.Logic.Services;

public class ServiceManager : IServiceManager
{
    private readonly IService _service;

    public ServiceManager(IServiceFactory serviceFactory,
        IStaticConfiguration configuration)
    {
        _service = serviceFactory.Get(configuration.ServiceName);
    }

    public Task StartAsync()
    {
        return Task.Run(Start);
    }

    public Task StopAsync()
    {
        return Task.Run(Stop);
    }

    public void Start()
    {
        _service.Start();
    }

    public void Stop()
    {
        _service.Stop();
    }
}