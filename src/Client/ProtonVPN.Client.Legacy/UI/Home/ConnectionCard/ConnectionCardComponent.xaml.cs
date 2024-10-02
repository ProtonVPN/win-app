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

namespace ProtonVPN.Client.Legacy.UI.Home.ConnectionCard;

public sealed partial class ConnectionCardComponent
{
    public ConnectionCardViewModel ViewModel { get; }

    public ConnectionCardComponent()
    {
        ViewModel = App.GetService<ConnectionCardViewModel>();
        InitializeComponent();
    }

    private void OnButtonIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // This code makes sure the button receives to connect/cancel/disconnect receives focus automatically when enabled
        if (sender is Button button && button.IsEnabled)
        {            
            button.Focus(FocusState.Programmatic);
        }
    }
}