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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Events;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Profiles;
using ProtonVPN.ViewModels;
using ProtonVPN.Windows;

namespace ProtonVPN.QuickLaunch
{
    internal class QuickLaunchViewModel :
        LanguageAwareViewModel,
        IVpnStateAware,
        IUserLocationAware,
        IConnectionDetailsAware
    {
        private readonly ProfileManager _profileManager;
        private readonly ProfileViewModelFactory _profileHelper;
        private readonly IVpnManager _vpnManager;
        private readonly AppWindow _appWindow;
        private readonly IUserStorage _userStorage;
        
        public ICommand ShowAppCommand { get; set; }
        public ICommand QuickConnectCommand { get; set; }
        public ICommand ProfileConnectCommand { get; set; }

        private VpnStatus _vpnStatus;

        private string _ip;
        public string Ip
        {
            get => _ip;
            set => Set(ref _ip, value);
        }
        
        private string _serverName;
        public string ServerName
        {
            get => _serverName;
            set => Set(ref _serverName, value);
        }
        
        private string _countryCode;
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
        
        private bool _showQuickConnectPopup;
        public bool ShowQuickConnectPopup
        {
            get => _showQuickConnectPopup;
            set => Set(ref _showQuickConnectPopup, value);
        }
        
        private IReadOnlyList<ProfileViewModel> _profiles;
        public IReadOnlyList<ProfileViewModel> Profiles
        {
            get => _profiles;
            set => Set(ref _profiles, value);
        }
        
        private ViewModel _selectedProfile;
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
            ProfileViewModelFactory profileHelper,
            IVpnManager vpnManager,
            AppWindow appWindow, 
            IUserStorage userStorage)
        {
            ShowAppCommand = new RelayCommand(ShowAppAction);
            QuickConnectCommand = new RelayCommand(QuickConnectAction);
            ProfileConnectCommand = new RelayCommand<ProfileViewModel>(ProfileConnectAction);

            _profileManager = profileManager;
            _profileHelper = profileHelper;
            _vpnManager = vpnManager;
            _appWindow = appWindow;
            _userStorage = userStorage;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            Server server = e.State.Server;

            _vpnStatus = e.State.Status;

            switch (e.State.Status)
            {
                case VpnStatus.Connected:
                    ServerName = server.Name;
                    CountryCode = server.EntryCountry;
                    SetIp(server.ExitIp);
                    Connected = true;
                    Connecting = false;
                    Disconnected = false;
                    break;
                case VpnStatus.Pinging:
                case VpnStatus.Connecting:
                case VpnStatus.Reconnecting:
                    ServerName = server.Name;
                    CountryCode = server.EntryCountry;
                    SetUserIp();
                    Connected = false;
                    Connecting = true;
                    Disconnected = false;
                    break;
                case VpnStatus.Disconnected:
                case VpnStatus.Disconnecting:
                    ServerName = "";
                    CountryCode = "";
                    SetUserIp();
                    Connected = false;
                    Connecting = false;
                    Disconnected = true;
                    break;
            }

            return Task.CompletedTask;
        }

        public async Task OnConnectionDetailsChanged(ConnectionDetails connectionDetails)
        {
            if (!connectionDetails.ServerIpAddress.IsNullOrEmpty())
            {
                SetIp(connectionDetails.ServerIpAddress);
            }
        }

        private void SetUserIp()
        {
            if (_connected)
            {
                return;
            }

            SetIp(_userStorage.GetLocation().Ip);
        }

        private void SetIp(string ip)
        {
            Ip = ip;
        }

        public async Task OnUserLocationChanged(UserLocationEventArgs e)
        {
            if (_vpnStatus == VpnStatus.Disconnected)
            {
                SetIp(e.Location.Ip);
            }
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
                await _vpnManager.QuickConnectAsync();
            }
            else
            {
                await _vpnManager.DisconnectAsync();
            }
        }

        public async void Load()
        {
            SetUserIp();
            await LoadProfiles();
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
            if (viewModel == null)
            {
                return;
            }

            Profile profile = await _profileManager.GetProfileById(viewModel.Id);
            if (profile != null)
            {
                await _vpnManager.ConnectAsync(profile);
            }
        }

        private void ShowAppAction()
        {
            _appWindow.Show();
            if (_appWindow.WindowState.Equals(WindowState.Minimized))
            {
                _appWindow.WindowState = WindowState.Normal;
            }

            _appWindow.Activate();
            if (Connecting)
            {
                _appWindow.Handle(new ToggleOverlay(true));
            }
        }
    }
}