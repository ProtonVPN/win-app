/*
 * Copyright (c) 2022 Proton Technologies AG
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ProtonVPN.Login.ViewModels;

namespace ProtonVPN.Login.Views
{
    public partial class LoginView
    {
        public LoginView()
        {
            InitializeComponent();
            HelpPopup.Opened += OnHelpPopupOpened;
        }

        private void OnHelpPopupOpened(object sender, System.EventArgs e)
        {
            HelpPopup.HorizontalOffset = -(HelpButton.ActualWidth - HelpPopup.ActualWidth) / 2;
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ImgShowHide.Visibility = sender is PasswordBox pb && pb.Password.Length > 0
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void PasswordInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            ExecuteLoginCommand();
        }

        private void UsernameInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            ExecuteLoginCommand();
        }

        private void ExecuteLoginCommand()
        {
            var context = (LoginViewModel)DataContext;
            if (context.LoginCommand.CanExecute(PasswordInput))
            {
                context.LoginCommand.Execute(PasswordInput);
            }
        }

        private void ShowPassword()
        {
            PasswordInput.Visibility = Visibility.Hidden;
            PasswordText.Text = PasswordInput.Password;
            PasswordText.Visibility = Visibility.Visible;
        }

        private void HidePassword()
        {
            PasswordInput.Visibility = Visibility.Visible;
            PasswordText.Visibility = Visibility.Hidden;
            PasswordText.Text = string.Empty;
        }

        private void ImgShowHide_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            ShowPassword();
        }

        private void ImgShowHide_MouseLeave(object sender, MouseEventArgs e)
        {
            HidePassword();
        }

        private void ImgShowHide_PreviewMouseUp(object sender, MouseEventArgs e)
        {
            HidePassword();
            PasswordInput.Focus();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            LoginInput.Focus();
        }
    }
}
