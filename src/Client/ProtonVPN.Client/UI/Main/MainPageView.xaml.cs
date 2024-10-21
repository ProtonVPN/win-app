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
using ProtonVPN.Client.Contracts.Bases;
using ProtonVPN.Client.Services.Navigation;

namespace ProtonVPN.Client.UI.Main;

public sealed partial class MainPageView : IContextAware
{
    public MainPageViewModel ViewModel { get; }

    public MainViewNavigator Navigator { get; }

    public MainPageView()
    {
        ViewModel = App.GetService<MainPageViewModel>();
        Navigator = App.GetService<MainViewNavigator>();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Navigator.Initialize(MainNavigationFrame);
        ViewModel.Activate();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
        Navigator.Reset();
    }

    private void OnSidebarPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ViewModel.OnSidebarInteractionStarted();
    }

    private void OnSidebarPointerExited(object sender, PointerRoutedEventArgs e)
    {
        ViewModel.OnSidebarInteractionEnded();
    }
}