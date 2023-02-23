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
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Translations;

namespace ProtonVPN.Modals
{
    public class DisabledServiceModalViewModel : BaseModalViewModel
    {
        public DisabledServiceModalViewModel()
        {
            EnableServiceCommand = new RelayCommand(EnableService);
            LearnMoreCommand = new RelayCommand(LearnMore);
        }

        public ICommand EnableServiceCommand { get; }

        public ICommand LearnMoreCommand { get; }

        public string LearnMoreButtonText => LearnMoreActive
            ? Translation.Get("Dialogs_DisabledService_btn_ShowLess")
            : Translation.Get("Dialogs_DisabledService_btn_LearnMore");

        private bool _learnMoreActive;

        public bool LearnMoreActive
        {
            get => _learnMoreActive;
            set => Set(ref _learnMoreActive, value);
        }

        private void EnableService()
        {
            TryClose(true);
        }

        private void LearnMore()
        {
            LearnMoreActive = !LearnMoreActive;
            NotifyOfPropertyChange(nameof(LearnMoreButtonText));
        }
    }
}
