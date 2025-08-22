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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Extensions;

namespace ProtonVPN.Client.UI.Main.Components;

public sealed partial class ConnectionErrorComponent
{    
    // FadeInOutAnimationDuration is 800ms + 500ms delay
    private static readonly TimeSpan _animationDuration = TimeSpan.FromMilliseconds(1300);

    public ConnectionErrorViewModel ViewModel { get; }

    public ConnectionErrorComponent()
    {
        ViewModel = App.GetService<ConnectionErrorViewModel>();
        InitializeComponent();

        ActionInfoBar.RegisterPropertyChangedCallback(InfoBar.IsOpenProperty, OnInfoBarIsOpenChangedAsync);
    }

    private async void OnInfoBarIsOpenChangedAsync(DependencyObject sender, DependencyProperty dp)
    {
        if (ActionInfoBar.IsOpen && this.IsParentWindowFocused())
        {
            await Task.Delay(_animationDuration);

            ActionInfoBar.Focus(FocusState.Programmatic);
        }
    }
}