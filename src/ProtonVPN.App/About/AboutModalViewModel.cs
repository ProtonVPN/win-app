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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Update;
using ProtonVPN.Modals;

namespace ProtonVPN.About
{
    public class AboutModalViewModel : BaseModalViewModel, IUpdateStateAware
    {
        private static readonly ReleaseEqualityComparer ReleaseComparer = new ReleaseEqualityComparer();

        private readonly IConfiguration _appConfig;
        private readonly UpdateService _appUpdater;
        private readonly IAppSettings _appSettings;
        private readonly IModals _modals;

        public AboutModalViewModel(
            IConfiguration appConfig,
            UpdateService appUpdater,
            IAppSettings appSettings,
            UpdateViewModel updateViewModel,
            IModals modals)
        {
            _modals = modals;
            _appConfig = appConfig;
            _appUpdater = appUpdater;
            _appSettings = appSettings;
            Update = updateViewModel;
            LicenseCommand = new RelayCommand(ShowLicense);
        }

        public ICommand LicenseCommand { get; set; }

        public bool ShowPrimaryButton => true;

        public UpdateViewModel Update { get; }

        private IReadOnlyList<Release> _releases;
        public IReadOnlyList<Release> Releases
        {
            get => _releases;
            private set
            {
                if (_releases?.SequenceEqual(value, ReleaseComparer) != true)
                {
                    Set(ref _releases, value);
                    NotifyOfPropertyChange(nameof(EarlyAccess));
                }
            }
        }

        public string AppVersion => _appConfig.AppVersion;

        public bool? EarlyAccess
        {
            get
            {
                Version appVersion = Version.Parse(AppVersion);
                Release release = Releases?.FirstOrDefault(r => appVersion == r.Version);
                return release?.EarlyAccess;
            }
        }

        private string _lastUpdate;
        public string LastUpdate
        {
            get => _lastUpdate;
            set => Set(ref _lastUpdate, value);
        }

        public void OnUpdateStateChanged(UpdateStateChangedEventArgs e)
        {
            Releases = e.ReleaseHistory;
        }

        protected override void OnActivate()
        {
            LastUpdate = _appSettings.LastUpdate;
        }

        public override void BeforeOpenModal(dynamic options)
        {
            _appUpdater.StartCheckingForUpdate();
        }

        private void ShowLicense()
        {
            _modals.Show<LicenseModalViewModel>();
        }
    }
}
