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

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Common.UI.Windowing;
using ProtonVPN.Client.Common.UI.Windowing.System;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client;

public sealed partial class MainWindow
{
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IServiceManager _serviceManager;

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/ProtonVPN.ico"));
        AppWindow.Title = Shell.ViewModel.Title;
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        Shell.Initialize(this);

        _eventMessageSender = App.GetService<IEventMessageSender>();

        _settings = App.GetService<ISettings>();
        _serviceManager = App.GetService<IServiceManager>();
        SetInitialSizeAndPosition();

        Closed += OnWindowClosed;
        VisibilityChanged += OnVisibilityChanged;
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        AppTitleBar.Opacity = args.WindowActivationState != WindowActivationState.Deactivated ? 1.0 : 0.5;
    }

    private void SetInitialSizeAndPosition()
    {
        double windowWidth = _settings.WindowWidth ?? Width;
        double windowHeight = _settings.WindowHeight ?? Height;
        double? windowXPosition = _settings.WindowXPosition;
        double? windowYPosition = _settings.WindowYPosition;
        bool isWindowMaximized = _settings.IsWindowMaximized;

        if (windowXPosition is null || windowYPosition is null)
        {
            W32Point? point = MonitorCalculator.CalculateWindowCenteredInCursorMonitor(Width, Height);
            if (point is not null)
            {
                windowXPosition = point.Value.X;
                windowYPosition = point.Value.Y;
            }
        }

        if (windowXPosition is not null && windowYPosition is not null)
        {
            W32Rect windowRectangle = new(new W32Point((int)windowXPosition, (int)windowYPosition),
                width: (int)windowWidth, height: (int)windowHeight);
            W32Rect? validWindowRectangle = MonitorCalculator.GetValidWindowSizeAndPosition(windowRectangle);
            if (validWindowRectangle is not null)
            {
                this.MoveAndResize(x: validWindowRectangle.Value.Left, y: validWindowRectangle.Value.Top,
                    width: validWindowRectangle.Value.GetWidth(), height: validWindowRectangle.Value.GetHeight());
            }
        }

        if (isWindowMaximized)
        {
            Activated += MaximizeOnFirstActivation;
        }
    }

    private void MaximizeOnFirstActivation(object sender, WindowActivatedEventArgs args)
    {
        this.Maximize();
        Activated -= MaximizeOnFirstActivation;
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        _eventMessageSender.Send(new MainWindowClosedMessage());

        SaveWindowState((MainWindow)sender);
        _serviceManager.Stop();
    }

    private void OnVisibilityChanged(object sender, WindowVisibilityChangedEventArgs args)
    {
        SaveWindowState((MainWindow)sender);
    }

    private void SaveWindowState(MainWindow mainWindow)
    {
        OverlappedPresenterState? windowState = GetWindowState(mainWindow);

        if (windowState == OverlappedPresenterState.Restored)
        {
            SaveWindowPosition(mainWindow);
            _settings.WindowWidth = Convert.ToInt32(mainWindow.Width);
            _settings.WindowHeight = Convert.ToInt32(mainWindow.Height);
            _settings.IsWindowMaximized = false;
        }
        else if (windowState == OverlappedPresenterState.Maximized)
        {
            SaveWindowPosition(mainWindow);
            _settings.IsWindowMaximized = true;
        }
    }

    private void SaveWindowPosition(MainWindow mainWindow)
    {
        _settings.WindowXPosition = mainWindow.AppWindow.Position.X;
        _settings.WindowYPosition = mainWindow.AppWindow.Position.Y;
    }

    private OverlappedPresenterState? GetWindowState(MainWindow mainWindow)
    {
        return mainWindow.Presenter is not null and OverlappedPresenter overlappedPresenter
               ? overlappedPresenter.State
               : null;
    }

    private void OnAppTitleBarSizeChanged(object sender, SizeChangedEventArgs e)
    {
        this.SetDragAreaFromTitleBar(AppTitleBar);
    }
}