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
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.Legacy.UI.Connections.Bases;

namespace ProtonVPN.Client.Legacy.UI.Connections.Common.Controls;

public sealed partial class CountryPageControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(CountryPageViewModelBase), typeof(CountryPageControl), new PropertyMetadata(default, OnViewModelPropertyChanged));

    public CountryPageViewModelBase ViewModel
    {
        get => (CountryPageViewModelBase)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public CountryPageControl()
    {
        InitializeComponent();
    }

    private static void OnViewModelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is CountryPageControl control)
        {
            if (e.OldValue is PageViewModelBase oldViewModel)
            {
                oldViewModel.ResetContentScrollRequested -= control.OnResetContentScrollRequested;
            }

            if (e.NewValue is PageViewModelBase newViewModel)
            {
                newViewModel.ResetContentScrollRequested += control.OnResetContentScrollRequested;
            }
        }
    }

    private void OnResetContentScrollRequested(object? sender, EventArgs e)
    {
        PageContentHost.ResetContentScroll();
    }
}