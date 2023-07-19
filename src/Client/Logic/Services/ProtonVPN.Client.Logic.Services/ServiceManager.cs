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
using ProtonVPN.OperatingSystems.Services.Contracts;

namespace ProtonVPN.Client.Logic.Services;

public class ServiceManager : IServiceManager
{
#warning This should be a configuration
    // TODO This should be a configuration
    private const string SERVICE_NAME = "ProtonVPN Service";

    private readonly IService _service;

    public ServiceManager(IServiceFactory serviceFactory)
    {
        _service = serviceFactory.Get(SERVICE_NAME);
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