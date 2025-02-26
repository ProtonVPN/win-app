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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.OperatingSystems.Services.Contracts;

namespace ProtonVPN.Client.Handlers;

public class BaseFilteringEngineDialogHandler : IBaseFilteringEngineDialogHandler
{
    private readonly ILogger _logger;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IServiceFactory _serviceFactory;
    private readonly ILocalizationProvider _localizer;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IUIThreadDispatcher _uIThreadDispatcher;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public BaseFilteringEngineDialogHandler(
        ILogger logger,
        IUrlsBrowser urlsBrowser,
        IStaticConfiguration staticConfiguration,
        IServiceFactory serviceFactory,
        ILocalizationProvider localizer,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IUIThreadDispatcher uIThreadDispatcher)
    {
        _logger = logger;
        _urlsBrowser = urlsBrowser;
        _staticConfiguration = staticConfiguration;
        _serviceFactory = serviceFactory;
        _localizer = localizer;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _uIThreadDispatcher = uIThreadDispatcher;
    }

    public async Task<bool> HandleAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            return await _uIThreadDispatcher.TryEnqueueAsync(ShowOverlayAsync);
        }
        catch (Exception)
        {
            // The first call from bootstrapper calls this method too early and the UI is not yet ready.
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ShowOverlayAsync()
    {
        IService bfeService = _serviceFactory.Get(_staticConfiguration.BaseFilteringEngineServiceName);
        if (bfeService.IsEnabled() && bfeService.IsRunning())
        {
            return;
        }

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

        _logger.Warn<AppServiceLog>("Base Filtering Engine (BFE) service is disabled. Shutting down the application.");

        Environment.Exit(0);
    }
}