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
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Contracts.Bases;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Services.Navigation;
using ProtonVPN.Client.UI.Main.Home;
using Windows.Foundation;

namespace ProtonVPN.Client.UI.Main;

public sealed partial class MainPageView : IContextAware
{
    private const double MAIN_FRAME_LEFT_MARGIN = 10.0;

    public MainPageViewModel ViewModel { get; }

    public MainViewNavigator Navigator { get; }

    public MainPageView()
    {
        ViewModel = App.GetService<MainPageViewModel>();
        Navigator = App.GetService<MainViewNavigator>();

        InitializeComponent();

        Navigator.Initialize(MainNavigationFrame);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        LayoutRoot.PointerPressed += OnPointerPressed;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Navigator.Load();
        ViewModel.Activate();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
        Navigator.Unload();
    }

    private void OnSidebarPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ViewModel.OnSidebarInteractionStarted();
    }

    private void OnSidebarPointerExited(object sender, PointerRoutedEventArgs e)
    {
        ViewModel.OnSidebarInteractionEnded();
    }

    private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        Point point = e.GetCurrentPoint(LayoutRoot).Position;
        GeneralTransform transform = MainNavigationFrame.TransformToVisual(LayoutRoot);
        Rect mainNavigationFrameRect = new(new Point(0, 0), MainNavigationFrame.RenderSize);
        mainNavigationFrameRect = transform.TransformBounds(mainNavigationFrameRect);

        // Add some margin to the left so that we can resize the sidebar without navigating to home page
        mainNavigationFrameRect.X -= MAIN_FRAME_LEFT_MARGIN;
        mainNavigationFrameRect.Width += MAIN_FRAME_LEFT_MARGIN;

        if (!mainNavigationFrameRect.Contains(point))
        {
            await ViewModel.CloseCurrentSettingsPageAsync();
        }
    }
}