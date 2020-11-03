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

using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Modals;

namespace ProtonVPN.BugReporting
{
    public class FailureViewModel : ViewModel
    {
        private readonly IModals _modals;

        public FailureViewModel(IModals modals)
        {
            _modals = modals;
            TroubleshootCommand = new RelayCommand(TroubleshootAction);
        }

        private string _error;

        public string Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public ICommand TroubleshootCommand { get; set; }

        private void TroubleshootAction()
        {
            _modals.Show<TroubleshootModalViewModel>();
        }
    }
}
