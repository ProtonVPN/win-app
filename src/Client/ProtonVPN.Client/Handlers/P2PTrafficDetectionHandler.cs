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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;

namespace ProtonVPN.Client.Handlers;

public class P2PTrafficDetectionHandler : IHandler,
    IEventMessageReceiver<P2PTrafficDetectedMessage>
{
    private readonly ILocalizationProvider _localizer;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    public P2PTrafficDetectionHandler(
        ILocalizationProvider localizer,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _localizer = localizer;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    public void Receive(P2PTrafficDetectedMessage message)
    {
        _uiThreadDispatcher.TryEnqueue(async () => await HandleAsync());
    }

    private async Task HandleAsync()
    {
        MessageDialogParameters parameters = new()
        {
            Title = _localizer.Get("Dialogs_P2PDetection_Title"),
            Message = _localizer.Get("Dialogs_P2PDetection_Description"),
            PrimaryButtonText = _localizer.Get("Common_Actions_Close"),
            UseVerticalLayoutForButtons = true,
        };

        await _mainWindowOverlayActivator.ShowMessageAsync(parameters);
    }
}