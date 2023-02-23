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

using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Modals
{
    public class KillSwitchConfirmationModalViewModel : BaseModalViewModel
    {
        private readonly IAppSettings _appSettings;

        private bool _isToNotShowThisMessageAgain;

        public KillSwitchConfirmationModalViewModel(IAppSettings appSettings)
        {
            _appSettings = appSettings;

            CancelCommand = new RelayCommand(CancelAction);
            EnableCommand = new RelayCommand(EnableAction);
        }

        public ICommand CancelCommand { get; }
        public ICommand EnableCommand { get; }

        public bool IsToNotShowThisMessageAgain
        {
            get => _isToNotShowThisMessageAgain;
            set => Set(ref _isToNotShowThisMessageAgain, value);
        }

        public override void CloseAction()
        {
            CancelAction();
        }

        public override void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            base.OnAppSettingsChanged(e);

            if (e.PropertyName == nameof(IAppSettings.DoNotShowKillSwitchConfirmationDialog))
            {
                IsToNotShowThisMessageAgain = _appSettings.DoNotShowKillSwitchConfirmationDialog;
            }
        }

        private void CancelAction()
        {
            _isToNotShowThisMessageAgain = false;
            TryClose(false);
        }

        private void EnableAction()
        {
            if (_isToNotShowThisMessageAgain)
            {
                _appSettings.DoNotShowKillSwitchConfirmationDialog = true;
            }

            TryClose(true);
        }
    }
}