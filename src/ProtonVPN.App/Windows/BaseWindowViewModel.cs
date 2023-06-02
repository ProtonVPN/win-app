﻿/*
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

using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.ViewModels;

namespace ProtonVPN.Windows
{
    public abstract class BaseWindowViewModel : LanguageAwareViewModel
    {
        private WindowState _windowState;

        protected BaseWindowViewModel()
        {
            CloseCommand = new RelayCommand(CloseAction);
            MinimizeCommand = new RelayCommand(MinimizeAction);
        }

        public ICommand CloseCommand { get; set; }
        public ICommand MinimizeCommand { get; set; }

        public bool HideWindowControls { get; set; }

        public WindowState WindowState
        {
            get => _windowState;
            set => Set(ref _windowState, value);
        }

        public abstract void CloseAction();

        private void MinimizeAction()
        {
            WindowState = WindowState.Minimized;
        }
    }
}
