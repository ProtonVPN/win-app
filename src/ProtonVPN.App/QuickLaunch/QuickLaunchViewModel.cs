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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Name;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Profiles;
using ProtonVPN.Sidebar.ChangeServer;
using ProtonVPN.ViewModels;
using ProtonVPN.Windows;

namespace ProtonVPN.QuickLaunch
{
    internal class QuickLaunchViewModel :
        LanguageAwareViewModel,
        IVpnStateAware,
        IUserLocationAware,
        IConnectionDetailsAware,
        IVpnPlanAware,
        IHandle<ChangeServerTimeLeftMessage>
    {
        private readonly ProfileManager _profileManager;
        private readonly ProfileViewModelFactory _profileHelper;
        private readonly IVpnManager _vpnManager;
        private readonly AppWindow _appWindow;
        private readonly IAppSettings _appSettings;
        private readonly ServerChangeManager _serverChangeManager;
        private readonly IUserStorage _userStorage;
        
        public ICommand ShowAppCommand { get; set; }
        public ICommand QuickConnectCommand { get; set; }
        public ICommand ProfileConnectCommand { get; set; }
        public ICommand ChangeServerCommand { get; set; }

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

        private IName _connectionName;
        public IName ConnectionName
        {
            get => _connectionName;
            set => Set(ref _connectionName, value);
        }

        private bool _isB2B;
        public bool IsB2B
        {
            get => _isB2B;
            set => Set(ref _isB2B, value);
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

        private string _changeServerTimeLeft = string.Empty;
        public string ChangeServerTimeLeft
        {
            get => _changeServerTimeLeft;
            set => Set(ref _changeServerTimeLeft, value);
        }

        public bool IsToShowProfileSelection =>
            !_appSettings.FeatureFreeRescopeEnabled || _userStorage.GetUser().Paid();

        public bool IsToShowChangeServerButton => Connected && !IsToShowProfileSelection;

        public QuickLaunchViewModel(
            IEventAggregator eventAggregator,
            ProfileManager profileManager,
            ProfileViewModelFactory profileHelper,
            IVpnManager vpnManager,
            AppWindow appWindow,
            IAppSettings appSettings,
            ServerChangeManager serverChangeManager,
            IUserStorage userStorage)
        {
            eventAggregator.Subscribe(this);

            ShowAppCommand = new RelayCommand(ShowAppAction);
            QuickConnectCommand = new RelayCommand(QuickConnectAction);
            ProfileConnectCommand = new RelayCommand<ProfileViewModel>(ProfileConnectAction);
            ChangeServerCommand = new RelayCommand(ChangeServerActionAsync);

            _profileManager = profileManager;
            _profileHelper = profileHelper;
            _vpnManager = vpnManager;
            _appWindow = appWindow;
            _appSettings = appSettings;
            _serverChangeManager = serverChangeManager;
            _userStorage = userStorage;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            Server server = e.State.Server;

            _vpnStatus = e.State.Status;

            switch (e.State.Status)
            {
                case VpnStatus.Connected:
                    SetConnectionName(server);
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
                    SetConnectionName(server);
                    ServerName = server.Name;
                    CountryCode = server.EntryCountry;
                    SetUserIp();
                    Connected = false;
                    Connecting = true;
                    Disconnected = false;
                    break;
                case VpnStatus.Disconnected:
                case VpnStatus.Disconnecting:
                    SetConnectionName(null);
                    ServerName = "";
                    CountryCode = "";
                    SetUserIp();
                    Connected = false;
                    Connecting = false;
                    Disconnected = true;
                    break;
            }

            NotifyOfPropertyChange(nameof(IsToShowChangeServerButton));

            return Task.CompletedTask;
        }

        public async Task HandleAsync(ChangeServerTimeLeftMessage message, CancellationToken cancellationToken)
        {
            ChangeServerTimeLeft = message.TimeLeftInSeconds > 0 ? message.TimeLeftFormatted : string.Empty;
        }

        private void SetConnectionName(Server server)
        {
            if (server is null)
            {
                ConnectionName = null;
                IsB2B = false;
                return;
            }

            ConnectionName = server.CreateConnectionName();
            IsB2B = server.IsB2B();
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

            switch (e.PropertyName)
            {
                case nameof(IAppSettings.Language):
                case nameof(IAppSettings.Profiles):
                case nameof(IAppSettings.SecureCore):
                    await LoadProfiles();
                    break;
                case nameof(IAppSettings.FeatureFreeRescopeEnabled):
                    NotifyOfPropertyChange(nameof(IsToShowProfileSelection));
                    break;
            }
        }

        public async Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(IsToShowProfileSelection));
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

        private async void ShowAppAction()
        {
            await _appWindow.OpenWindowAsync();
        }

        private async void ChangeServerActionAsync()
        {
            await _serverChangeManager.ChangeServerAsync();
        }
    }
}