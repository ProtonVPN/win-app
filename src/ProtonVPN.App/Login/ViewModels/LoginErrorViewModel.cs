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

using ProtonVPN.Core.MVVM;

namespace ProtonVPN.Login.ViewModels
{
    public class LoginErrorViewModel : ViewModel
    {
        private readonly StandardErrorViewModel _standardErrorViewModel;
        private readonly GeneralErrorViewModel _generalErrorViewModel;

        public LoginErrorViewModel(StandardErrorViewModel standardErrorViewModel, GeneralErrorViewModel generalErrorViewModel)
        {
            _standardErrorViewModel = standardErrorViewModel;
            _generalErrorViewModel = generalErrorViewModel;
        }

        private StandardErrorViewModel _viewModel;
        public StandardErrorViewModel ViewModel
        {
            get => _viewModel;
            set => Set(ref _viewModel, value);
        }

        public void SetStandardError(string message)
        {
            _standardErrorViewModel.SetError(message);
            ViewModel = _standardErrorViewModel;
        }

        public void SetDetailedError(string message)
        {
            _generalErrorViewModel.SetDetailedErrorMessage(message);
            ViewModel = _generalErrorViewModel;
        }

        public void SetOutdatedError(string message)
        {
            _generalErrorViewModel.SetOutdatedErrorMessage(message);
            ViewModel = _generalErrorViewModel;
        }

        public void ClearError()
        {
            ViewModel?.Reset();
            _viewModel = null;
        }
    }
}
