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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ProtonVPN.ErrorMessage
{
    internal class BaseWindowViewModel : INotifyPropertyChanged
    {
        public BaseWindowViewModel()
        {
            MinimizeCommand = new GenericCommand(MinimizeAction);
            CloseCommand = new GenericCommand(CloseAction);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand MinimizeCommand { get; }

        public ICommand CloseCommand { get; }

        private WindowState _windowState;
        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                _windowState = value;
                OnPropertyChanged();
            }
        }

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void MinimizeAction()
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseAction()
        {
            Application.Current.Shutdown();
        }
    }
}
