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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Client.Handlers;

public class IncorrectSystemTimeHandler : IHandler, IEventMessageReceiver<IncorrectSystemTimeMessage>
{
    private readonly ILocalizationProvider _localizer;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly IUrlsBrowser _urlsBrowser;

    private readonly string _articleUrl;

    public IncorrectSystemTimeHandler(ILocalizationProvider localizer,
        IMainWindowActivator mainWindowActivator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IUIThreadDispatcher uiThreadDispatcher,
        IUrlsBrowser urlsBrowser,
        IConfiguration config)
    {
        _localizer = localizer;
        _mainWindowActivator = mainWindowActivator;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _uiThreadDispatcher = uiThreadDispatcher;
        _urlsBrowser = urlsBrowser;

        _articleUrl = config.Urls.IncorrectSystemTimeArticleUrl;
    }

    public void Receive(IncorrectSystemTimeMessage message)
    {
        _uiThreadDispatcher.TryEnqueue(async () => await HandleAsync());
    }

    private async Task HandleAsync()
    {
        _mainWindowActivator.Activate();

        ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = _localizer.Get("Dialogs_UpdateSystemClock_Title"),
                Message = _localizer.Get("Dialogs_UpdateSystemClock_Description"),
                MessageType = DialogMessageType.String,
                PrimaryButtonText = _localizer.Get("Dialogs_UpdateSystemClock_PrimaryButton"),
                CloseButtonText = _localizer.Get("Common_Actions_Close"),
                UseVerticalLayoutForButtons = true
            });

        if (result is ContentDialogResult.Primary)
        {
            _urlsBrowser.BrowseTo(_articleUrl);
        }
    }
}