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
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.MVVM.Converters;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Name;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Onboarding;
using ProtonVPN.PortForwarding.ActivePorts;
using ProtonVPN.Settings;
using ProtonVPN.Sidebar.Announcements;
using ProtonVPN.Translations;

namespace ProtonVPN.Sidebar
{
    public class ConnectionStatusViewModel :
        ViewModel,
        IVpnStateAware,
        IOnboardingStepAware,
        IServersAware,
        IUserLocationAware,
        ISettingsAware,
        IServiceSettingsStateAware,
        IConnectionDetailsAware
    {
        private readonly IAppSettings _appSettings;
        private readonly SidebarManager _sidebarManager;
        private readonly IVpnManager _vpnManager;
        private readonly ServerManager _serverManager;
        private readonly VpnConnectionSpeed _speedTracker;
        private readonly IUserStorage _userStorage;
        private readonly IModals _modals;
        private readonly ILogger _logger;
        private readonly SettingsModalViewModel _settingsModalViewModel;
        private readonly EnumToDisplayTextConverter _enumToDisplayTextConverter;

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
            IModals modals, 
            ILogger logger,
            SettingsModalViewModel settingsModalViewModel,
            AnnouncementsViewModel announcementsViewModel,
            PortForwardingActivePortViewModel activePortViewModel)
        {
            _appSettings = appSettings;
            _sidebarManager = sidebarManager;
            _vpnManager = vpnManager;
            _serverManager = serverManager;
            _speedTracker = speedTracker;
            _userStorage = userStorage;
            _modals = modals;
            _logger = logger;
            _settingsModalViewModel = settingsModalViewModel;
            _enumToDisplayTextConverter = new EnumToDisplayTextConverter();

            QuickConnectCommand = new RelayCommand(QuickConnectAction);
            DisableKillSwitchCommand = new RelayCommand(DisableKillSwitch);
            ToggleSidebarModeCommand = new RelayCommand(ToggleSidebarModeAction);
            CloseVpnAcceleratorReconnectionPopupCommand = new RelayCommand(CloseVpnAcceleratorReconnectionPopupAction);
            OpenNotificationSettingsCommand = new RelayCommand(OpenNotificationSettingsAction);

            Announcements = announcementsViewModel;
            ActivePortViewModel = activePortViewModel;
            ActivePortViewModel.PropertyChanged += OnActivePortViewModelPropertyChanged;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnSecondPassed;
        }

        private void OnActivePortViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        public AnnouncementsViewModel Announcements { get; }
        public PortForwardingActivePortViewModel ActivePortViewModel { get; }

        public ICommand QuickConnectCommand { get; set; }
        public ICommand DisableKillSwitchCommand { get; set; }
        public ICommand ToggleSidebarModeCommand { get; set; }
        public ICommand CloseVpnAcceleratorReconnectionPopupCommand { get; set; }
        public ICommand OpenNotificationSettingsCommand { get; set; }

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

        private string _adapterProtocol;
        public string AdapterProtocol
        {
            get => _adapterProtocol;
            set => Set(ref _adapterProtocol, value);
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
        
        public bool HasPortForwardingValue => ActivePortViewModel.HasPortForwardingValue;

        public bool SidebarMode
        {
            get => _sidebarMode;
            set => Set(ref _sidebarMode, value);
        }
        
        public bool IsToShowVpnAcceleratorReconnectionPopup { get; set; }
        public Server FromServer { get; private set; }
        public bool IsFromServerSecureCore { get; private set; }
        public Server ToServer { get; private set; }
        public bool IsToServerSecureCore { get; private set; }

        public void ShowVpnAcceleratorReconnectionPopup(Server previousServer, Server currentServer)
        {
            IsToShowVpnAcceleratorReconnectionPopup = true;

            FromServer = previousServer;
            IsFromServerSecureCore = previousServer.IsSecureCore();

            ToServer = currentServer;
            IsToServerSecureCore = currentServer.IsSecureCore();

            OnPropertyChanged(nameof(IsToShowVpnAcceleratorReconnectionPopup));
            OnPropertyChanged(nameof(FromServer));
            OnPropertyChanged(nameof(IsFromServerSecureCore));
            OnPropertyChanged(nameof(ToServer));
            OnPropertyChanged(nameof(IsToServerSecureCore));
        }

        public void OnServersUpdated()
        {
            if (ConnectedServer != null)
            {
                SetConnectedServerByIdOrDefault(ConnectedServer.Id);
            }
        }

        private void SetConnectedServerByIdOrDefault(string serverId, Server defaultServer = null)
        {
            // Retrieve the fresh server object to display updated server load value
            Server updatedServer = _serverManager.GetServer(new ServerById(serverId));

            if (updatedServer == null)
            {
                LogNullUpdatedServer(serverId, defaultServer);
                if (defaultServer != null)
                {
                    ConnectedServer = defaultServer;
                }
            }
            else
            {
                ConnectedServer = updatedServer;
            }
        }

        private async void LogNullUpdatedServer(string serverId, Server defaultServer)
        {
            string defaultServerText = defaultServer == null ? "null" : $"[Id: '{defaultServer.Id}' Name: '{defaultServer.Name}']";
            _logger.Error<AppLog>($"The connected server does not exist, reconnecting. ServerId: '{serverId}' DefaultServer: {defaultServerText}");

            await _vpnManager.ReconnectAsync(new VpnReconnectionSettings()
            {
                IsToExcludeLastServer = true,
                IsToForceSmartReconnect = true,
            });
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            switch (_vpnStatus)
            {
                case VpnStatus.Connected:
                    SetStatusAsConnected(e);
                    break;
                case VpnStatus.Pinging:
                case VpnStatus.Connecting:
                case VpnStatus.Reconnecting:
                case VpnStatus.Disconnected:
                case VpnStatus.Disconnecting:
                    SetStatusAsNotConnected();
                    break;
            }

            SetKillSwitchActivated(e.NetworkBlocked, e.State.Status);

            return Task.CompletedTask;
        }

        private void SetStatusAsConnected(VpnStateChangedEventArgs e)
        {
            Server server = e.State.Server;
            Connected = true;
            Disconnected = false;
            if (server == null)
            {
                LogNullConnectedServer(e);
            }
            else
            {
                SetConnectedServerByIdOrDefault(server.Id, Server.Empty());
                SetIp(server.ExitIp);
                SetConnectionName(ConnectedServer);
            }
            AdapterProtocol = GetAdapterProtocol(e);
            _timer.Start();
        }

        private void SetStatusAsNotConnected()
        {
            Connected = false;
            Disconnected = true;
            ConnectedServer = null;
            ConnectionName = null;
            SetUserIp();
            _timer.Stop();
        }

        private void LogNullConnectedServer(VpnStateChangedEventArgs e)
        {
            _logger.Error<AppLog>($"The status changed to Connected but the associated Server is null. Error: '{e.Error}' " +
                                  $"NetworkBlocked: '{e.NetworkBlocked}' UnexpectedDisconnect: '{e.UnexpectedDisconnect}' " +
                                  $"[State] Status: '{e.State.Status}' EntryIp: '{e.State.EntryIp}' Label: '{e.State.Label}' " +
                                  $"NetworkAdapterType: '{e.State.NetworkAdapterType}' VpnProtocol: '{e.State.VpnProtocol}'");
        }

        private string GetAdapterProtocol(VpnStateChangedEventArgs e)
        {
            if (e.State.VpnProtocol == VpnProtocol.WireGuard)
            {
                return Translation.Get("WireGuard_lbl");
            }

            return (string)_enumToDisplayTextConverter.Convert(e.State.VpnProtocol, typeof(string), null, null);
        }

        private void SetKillSwitchActivated(bool isNetworkBlocked, VpnStatus vpnStatus)
        {
            KillSwitchActivated = isNetworkBlocked && 
                                  (vpnStatus == VpnStatus.Disconnecting ||
                                   vpnStatus == VpnStatus.Disconnected);
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

        public void OnServiceSettingsStateChanged(ServiceSettingsStateChangedEventArgs e)
        {
            SetKillSwitchActivated(e.IsNetworkBlocked, e.CurrentState.State.Status);
        }

        public async Task OnConnectionDetailsChanged(ConnectionDetails connectionDetails)
        {
            if (!connectionDetails.ServerIpAddress.IsNullOrEmpty())
            {
                SetIp(connectionDetails.ServerIpAddress);
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

            Ip = _userStorage.GetLocation().Ip;
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

        public void CloseVpnAcceleratorReconnectionPopupAction()
        {
            IsToShowVpnAcceleratorReconnectionPopup = false;
            OnPropertyChanged(nameof(IsToShowVpnAcceleratorReconnectionPopup));
        }

        private void OpenNotificationSettingsAction()
        {
            CloseVpnAcceleratorReconnectionPopupAction();
            _settingsModalViewModel.OpenConnectionTab();
            _modals.Show<SettingsModalViewModel>();
        }
    }
}