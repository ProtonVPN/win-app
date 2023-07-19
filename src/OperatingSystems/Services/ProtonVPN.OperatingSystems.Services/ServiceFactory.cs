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

using System.Collections.Concurrent;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.OperatingSystems.Processes.Contracts;
using ProtonVPN.OperatingSystems.Services.Contracts;

namespace ProtonVPN.OperatingSystems.Services;

public class ServiceFactory : IServiceFactory
{
    private readonly ILogger _logger;
    private readonly ICommandLineCaller _commandLineCaller;
    private readonly ConcurrentDictionary<string, IService> _services = new();
    private readonly object _lock = new();

    public ServiceFactory(ILogger logger, ICommandLineCaller commandLineCaller)
    {
        _logger = logger;
        _commandLineCaller = commandLineCaller;
    }

    public IService Get(string name)
    {
        IService? service;
        if (_services.TryGetValue(name, out service))
        {
            return service;
        }
        lock (_lock)
        {
            if (_services.TryGetValue(name, out service))
            {
                return service;
            }
            service = new Service(name, _logger, _commandLineCaller);
            _services.TryAdd(name, service);
            return service;
        }
    }
}