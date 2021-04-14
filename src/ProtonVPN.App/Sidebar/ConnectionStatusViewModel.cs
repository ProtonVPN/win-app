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

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Name;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Onboarding;
using ProtonVPN.P2PDetection;
using ProtonVPN.Sidebar.Announcements;

namespace ProtonVPN.Sidebar
{
    internal class ConnectionStatusViewModel :
        ViewModel,
        IVpnStateAware,
        IOnboardingStepAware,
        IServersAware,
        IUserLocationAware,
        ITrafficForwardedAware,
        ISettingsAware
    {
        private readonly IAppSettings _appSettings;
        private readonly SidebarManager _sidebarManager;
        private readonly IVpnManager _vpnManager;
        private readonly ServerManager _serverManager;
        private readonly VpnConnectionSpeed _speedTracker;
        private readonly IUserStorage _userStorage;
        private readonly DispatcherTimer _timer;
        private VpnStatus _vpnStatus;
        private bool _sidebarMode;

        public ConnectionStatusViewModel(
            IAppSettings appSettings,
            SidebarManager sidebarManager,
            ServerManager serverManager,
            IVpnManager vpnManager,
            VpnConnectionSpeed speedTracker,
            IUserStorage userStorage,
            AnnouncementsViewModel announcementsViewModel)
        {
            _appSettings = appSettings;
            _sidebarManager = sidebarManager;
            _vpnManager = vpnManager;
            _serverManager = serverManager;
            _speedTracker = speedTracker;
            _userStorage = userStorage;

            QuickConnectCommand = new RelayCommand(QuickConnectAction);
            DisableKillSwitchCommand = new RelayCommand(DisableKillSwitch);
            ToggleSidebarModeCommand = new RelayCommand(ToggleSidebarModeAction);
            Announcements = announcementsViewModel;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnSecondPassed;
        }

        public AnnouncementsViewModel Announcements { get; }

        public ICommand QuickConnectCommand { get; set; }
        public ICommand DisableKillSwitchCommand { get; set; }
        public ICommand ToggleSidebarModeCommand { get; set; }

        private bool _killSwitchActivated;
        public bool KillSwitchActivated
        {
            get => _killSwitchActivated;
            set => Set(ref _killSwitchActivated, value);
        }

        public string ServerExitCountry => _connectedServer?.ExitCountry;

        public int ServerLoad => _connectedServer?.Load ?? 0;

        private string _ip;
        public string Ip
        {
            get => _ip;
            set => Set(ref _ip, value);
        }

        private bool _showFirstOnboardingStep;
        public bool ShowFirstOnboardingStep
        {
            get => _showFirstOnboardingStep;
            set => Set(ref _showFirstOnboardingStep, value);
        }

        private VpnProtocol _protocol;
        public VpnProtocol Protocol
        {
            get => _protocol;
            set => Set(ref _protocol, value);
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set => Set(ref _connected, value);
        }

        private bool _disconnected = true;
        public bool Disconnected
        {
            get => _disconnected;
            set => Set(ref _disconnected, value);
        }

        private IName _connectionName;
        public IName ConnectionName
        {
            get => _connectionName;
            set => Set(ref _connectionName, value);
        }

        private double _currentDownloadSpeed;
        public double CurrentDownloadSpeed
        {
            get => _currentDownloadSpeed;
            set => Set(ref _currentDownloadSpeed, value);
        }

        private double _currentUploadSpeed;
        public double CurrentUploadSpeed
        {
            get => _currentUploadSpeed;
            set => Set(ref _currentUploadSpeed, value);
        }

        public bool SidebarMode
        {
            get => _sidebarMode;
            set => Set(ref _sidebarMode, value);
        }

        public void OnServersUpdated()
        {
            if (ConnectedServer != null)
            {
                // Retrieve the fresh server object to display updated server load value
                ConnectedServer = _serverManager.GetServer(new ServerById(ConnectedServer.Id)) ?? ConnectedServer;
            }
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            switch (_vpnStatus)
            {
                case VpnStatus.Connected:
                    var server = e.State.Server;
                    if (server != null)
                    {
                        Connected = true;
                        Disconnected = false;
                        // Retrieve the fresh server object to display updated server load value
                        ConnectedServer = _serverManager.GetServer(new ServerById(server.Id)) ?? Server.Empty();
                        SetIp(server.ExitIp);
                        SetConnectionName(ConnectedServer);
                        Protocol = e.Protocol;
                        _timer.Start();
                    }
                    break;
                case VpnStatus.Connecting:
                case VpnStatus.Reconnecting:
                case VpnStatus.Disconnected:
                case VpnStatus.Disconnecting:
                    Connected = false;
                    Disconnected = true;
                    ConnectedServer = null;
                    ConnectionName = null;
                    SetUserIp();
                    _timer.Stop();
                    break;
            }

            KillSwitchActivated = e.NetworkBlocked && 
                                  (e.State.Status == VpnStatus.Disconnecting ||
                                   e.State.Status == VpnStatus.Disconnected);

            return Task.CompletedTask;
        }

        public Task OnUserLocationChanged(UserLocationEventArgs e)
        {
            if (_vpnStatus != VpnStatus.Disconnected)
            {
                return Task.CompletedTask;
            }

            SetIp(e.Location.Ip);

            return Task.CompletedTask;
        }

        public void OnTrafficForwarded(string ip)
        {
            SetIp(ip);
        }

        public void Load()
        {
            SetUserIp();
            SetSidebarMode();
        }

        public void OnStepChanged(int step)
        {
            ShowFirstOnboardingStep = step == 1;
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAppSettings.SidebarMode))
            {
                SetSidebarMode();
            }

            if (e.PropertyName == nameof(IAppSettings.Language))
            {
                OnPropertyChanged(nameof(ServerLoad));
                Server connectedServer = ConnectedServer;
                if (connectedServer != null)
                {
                    SetConnectionName(connectedServer);
                }
            }
        }

        private Server _connectedServer;
        private Server ConnectedServer
        {
            get => _connectedServer;
            set
            {
                _connectedServer = value;
                OnPropertyChanged(nameof(ServerExitCountry));
                OnPropertyChanged(nameof(ServerLoad));
            }
        }

        private void OnSecondPassed(object sender, EventArgs e)
        {
            VpnSpeed speed = _speedTracker.Speed();
            CurrentDownloadSpeed = speed.DownloadSpeed;
            CurrentUploadSpeed = speed.UploadSpeed;
        }

        private void SetUserIp()
        {
            if (_connected)
            {
                return;
            }

            Ip = _userStorage.Location().Ip;
        }

        private void SetIp(string ip)
        {
            Ip = ip;
        }

        private async void QuickConnectAction()
        {
            if (_vpnStatus == VpnStatus.Disconnecting ||
                _vpnStatus == VpnStatus.Disconnected)
            {
                await _vpnManager.QuickConnectAsync();
            }
            else
            {
                await _vpnManager.DisconnectAsync();
            }
        }

        private void SetConnectionName(Server server)
        {
            if (server.IsSecureCore())
            {
                ConnectionName = server.GetServerName();
            }
            else
            {
                ConnectionName = new StandardServerName
                {
                    EntryCountryCode = server.EntryCountry,
                    Name = server.Name
                };
            }
        }

        private void DisableKillSwitch()
        {
            _appSettings.KillSwitchMode = KillSwitchMode.Off;
        }

        private void ToggleSidebarModeAction()
        {
            SidebarMode = !SidebarMode;
            _sidebarManager.ToggleSidebar();
        }

        private void SetSidebarMode()
        {
            _sidebarMode = _appSettings.SidebarMode;
            OnPropertyChanged(nameof(SidebarMode));
        }
    }
}