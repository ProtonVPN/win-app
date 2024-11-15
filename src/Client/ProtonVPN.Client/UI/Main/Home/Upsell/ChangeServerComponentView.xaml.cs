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

using Microsoft.UI.Xaml;
using ProtonVPN.Client.Core.Bases;

namespace ProtonVPN.Client.UI.Main.Home.Upsell;

public sealed partial class ChangeServerComponentView : IContextAware
{
    private long? _callbackToken;

    public ChangeServerComponentViewModel ViewModel { get; }

    public ChangeServerComponentView()
    {
        ViewModel = App.GetService<ChangeServerComponentViewModel>();

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
        ViewModel.Activate();

        _callbackToken = ConnectionCardChangeServerTimeoutButton.RegisterPropertyChangedCallback(UIElement.VisibilityProperty, OnTimeoutButtonVisibilityChanged);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();

        HideChangeServerFlyout();

        if (_callbackToken.HasValue)
        {
            ConnectionCardChangeServerTimeoutButton.UnregisterPropertyChangedCallback(UIElement.VisibilityProperty, _callbackToken.Value);
        }
    }

    private void OnTimeoutButtonVisibilityChanged(DependencyObject sender, DependencyProperty dp)
    {
        if (ConnectionCardChangeServerTimeoutButton.Visibility == Visibility.Collapsed)
        {
            HideChangeServerFlyout();
        }
    }

    private void HideChangeServerFlyout()
    {
        if (ConnectionCardChangeServerTimeoutButton?.Flyout?.IsOpen ?? false)
        {
            ConnectionCardChangeServerTimeoutButton.Flyout.Hide();
        }
    }
}