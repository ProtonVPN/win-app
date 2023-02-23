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
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.BugReporting.Actions;
using ProtonVPN.Translations;

namespace ProtonVPN.BugReporting.Screens
{
    public class SentViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;
        private string _email;

        public SentViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            FinishReportCommand = new RelayCommand(FinishReportAction);
        }

        public ICommand FinishReportCommand { get; }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                NotifyOfPropertyChange(SuccessMessage);
            }
        }

        public string SuccessMessage => Translation.Format("BugReport_lbl_WillGetBack", Email);

        private void FinishReportAction()
        {
            _eventAggregator.PublishOnUIThread(new FinishReportAction());
        }
    }
}