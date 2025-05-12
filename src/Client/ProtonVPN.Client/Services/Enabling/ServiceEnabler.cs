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
using ProtonVPN.Client.Contracts.Services.Lifecycle;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.OperatingSystems.Services.Contracts;

namespace ProtonVPN.Client.Services.Enabling;

public class ServiceEnabler : IServiceEnabler
{
    private readonly ILogger _logger;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly ILocalizationProvider _localizer;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly Lazy<IAppExitInvoker> _appExitInvoker;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ServiceEnabler(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        ILocalizationProvider localizer,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        Lazy<IAppExitInvoker> appExitInvoker)
    {
        _logger = logger;
        _uiThreadDispatcher = uiThreadDispatcher;
        _localizer = localizer;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _appExitInvoker = appExitInvoker;
    }

    public async Task EnableAsync(IService service)
    {
        if (service.IsEnabled())
        {
            return;
        }

        await _semaphore.WaitAsync();

        try
        {
            await _uiThreadDispatcher.TryEnqueueAsync(() => ShowOverlayAsync(service));
        }
        catch (Exception)
        {
            // The first call from bootstrapper calls this method too early and the UI is not yet ready.
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ShowOverlayAsync(IService service)
    {
        if (service.IsEnabled())
        {
            return;
        }

        MessageDialogParameters parameters = new()
        {
            Title = _localizer.GetFormat("Dialogs_DisabledService_Title"),
            Message = _localizer.Get("Dialogs_DisabledService_Description"),
            PrimaryButtonText = _localizer.Get("Common_Actions_Enable"),
            CloseButtonText = _localizer.Get("Common_Actions_Close"),
        };

        ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(parameters);
        if (result is ContentDialogResult.Primary)
        {
            _logger.Info<AppServiceLog>($"The user requested to enable service {service.Name}.");
            service.Enable();
        }
        else
        {
            _logger.Info<AppServiceLog>($"The user refused to enable service {service.Name}.");
        }

        if (!service.IsEnabled())
        {
            _logger.Info<AppServiceLog>($"The service {service.Name} was not enabled. Shutting down the application.");
            await _appExitInvoker.Value.ForceExitAsync();
        }
    }
}