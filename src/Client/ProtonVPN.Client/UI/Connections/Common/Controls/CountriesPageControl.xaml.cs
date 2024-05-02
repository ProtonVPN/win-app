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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.UI.Connections.Bases;
using Windows.System;

namespace ProtonVPN.Client.UI.Connections.Common.Controls;

public sealed partial class CountriesPageControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(CountriesPageViewModelBase), typeof(CountriesPageControl), new PropertyMetadata(default, OnViewModelPropertyChanged));

    public CountriesPageViewModelBase ViewModel
    {
        get => (CountriesPageViewModelBase)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public CountriesPageControl()
    {
        InitializeComponent();
    }

    public void FocusSearchQueryBox()
    {
        SearchQueryBox.Focus(FocusState.Programmatic);
    }

    public void FocusSearchQueryBox(VirtualKey key)
    {
        if (char.IsLetterOrDigit((char)key) || key == VirtualKey.Back)
        {
            FocusSearchQueryBox();
        }
    }

    public async void FocusSearchQueryBoxWithDelayAsync()
    {
        await Task.Delay(200);
        FocusSearchQueryBox();
    }

    private static void OnViewModelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is CountriesPageControl control)
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

    private void OnSearchQueryChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        PageContentHost.ResetContentScroll();
    }
}