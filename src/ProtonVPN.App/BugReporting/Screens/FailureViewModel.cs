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

using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.BugReporting.Actions;
using ProtonVPN.Core.Modals;
using ProtonVPN.Modals;

namespace ProtonVPN.BugReporting.Screens
{
    public class FailureViewModel : Screen
    {
        private string _error;
        private readonly IModals _modals;
        private readonly IEventAggregator _eventAggregator;

        public FailureViewModel(IModals modals, IEventAggregator eventAggregator)
        {
            _modals = modals;
            _eventAggregator = eventAggregator;
            TroubleshootCommand = new RelayCommand(TroubleshootAction);
            RetryCommand = new RelayCommand(RetryAction);
            BackCommand = new RelayCommand(BackAction);
        }

        public string Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public ICommand TroubleshootCommand { get; set; }
        public ICommand RetryCommand { get; set; }
        public ICommand BackCommand { get; set; }

        private void TroubleshootAction()
        {
            _modals.Show<TroubleshootModalViewModel>();
        }

        private void RetryAction()
        {
            _eventAggregator.PublishOnUIThread(new RetryAction());
        }

        private void BackAction()
        {
            _eventAggregator.PublishOnUIThread(new GoBackAfterFailureAction());
        }
    }
}