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

namespace ProtonVPN.Modals
{
    public class TroubleshootModalViewModel : BaseModalViewModel, ISettingsAware
    {
        private readonly IActiveUrls _urls;
        private readonly IAppSettings _appSettings;

        public TroubleshootModalViewModel(IActiveUrls urls, IAppSettings appSettings)
        {
            _urls = urls;
            _appSettings = appSettings;

            ProtonStatusCommand = new RelayCommand(OpenProtonStatusPage);
            ProtonTwitterCommand = new RelayCommand(OpenProtonTwitterPage);
            TorCommand = new RelayCommand(OpenTorPage);
            SupportFormCommand = new RelayCommand(OpenSupportFormPage);
            AlternativeRoutingCommand = new RelayCommand(OpenAlternativeRoutingPage);
        }

        public ICommand ProtonStatusCommand { get; }
        public ICommand ProtonTwitterCommand { get; }
        public ICommand TorCommand { get; }
        public ICommand SupportFormCommand { get; }
        public ICommand AlternativeRoutingCommand { get; }

        public bool DoHEnabled
        {
            get => _appSettings.DoHEnabled;
            set => _appSettings.DoHEnabled = value;
        }

        public override void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            base.OnAppSettingsChanged(e);

            if (e.PropertyName == nameof(IAppSettings.DoHEnabled))
            {
                DoHEnabled = _appSettings.DoHEnabled;
            }
        }

        private void OpenProtonStatusPage()
        {
            _urls.ProtonStatusUrl.Open();
        }

        private void OpenProtonTwitterPage()
        {
            _urls.ProtonTwitterUrl.Open();
        }

        private void OpenTorPage()
        {
            _urls.TorBrowserUrl.Open();
        }

        private void OpenSupportFormPage()
        {
            _urls.SupportFormUrl.Open();
        }

        private void OpenAlternativeRoutingPage()
        {
            _urls.AlternativeRoutingUrl.Open();
        }
    }
}
