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
using ProtonVPN.Core.Update;
using System.Windows.Input;

namespace ProtonVPN.Login.ViewModels
{
    public class OutdatedAppErrorViewModel : StandardErrorViewModel, IUpdateStateAware
    {
        private readonly UpdateService _updateService;

        private bool _updateRequested;
        private bool _checking;

        public bool Checking
        {
            get => _checking;
            set => Set(ref _checking, value);
        }

        public OutdatedAppErrorViewModel(UpdateService updateService)
        {
            _updateService = updateService;
            UpdateCommand = new RelayCommand(UpdateAction);
        }

        public ICommand UpdateCommand { get; set; }

        private void UpdateAction()
        {
            _updateRequested = true;
            _updateService.StartCheckingForUpdate();
        }

        public void OnUpdateStateChanged(UpdateStateChangedEventArgs e)
        {
            Checking = e.Status == UpdateStatus.Checking || e.Status == UpdateStatus.Downloading;

            if (e.Ready && _updateRequested)
            {
                _updateRequested = false;
                _updateService.Update(false);
            }
        }
    }
}
