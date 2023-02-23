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
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Settings;
using ProtonVPN.Modals.Dialogs;

namespace ProtonVPN.Modals.Protocols
{
    public class EnableSmartProtocolModalViewModel : QuestionModalViewModel
    {
        protected readonly IActiveUrls Urls;
        private readonly IAppSettings _appSettings;

        private bool? _isToNotShowThisMessageAgain;

        public EnableSmartProtocolModalViewModel(IActiveUrls urls, IAppSettings appSettings)
        {
            Urls = urls;
            _appSettings = appSettings;

            ReadAboutSmartProtocolCommand = new RelayCommand(ReadAboutSmartProtocolAction);
        }

        public ICommand ReadAboutSmartProtocolCommand { get; }
        
        public bool IsToNotShowThisMessageAgain
        {
            get => GetIsToNotShowThisMessageAgain();
            set => Set(ref _isToNotShowThisMessageAgain, value);
        }

        private bool GetIsToNotShowThisMessageAgain()
        {
            _isToNotShowThisMessageAgain ??= _appSettings.DoNotShowEnableSmartProtocolDialog;
            return _isToNotShowThisMessageAgain.Value;
        }

        private void ReadAboutSmartProtocolAction()
        {
            Urls.AboutSmartProtocolUrl.Open();
        }

        public override void CloseAction()
        {
            SaveDoNotShowEnableSmartProtocolDialogSetting();
            _isToNotShowThisMessageAgain = null;
            TryClose(false);
        }

        private void SaveDoNotShowEnableSmartProtocolDialogSetting()
        {
            if (IsToNotShowThisMessageAgain)
            {
                _appSettings.DoNotShowEnableSmartProtocolDialog = true;
            }
        }

        protected override void ContinueAction()
        {
            SaveDoNotShowEnableSmartProtocolDialogSetting();
            _isToNotShowThisMessageAgain = null;
            TryClose(true);
        }
        
        public override void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            base.OnAppSettingsChanged(e);
            if (e.PropertyName.Equals(nameof(IAppSettings.OvpnProtocol)))
            {
                _appSettings.DoNotShowEnableSmartProtocolDialog = false;
            }
        }
    }
}
