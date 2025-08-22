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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.UI.Controls.Custom;
using ProtonVPN.Client.Common.UI.Extensions;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Controls;

public sealed partial class ConnectionItemsControl
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(ConnectionItemsControl), new PropertyMetadata(default));

    public static readonly DependencyProperty ScrollBarVisibilityProperty =
        DependencyProperty.Register(nameof(ScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ConnectionItemsControl), new PropertyMetadata(ScrollBarVisibility.Auto));

    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ScrollBarVisibility ScrollBarVisibility
    {
        get => (ScrollBarVisibility)GetValue(ScrollBarVisibilityProperty);
        set => SetValue(ScrollBarVisibilityProperty, value);
    }

    public ConnectionItemsControl()
    {
        InitializeComponent();
        Loaded += OnConnectionItemsControlLoaded;
    }

    private void OnConnectionItemsControlLoaded(object sender, RoutedEventArgs e)
    {
        WireUpButtonClickEvents();
    }

    private void WireUpButtonClickEvents()
    {
        IEnumerable<ServerConnectionRowButton> buttons = ConnectionItemsList.FindChildrenOfType<ServerConnectionRowButton>();
        foreach (ServerConnectionRowButton button in buttons)
        {
            button.Click -= OnServerConnectionRowButtonClick;
            button.Click += OnServerConnectionRowButtonClick;
        }
    }

    private void OnServerConnectionRowButtonClick(object sender, RoutedEventArgs e)
    {
        FindAndCloseDualConnectionRowButtonFlyouts(XamlRoot.Content);
    }

    private void FindAndCloseDualConnectionRowButtonFlyouts(DependencyObject parent)
    {
        IEnumerable<DualConnectionRowButton> dualButtons = parent.FindChildrenOfType<DualConnectionRowButton>();
        
        foreach (DualConnectionRowButton dualButton in dualButtons)
        {
            FlyoutBase flyout = dualButton.SecondaryCommandFlyout;
            if (flyout != null && flyout.IsOpen)
            {
                flyout.Hide();
                return;
            }
        }
    }
           
    private void OnMenuFlyoutClosing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
    {
        Focus(FocusState.Programmatic);
    }
        
    public void ResetContentScroll()
    {
        if (ConnectionItemsList?.Items.Count > 0)
        {
            ConnectionItemsList.ScrollIntoView(ConnectionItemsList.Items[0]);
        }
    }

    public async Task ScrollToItemAsync(object item)
    {
        await Task.Delay(300);

        ConnectionItemsList.ScrollIntoView(item);
    }
}