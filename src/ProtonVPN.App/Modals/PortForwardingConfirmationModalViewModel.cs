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
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Modals
{
    public class PortForwardingConfirmationModalViewModel : BaseModalViewModel
    {
        private readonly IActiveUrls _urls;
        private readonly IAppSettings _appSettings;

        private bool _isToNotShowThisMessageAgain;

        public PortForwardingConfirmationModalViewModel(IActiveUrls urls, IAppSettings appSettings)
        {
            _urls = urls;
            _appSettings = appSettings;

            IsToNotShowThisMessageAgain = _appSettings.DoNotShowPortForwardingConfirmationDialog;

            CancelCommand = new RelayCommand(CancelAction);
            EnableCommand = new RelayCommand(EnableAction);
            ReadPortForwardingRisksCommand = new RelayCommand(ReadPortForwardingRisksAction);
        }

        public ICommand CancelCommand { get; }
        public ICommand EnableCommand { get; }
        public ICommand ReadPortForwardingRisksCommand { get; }

        public bool IsToNotShowThisMessageAgain
        {
            get => _isToNotShowThisMessageAgain;
            set => Set(ref _isToNotShowThisMessageAgain, value);
        }

        public override void CloseAction()
        {
            CancelAction();
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
                _appSettings.DoNotShowPortForwardingConfirmationDialog = true;
            }

            TryClose(true);
        }

        private void ReadPortForwardingRisksAction()
        {
            _urls.PortForwardingRisksUrl.Open();
        }
    }
}