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
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client;

public sealed partial class MainWindow
{
    private readonly ISettings _settings;
    private readonly IServiceManager _serviceManager;

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/ProtonVPN.ico"));
        Content = null;
        Title = App.APPLICATION_NAME;

        _settings = App.GetService<ISettings>();
        _serviceManager = App.GetService<IServiceManager>();
        SetInitialSizeAndPosition();

        Closed += OnWindowClosed;
        VisibilityChanged += OnVisibilityChanged;
    }

    private void SetInitialSizeAndPosition()
    {
        double? windowWidth = _settings.WindowWidth;
        double? windowHeight = _settings.WindowHeight;
        double? windowXPosition = _settings.WindowXPosition;
        double? windowYPosition = _settings.WindowYPosition;
        bool isWindowMaximized = _settings.IsWindowMaximized;

        if (windowXPosition is null || windowYPosition is null)
        {
            W32Point? point = CursorWindowCalculator.CalculateWindowCenteredInCursorMonitor(Width, Height);
            if (point is not null)
            {
                windowXPosition = point.Value.X;
                windowYPosition = point.Value.Y;
            }
        }

        if (windowXPosition is not null && windowYPosition is not null)
        {
            this.MoveAndResize(x: windowXPosition.Value, y: windowYPosition.Value,
                width: windowWidth ?? Width, height: windowHeight ?? Height);
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
}