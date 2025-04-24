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
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.OperatingSystems.Network.Contracts;

namespace ProtonVPN.Client.Handlers;

public class SystemProxyDetectionHandler : IHandler,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly ILocalizationProvider _localizer;
    private readonly IProxyDetector _proxyDetector;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    private bool _isToShowWarningOverlay = true;

    public SystemProxyDetectionHandler(
        IUrlsBrowser urlsBrowser,
        ILocalizationProvider localizer,
        IProxyDetector proxyDetector,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _urlsBrowser = urlsBrowser;
        _localizer = localizer;
        _proxyDetector = proxyDetector;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (message.ConnectionStatus == ConnectionStatus.Connected && _proxyDetector.IsEnabled() && _isToShowWarningOverlay)
        {
            _isToShowWarningOverlay = false;

            _uiThreadDispatcher.TryEnqueue(async () => await HandleAsync());
        }

        if (message.ConnectionStatus == ConnectionStatus.Disconnected)
        {
            _isToShowWarningOverlay = true;
        }
    }

    private Task HandleAsync()
    {
        MessageDialogParameters parameters = new()
        {
            Title = _localizer.Get("Dialogs_SystemProxy_Title"),
            Message = _localizer.Get("Dialogs_SystemProxy_Description"),
            PrimaryButtonText = _localizer.Get("Common_Actions_GotIt"),
            MessageType = DialogMessageType.RichText,
            TrailingInlineButton = new InlineTextButton()
            {
                Text = _localizer.Get("Common_Links_LearnMore"),
                Url = _urlsBrowser.ActiveProxyLearnMore
            },
            UseVerticalLayoutForButtons = true,
        };

        return _mainWindowOverlayActivator.ShowMessageAsync(parameters);
    }
}