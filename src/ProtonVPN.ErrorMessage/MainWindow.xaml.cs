/*
 * Copyright (c) 2020 Proton Technologies AG
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

namespace ProtonVPN.ErrorMessage
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel _vm;

        internal MainWindow(MainWindowViewModel vm)
        {
            _vm = vm;
            InitializeComponent();

            DataContext = vm;
        }

        private void Repair(object sender, RoutedEventArgs e)
        {
            _vm.Repair();
        }

        private void Download(object sender, RoutedEventArgs e)
        {
            _vm.Download();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
