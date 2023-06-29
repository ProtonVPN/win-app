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

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

using ProtonVPN.Client.Activation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.UI;

namespace ProtonVPN.Client.Models.MainWindowActivation;

public class MainWindowActivator : IMainWindowActivator
{
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IThemeSelector _themeSelector;
    private readonly ShellViewModel _shellViewModel;

    public MainWindowActivator(ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers,
        IThemeSelector themeSelector,
        ShellViewModel shellViewModel)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _themeSelector = themeSelector;
        _shellViewModel = shellViewModel;
    }

    public async Task ActivateAsync(LaunchActivatedEventArgs activationArgs)
    {
        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            App.MainWindow.Content = new ShellPage(_shellViewModel);
        }

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        // Activate the MainWindow.
        App.MainWindow.Activate();
        _themeSelector.Initialize();

        // Set the theme again once all priority events are done such as UI rendering, in order to fix the TitleBar bug.
        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () => _themeSelector.Initialize());
    }

    private async Task HandleActivationAsync(LaunchActivatedEventArgs activationArgs)
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
}