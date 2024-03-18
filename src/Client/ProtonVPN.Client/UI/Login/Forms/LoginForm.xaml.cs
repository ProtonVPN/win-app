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
using Windows.System;

namespace ProtonVPN.Client.UI.Login.Forms;

public sealed partial class LoginForm
{
    public LoginForm()
    {
        ViewModel = App.GetService<LoginFormViewModel>();
        InitializeComponent();
        UsernameTextBox.Loaded += OnUsernameTextBoxLoaded;
    }

    private void OnUsernameTextBoxLoaded(object sender, RoutedEventArgs e)
    {
        UsernameTextBox.Focus(FocusState.Programmatic);
        UsernameTextBox.SelectAll();
    }

    public LoginFormViewModel ViewModel { get; }

    private void OnFormBoxKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && LoginButton.Command.CanExecute(null))
        {
            LoginButton.Command.Execute(null);
        }
    }
}