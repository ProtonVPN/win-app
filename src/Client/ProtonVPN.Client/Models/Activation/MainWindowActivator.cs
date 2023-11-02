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
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Models.Activation;

public class MainWindowActivator : WindowActivatorBase, IMainWindowActivator, IEventMessageReceiver<LoggedInMessage>, IEventMessageReceiver<LoggedOutMessage>
{
    private readonly MainWindow _mainWindow;

    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IDialogActivator _dialogActivator;
    private readonly IServiceManager _serviceManager;
    private readonly IEventMessageSender _eventMessageSender;

    public MainWindowActivator(
        MainWindow mainWindow,
        ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers,
        ILogger logger,
        IThemeSelector themeSelector,
        IDialogActivator dialogActivator,
        IServiceManager serviceManager,
        IEventMessageSender eventMessageSender)
        : base(logger, themeSelector)
    {
        _mainWindow = mainWindow;
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _dialogActivator = dialogActivator;
        _serviceManager = serviceManager;
        _eventMessageSender = eventMessageSender;

        // By default app can't be resized, minimized, maximized until user logged in
        _mainWindow.IsMinimizable = false;
        _mainWindow.IsMaximizable = false;
        _mainWindow.IsResizable = false;

        _mainWindow.Closed += OnMainWindowClosed;
    }

    public async Task ActivateAsync(LaunchActivatedEventArgs activationArgs)
    {
        _mainWindow.ApplyTheme(ThemeSelector.GetTheme().Theme);

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        // Activate the MainWindow.
        _mainWindow.Activate();
    }

    public void Receive(LoggedInMessage message)
    {
        _mainWindow.IsMinimizable = true;
        _mainWindow.IsMaximizable = true;
        _mainWindow.IsResizable = true;

        // Apply theme based on user settings
        InvalidateAppTheme();
    }

    public void Receive(LoggedOutMessage message)
    {
        _mainWindow.IsMinimizable = false;
        _mainWindow.IsMaximizable = false;
        _mainWindow.IsResizable = false;

        // Theme is saved in user settings which cannot be retrieved until user logged in.
        // When user logged out, app applies the default theme (Dark)
        InvalidateAppTheme();
    }

    protected override void OnThemeChanged(ElementTheme theme)
    {
        InvalidateAppTheme();
    }

    private void InvalidateAppTheme()
    {
        _mainWindow.ApplyTheme(ThemeSelector.GetTheme().Theme);
    }

    private void OnMainWindowClosed(object sender, WindowEventArgs args)
    {
        _mainWindow.Closed -= OnMainWindowClosed;

        _eventMessageSender.Send(new MainWindowClosedMessage());
        _dialogActivator.CloseAll();
        _serviceManager.Stop();
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