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

using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Input;

namespace ProtonVPN.Login.ViewModels
{
    public class GeneralErrorViewModel : StandardErrorViewModel
    {
        private readonly DetailedErrorViewModel _detailedErrorViewModel;
        private readonly OutdatedAppErrorViewModel _outdatedAppErrorViewModel;
        private bool _solutionsPopupVisible;

        private StandardErrorViewModel _viewModel;
        public StandardErrorViewModel ViewModel
        {
            get => _viewModel;
            set => Set(ref _viewModel, value);
        }

        public GeneralErrorViewModel(
            DetailedErrorViewModel detailedErrorViewModel,
            OutdatedAppErrorViewModel outdatedAppErrorViewModel)
        {
            _detailedErrorViewModel = detailedErrorViewModel;
            _outdatedAppErrorViewModel = outdatedAppErrorViewModel;

            ShowHelpPopupCommand = new RelayCommand(ShowHelpPopup);
            CloseHelpPopupCommand = new RelayCommand(CloseHelpPopup);
        }

        public ICommand GetHelpCommand { get; set; }
        public ICommand ShowHelpPopupCommand { get; set; }
        public ICommand CloseHelpPopupCommand { get; set; }

        public bool SolutionsPopupVisible
        {
            get => _solutionsPopupVisible;
            set => Set(ref _solutionsPopupVisible, value);
        }

        public void SetDetailedErrorMessage(string message)
        {
            _detailedErrorViewModel.SetError(message);
            ViewModel = _detailedErrorViewModel;
        }

        public void SetOutdatedErrorMessage(string message)
        {
            _outdatedAppErrorViewModel.SetError(message);
            ViewModel = _outdatedAppErrorViewModel;
        }

        public override void Reset()
        {
            base.Reset();
            CloseHelpPopup();
        }

        private void ShowHelpPopup()
        {
            SolutionsPopupVisible = true;
        }

        private void CloseHelpPopup()
        {
            SolutionsPopupVisible = false;
        }
    }
}
