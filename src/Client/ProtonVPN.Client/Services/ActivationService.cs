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

using Microsoft.UI.Xaml;

using ProtonVPN.Client.Activation;
using ProtonVPN.Client.Contracts.Services;
using ProtonVPN.Client.UI;

namespace ProtonVPN.Client.Services;

public class ActivationService : IActivationService
{
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ShellViewModel _shellViewModel;

    public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers,
        IThemeSelectorService themeSelectorService,
        ShellViewModel shellViewModel)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _themeSelectorService = themeSelectorService;
        _shellViewModel = shellViewModel;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            App.MainWindow.Content = new ShellPage(_shellViewModel);
        }

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        // Activate the MainWindow.
        App.MainWindow.Activate();

        // Execute tasks after activation.
        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        IActivationHandler? activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    private async Task InitializeAsync()
    {
        await _themeSelectorService.InitializeAsync().ConfigureAwait(false);
        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
#warning Delaying the startup fix some issues with applying the requested theme (when the theme is different from the windows default one)
        await Task.Delay(200);

        await _themeSelectorService.SetRequestedThemeAsync();
    }
}