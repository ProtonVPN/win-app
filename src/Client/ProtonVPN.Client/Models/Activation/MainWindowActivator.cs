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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Models.Activation;

public class MainWindowActivator : WindowActivatorBase, IMainWindowActivator, IEventMessageReceiver<LoginSuccessMessage>, IEventMessageReceiver<LogoutMessage>
{
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IDialogActivator _dialogActivator;

    public MainWindowActivator(ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers,
        ILogger logger,
        IThemeSelector themeSelector,
        IDialogActivator dialogActivator)
        : base(logger, themeSelector)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _dialogActivator = dialogActivator;

        // By default app can't be resized, minimized, maximized until user logged in
        App.MainWindow.IsMinimizable = false;
        App.MainWindow.IsMaximizable = false;
        App.MainWindow.IsResizable = false;
    }

    public async Task ActivateAsync(LaunchActivatedEventArgs activationArgs)
    {
        App.MainWindow.Closed += OnMainWindowClosed;

        App.MainWindow.ApplyTheme(ThemeSelector.GetTheme().Theme);

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        // Activate the MainWindow.
        App.MainWindow.Activate();
    }

    public void Receive(LoginSuccessMessage message)
    {
        App.MainWindow.IsMinimizable = true;
        App.MainWindow.IsMaximizable = true;
        App.MainWindow.IsResizable = true;
    }

    public void Receive(LogoutMessage message)
    {
        App.MainWindow.IsMinimizable = false;
        App.MainWindow.IsMaximizable = false;
        App.MainWindow.IsResizable = false;

        // Force dark theme when logged out.
        // Theme is saved in user settings which cannot be retrieved until user logged in
        App.MainWindow.ApplyTheme(ElementTheme.Dark);
    }

    protected override void OnThemeChanged(ElementTheme theme)
    {
        App.MainWindow.ApplyTheme(theme);
    }

    private void OnMainWindowClosed(object sender, WindowEventArgs args)
    {
        App.MainWindow.Closed -= OnMainWindowClosed;

        _dialogActivator.CloseAll();
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