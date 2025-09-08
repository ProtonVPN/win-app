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

using ProtonVPN.Client.Common.Messages;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.ProcessCommunication.Client;

namespace ProtonVPN.Client.Handlers;

public class ApplicationStoppedMessageHandler : IHandler,
    IEventMessageReceiver<ApplicationStoppedMessage>
{
    private readonly IServiceManager _serviceManager;
    private readonly IEnumerable<IServiceCaller> _serviceCallers;
    private readonly IClientControllerListener _clientControllerListener;
    private readonly INamedPipesConnectionFactory _namedPipesConnectionFactory;

    public ApplicationStoppedMessageHandler(
        IServiceManager serviceManager,
        IEnumerable<IServiceCaller> serviceCallers,
        IClientControllerListener clientControllerListener,
        INamedPipesConnectionFactory namedPipesConnectionFactory)
    {
        _serviceManager = serviceManager;
        _serviceCallers = serviceCallers;
        _clientControllerListener = clientControllerListener;
        _namedPipesConnectionFactory = namedPipesConnectionFactory;
    }

    public void Receive(ApplicationStoppedMessage message)
    {
        _namedPipesConnectionFactory.Stop();

        foreach (IServiceCaller serviceCaller in _serviceCallers)
        {
            serviceCaller.Stop();
        }

        _clientControllerListener.Stop();
        _serviceManager.Stop();
    }
}