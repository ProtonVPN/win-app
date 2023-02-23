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

using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace ProtonVPN.Account
{
    public partial class AccountModalView
    {
        private const int HEIGHT_OFFSET = 100;

        public AccountModalView()
        {
            InitializeComponent();
            Loaded += AccountModalView_Loaded;
        }

        private void AccountModalView_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper window = new WindowInteropHelper(this);
            IntPtr hWnd = window.Handle;
            Screen screen = Screen.FromHandle(hWnd);
            MaxHeight = screen.WorkingArea.Height - HEIGHT_OFFSET;
        }
    }
}