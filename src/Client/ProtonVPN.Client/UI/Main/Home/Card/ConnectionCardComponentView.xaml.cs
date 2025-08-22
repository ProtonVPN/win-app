/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Extensions;

namespace ProtonVPN.Client.UI.Main.Home.Card;

public sealed partial class ConnectionCardComponentView : IContextAware
{
    public static readonly DependencyProperty ButtonsAlignmentProperty =
        DependencyProperty.Register(nameof(ButtonsAlignment), typeof(HorizontalAlignment), typeof(ConnectionCardComponentView), new PropertyMetadata(HorizontalAlignment.Center));

    public HorizontalAlignment ButtonsAlignment
    {
        get => (HorizontalAlignment)GetValue(ButtonsAlignmentProperty);
        set => SetValue(ButtonsAlignmentProperty, value);
    }

    public ConnectionCardComponentViewModel ViewModel { get; }

    public ConnectionCardComponentView()
    {
        ViewModel = App.GetService<ConnectionCardComponentViewModel>();

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
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
    }

    private void OnButtonIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // This code makes sure the button to connect/cancel/disconnect receives focus automatically when enabled
        if (sender is Button button && button.IsEnabled && this.IsParentWindowFocused())
        {
            button.Focus(FocusState.Programmatic);
        }
    }
}