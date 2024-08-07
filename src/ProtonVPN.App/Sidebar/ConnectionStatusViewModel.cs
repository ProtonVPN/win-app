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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Threading;
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
using ProtonVPN.Sidebar.ChangeServer;

namespace ProtonVPN.Sidebar
{
    public class ConnectionStatusViewModel :
        ViewModel,
        IVpnStateAware,
        IOnboardingStepAware,
        IServersAware,
        IUserLocationAware,
        ISettingsAware,
        IConnectionDetailsAware,
        IUserDataAware,
        IHandle<ChangeServerTimeLeftMessage>
    {
        private readonly IAppSettings _appSettings;
        private readonly SidebarManager _sidebarManager;
        private readonly IVpnManager _vpnManager;
        private readonly ServerManager _serverManager;
        private readonly VpnConnectionSpeed _speedTracker;
        private readonly IUserStorage _userStorage;
        private readonly IModals _modals;
        private readonly ILogger _logger;
        private readonly ServerChangeManager _serverChangeManager;
        private readonly SettingsModalViewModel _settingsModalViewModel;
        private readonly EnumToDisplayTextConverter _enumToDisplayTextConverter;
        private readonly ISchedulerTimer _timer;

        private VpnStatus _vpnStatus;
        private bool _sidebarMode;

        public ConnectionStatusViewModel(
            IEventAggregator eventAggregator,
            IAppSettings appSettings,
            SidebarManager sidebarManager,
            ServerManager serverManager,
            IVpnManager vpnManager,
            VpnConnectionSpeed speedTracker,
            IUserStorage userStorage,
            IModals modals, 
            ILogger logger,
            ServerChangeManager serverChangeManager,
            SettingsModalViewModel settingsModalViewModel,
            AnnouncementsViewModel announcementsViewModel,
            PortForwardingActivePortViewModel activePortViewModel,
            IScheduler scheduler)
        {
            eventAggregator.Subscribe(this);

            _appSettings = appSettings;
            _sidebarManager = sidebarManager;
            _vpnManager = vpnManager;
            _serverManager = serverManager;
            _speedTracker = speedTracker;
            _userStorage = userStorage;
            _modals = modals;
            _logger = logger;
            _serverChangeManager = serverChangeManager;
            _settingsModalViewModel = settingsModalViewModel;
            _enumToDisplayTextConverter = new EnumToDisplayTextConverter();

            QuickConnectCommand = new RelayCommand(QuickConnectAction);
            ChangeServerCommand = new RelayCommand(ChangeServerActionAsync);
            DisableKillSwitchCommand = new RelayCommand(DisableKillSwitch);
            ToggleSidebarModeCommand = new RelayCommand(ToggleSidebarModeAction);
            CloseVpnAcceleratorReconnectionPopupCommand = new RelayCommand(CloseVpnAcceleratorReconnectionPopupAction);
            OpenNotificationSettingsCommand = new RelayCommand(OpenNotificationSettingsAction);

            Announcements = announcementsViewModel;
            ActivePortViewModel = activePortViewModel;
            ActivePortViewModel.PropertyChanged += OnActivePortViewModelPropertyChanged;

            _timer = scheduler.Timer();
            _timer.IsEnabled = false;
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnSecondPassed;
        }

        private void OnActivePortViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        public AnnouncementsViewModel Announcements { get; }
        public PortForwardingActivePortViewModel ActivePortViewModel { get; }

        public ICommand QuickConnectCommand { get; set; }
        public ICommand ChangeServerCommand { get; set; }
        public ICommand DisableKillSwitchCommand { get; set; }
        public ICommand ToggleSidebarModeCommand { get; set; }
        public ICommand CloseVpnAcceleratorReconnectionPopupCommand { get; set; }
        public ICommand OpenNotificationSettingsCommand { get; set; }

        public bool IsToShowFreeRescopeUI => _appSettings.FeatureFreeRescopeEnabled && !_userStorage.GetUser().Paid();

        private bool _killSwitchActivated;
        public bool KillSwitchActivated
        {
            get => _killSwitchActivated;
            set => Set(ref _killSwitchActivated, value);
        }

        public bool IsToShowChangeServerButton => Connected && IsToShowFreeRescopeUI;

        public string ServerExitCountry => _connectedServer?.ExitCountry;

        public int ServerLoad => _connectedServer?.Load ?? 0;

        private string _changeServerTimeLeft = string.Empty;
        public string ChangeServerTimeLeft
        {
            get => _changeServerTimeLeft;
            set => Set(ref _changeServerTimeLeft, value);
        }

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

        private bool _isB2B;
        public bool IsB2B
        {
            get => _isB2B;
            set => Set(ref _isB2B, value);
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

        public async Task HandleAsync(ChangeServerTimeLeftMessage message, CancellationToken cancellationToken)
        {
            ChangeServerTimeLeft = message.TimeLeftInSeconds > 0 ? message.TimeLeftFormatted : string.Empty;
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
            OnPropertyChanged(nameof(IsToShowChangeServerButton));

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
            switch (e.PropertyName)
            {
                case nameof(IAppSettings.SidebarMode):
                    SetSidebarMode();
                    break;
                case nameof(IAppSettings.Language):
                {
                    OnPropertyChanged(nameof(ServerLoad));
                    Server connectedServer = ConnectedServer;
                    if (connectedServer != null)
                    {
                        SetConnectionName(connectedServer);
                    }
                    break;
                }
                case nameof(IAppSettings.FeatureFreeRescopeEnabled):
                    OnPropertyChanged(nameof(IsToShowChangeServerButton));
                    OnPropertyChanged(nameof(IsToShowFreeRescopeUI));
                    break;
            }
        }

        public async Task OnConnectionDetailsChanged(ConnectionDetails connectionDetails)
        {
            if (!connectionDetails.ServerIpAddress.IsNullOrEmpty())
            {
                SetIp(connectionDetails.ServerIpAddress);
            }
        }

        public void OnUserDataChanged()
        {
            OnPropertyChanged(nameof(IsToShowChangeServerButton));
            OnPropertyChanged(nameof(IsToShowFreeRescopeUI));
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

        private async void ChangeServerActionAsync()
        {
            await _serverChangeManager.ChangeServerAsync();
        }

        private void SetConnectionName(Server server)
        {
            ConnectionName = server.CreateConnectionName();
            IsB2B = server.IsB2B();
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

        private async void OpenNotificationSettingsAction()
        {
            CloseVpnAcceleratorReconnectionPopupAction();
            _settingsModalViewModel.OpenConnectionTab();
            await _modals.ShowAsync<SettingsModalViewModel>();
        }
    }
}