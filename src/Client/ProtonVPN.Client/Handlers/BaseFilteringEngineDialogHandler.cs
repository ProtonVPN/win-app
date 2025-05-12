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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Contracts.Services.Lifecycle;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;

namespace ProtonVPN.Client.Handlers;

public class BaseFilteringEngineDialogHandler : IHandler,
    IEventMessageReceiver<ConnectionErrorMessage>
{
    private readonly ILogger _logger;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly ILocalizationProvider _localizer;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly IAppExitInvoker _appExitInvoker;

    private bool _isHandled;

    public BaseFilteringEngineDialogHandler(
        ILogger logger,
        IUrlsBrowser urlsBrowser,
        ILocalizationProvider localizer,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IUIThreadDispatcher uiThreadDispatcher,
        IAppExitInvoker appExitInvoker)
    {
        _logger = logger;
        _urlsBrowser = urlsBrowser;
        _localizer = localizer;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _uiThreadDispatcher = uiThreadDispatcher;
        _appExitInvoker = appExitInvoker;
    }

    public void Receive(ConnectionErrorMessage message)
    {
        if (message.VpnError.IsBaseFilteringEngineError())
        {
            _uiThreadDispatcher.TryEnqueue(async () => await ShowOverlayAndExitAsync());
        }
    }

    private async Task ShowOverlayAndExitAsync()
    {
        if (_isHandled)
        {
            return;
        }

        try
        {
            _isHandled = true;

            _logger.Warn<AppServiceLog>($"Base Filtering Engine (BFE) service is disabled.");

            MessageDialogParameters parameters = new()
            {
                Title = _localizer.GetFormat("Dialogs_BaseFilteringEngine_Title"),
                Message = $"{_localizer.Get("Dialogs_BaseFilteringEngine_Description")}{Environment.NewLine}{Environment.NewLine}",
                MessageType = DialogMessageType.RichText,
                PrimaryButtonText = _localizer.Get("Common_Actions_Exit_ProtonVPN"),
                TrailingInlineButton = new InlineTextButton()
                {
                    Text = _localizer.Get("Dialogs_BaseFilteringEngine_HowToEnable"),
                    Url = _urlsBrowser.EnableBaseFilteringEngine,
                },
                UseVerticalLayoutForButtons = true,
            };

            await _mainWindowOverlayActivator.ShowMessageAsync(parameters);

            _logger.Warn<AppServiceLog>("Shutting down the application.");

            await _appExitInvoker.ForceExitAsync();
        }
        catch (Exception e)
        {
            _logger.Error<AppServiceLog>("Failed to show BFE warning overlay.", e);
        }
        finally
        {
            _isHandled = false;
        }
    }
}