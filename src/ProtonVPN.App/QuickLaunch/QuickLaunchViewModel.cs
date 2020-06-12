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

using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.P2PDetection;
using ProtonVPN.Profiles;
using ProtonVPN.Vpn.Connectors;
using ProtonVPN.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ProtonVPN.ViewModels;

namespace ProtonVPN.QuickLaunch
{
    internal class QuickLaunchViewModel :
        LanguageAwareViewModel,
        IVpnStateAware,
        ITrafficForwardedAware
    {
        private ViewModel _selectedProfile;
        private string _vpnStateText;
        private VpnStatus _vpnStatus;

        private readonly ProfileManager _profileManager;
        private readonly ProfileViewModelFactory _profileHelper;
        private readonly QuickConnector _quickConnector;
        private readonly IVpnManager _vpnManager;
        private readonly AppWindow _appWindow;

        private IReadOnlyList<ProfileViewModel> _profiles;
        private bool _showQuickConnectPopup;
        private string _ip;
        private string _serverName;
        private string _countryCode;

        public ICommand ShowAppCommand { get; set; }
        public ICommand QuickConnectCommand { get; set; }
        public ICommand ProfileConnectCommand { get; set; }

        public string Ip
        {
            get => _ip;
            set => Set(ref _ip, value);
        }

        public string ServerName
        {
            get => _serverName;
            set => Set(ref _serverName, value);
        }

        public string CountryCode
        {
            get => _countryCode;
            set => Set(ref _countryCode, value);
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set => Set(ref _connected, value);
        }

        private bool _connecting;
        public bool Connecting
        {
            get => _connecting;
            set => Set(ref _connecting, value);
        }

        private bool _disconnected = true;
        public bool Disconnected
        {
            get => _disconnected;
            set => Set(ref _disconnected, value);
        }

        public bool ShowQuickConnectPopup
        {
            get => _showQuickConnectPopup;
            set => Set(ref _showQuickConnectPopup, value);
        }

        public string VpnStateText
        {
            get => _vpnStateText;
            set => Set(ref _vpnStateText, value);
        }

        public IReadOnlyList<ProfileViewModel> Profiles
        {
            get => _profiles;
            set => Set(ref _profiles, value);
        }

        public ViewModel SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                _selectedProfile = value;
                NotifyOfPropertyChange(() => SelectedProfile);
            }
        }

        public QuickLaunchViewModel(
            ProfileManager profileManager,
            QuickConnector quickConnector,
            ProfileViewModelFactory profileHelper,
            IVpnManager vpnManager,
            AppWindow appWindow)
        {
            _appWindow = appWindow;
            ShowAppCommand = new RelayCommand(ShowAppAction);
            QuickConnectCommand = new RelayCommand(QuickConnectAction);
            ProfileConnectCommand = new RelayCommand<ProfileViewModel>(ProfileConnectAction);

            _profileHelper = profileHelper;
            _profileManager = profileManager;
            _vpnManager = vpnManager;
            _quickConnector = quickConnector;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            var server = e.State.Server;

            _vpnStatus = e.State.Status;

            switch (e.State.Status)
            {
                case VpnStatus.Connected:
                    ServerName = server.Name;
                    CountryCode = server.EntryCountry;
                    Ip = server.ExitIp;
                    Connected = true;
                    Connecting = false;
                    Disconnected = false;
                    break;
                case VpnStatus.Connecting:
                case VpnStatus.Reconnecting:
                    ServerName = server.Name;
                    CountryCode = server.EntryCountry;
                    Connected = false;
                    Connecting = true;
                    Disconnected = false;
                    break;
                case VpnStatus.Disconnected:
                case VpnStatus.Disconnecting:
                    ServerName = "";
                    CountryCode = "";
                    Ip = "";
                    Connected = false;
                    Connecting = false;
                    Disconnected = true;
                    break;
            }

            return Task.CompletedTask;
        }

        public override async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            base.OnAppSettingsChanged(e);

            if (e.PropertyName.Equals(nameof(IAppSettings.Language)))
            {
                await LoadProfiles();
            }

            if (e.PropertyName.Equals(nameof(IAppSettings.Profiles))
                || e.PropertyName.Equals(nameof(IAppSettings.SecureCore)))
            {
                await LoadProfiles();
            }
        }

        private async void QuickConnectAction()
        {
            if (_vpnStatus.Equals(VpnStatus.Disconnected) ||
                _vpnStatus.Equals(VpnStatus.Disconnecting))
            {
                await _quickConnector.Connect();
            }
            else
            {
                await _quickConnector.Disconnect();
            }
        }

        public async void Load()
        {
            await LoadProfiles();
        }

        public void OnTrafficForwarded(string ip)
        {
            Ip = ip;
        }

        private async Task LoadProfiles()
        {
            Profiles = (await _profileHelper.GetProfiles())
                .OrderByDescending(p => p.IsPredefined)
                .ThenBy(p => p.Name)
                .ToList();
        }

        private async void ProfileConnectAction(ProfileViewModel viewModel)
        {
            ShowQuickConnectPopup = false;
            if (viewModel == null) return;
            var profile = await _profileManager.GetProfileById(viewModel.Id);
            if (profile == null) return;

            await _vpnManager.Connect(profile);
        }

        private void ShowAppAction()
        {
            if (!_appWindow.IsLoaded) return;

            _appWindow.Show();
            if (_appWindow.WindowState.Equals(WindowState.Minimized))
            {
                _appWindow.WindowState = WindowState.Normal;
            }

            _appWindow.Activate();
        }
    }
}
