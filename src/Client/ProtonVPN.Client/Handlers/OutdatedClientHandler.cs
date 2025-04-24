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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.Messages;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;

namespace ProtonVPN.Client.Handlers;

public class OutdatedClientHandler : IHandler, IOutdatedClientNotifier,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<HomePageDisplayedAfterLoginMessage>,
    IEventMessageReceiver<ApplicationStartedMessage>
{
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    private object _lock = new();

    private bool _isClientOutdated;
    private bool _isClientNotified;

    public OutdatedClientHandler(
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IEventMessageSender eventMessageSender,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _eventMessageSender = eventMessageSender;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    public void Receive(LoggedOutMessage message)
    {
        if (message.Reason != LogoutReason.ClientOutdated)
        {
            return;
        }

        NotifyOutdatedClient();
    }

    public void OnClientOutdated()
    {
        _isClientOutdated = true;
    }

    public void Receive(HomePageDisplayedAfterLoginMessage message)
    {
        if (_isClientOutdated)
        {
            _eventMessageSender.Send<ClientOutdatedMessage>();
        }
    }

    public void Receive(ApplicationStartedMessage message)
    {
        NotifyOutdatedClient();
    }

    private void NotifyOutdatedClient()
    {
        if (!_isClientOutdated)
        {
            return;
        }

        lock (_lock)
        {
            if (_isClientNotified)
            {
                return;
            }

            _isClientNotified = true;

            _uiThreadDispatcher.TryEnqueue(async () =>
            {
                await _mainWindowOverlayActivator.ShowOutdatedClientOverlayAsync();
            });
        }
    }
}