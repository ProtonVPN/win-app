/*
 * Copyright (c) 2021 Proton Technologies AG
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
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Announcements;
using ProtonVPN.Modals.Dialogs;

namespace ProtonVPN.Modals.Offers
{
    public class OfferModalViewModel : QuestionModalViewModel
    {
        private readonly IOsProcesses _processes;

        private Panel _panel;
        public Panel Panel
        {
            get => _panel;
            set
            {
                Set(ref _panel, value);
                OnPanelChange(value);
            }
        }

        private ActiveUrl _buttonUrl;

        public ICommand ButtonCommand { get; set; }

        public OfferModalViewModel(IOsProcesses processes)
        {
            _processes = processes;

            ButtonCommand = new RelayCommand(ButtonAction);
        }

        private void OnPanelChange(Panel value)
        {
            if (!string.IsNullOrEmpty(value?.Button?.Url))
            {
                _buttonUrl = new ActiveUrl(_processes, value.Button.Url)
                    .WithQueryParams(new() { { "utm_source", "windowsvpn" } });
            }
        }

        protected virtual void ButtonAction()
        {
            if (_buttonUrl != null)
            {
                _buttonUrl.Open();
                TryClose();
            }
        }
    }
}
