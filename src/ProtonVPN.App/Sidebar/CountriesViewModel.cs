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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals.Upsell;
using ProtonVPN.Servers;
using ProtonVPN.Sidebar.Announcements;
using ProtonVPN.Sidebar.QuickSettings;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.Sidebar
{
    internal class CountriesViewModel :
        Screen,
        IVpnPlanAware,
        IVpnStateAware,
        ISettingsAware,
        ILogoutAware,
        IServersAware,
        IAnnouncementsAware
    {
        private readonly IAppSettings _appSettings;
        private readonly IVpnManager _vpnManager;
        private readonly ServerListFactory _serverListFactory;
        private readonly IScheduler _scheduler;
        private readonly ServerConnector _serverConnector;
        private readonly CountryConnector _countryConnector;
        private readonly GatewayConnector _gatewayConnector;
        private readonly IModals _modals;
        private readonly IUserStorage _userStorage;
        private VpnStateChangedEventArgs _vpnState = new(new VpnState(VpnStatus.Disconnected), VpnError.None, false);

        public CountriesViewModel(
            IAppSettings appSettings,
            IVpnManager vpnManager,
            ServerListFactory serverListFactory,
            IScheduler scheduler,
            ServerConnector serverConnector,
            CountryConnector countryConnector,
            GatewayConnector gatewayConnector,
            IModals modals,
            IUserStorage userStorage,
            QuickSettingsViewModel quickSettingsViewModel)
        {
            _appSettings = appSettings;
            _vpnManager = vpnManager;
            _serverListFactory = serverListFactory;
            _scheduler = scheduler;
            _serverConnector = serverConnector;
            _countryConnector = countryConnector;
            _modals = modals;
            _userStorage = userStorage;
            _gatewayConnector = gatewayConnector;
            QuickSettingsViewModel = quickSettingsViewModel;

            Connect = new RelayCommand<ServerItemViewModel>(ConnectAction);
            Upgrade = new RelayCommand<ServerItemViewModel>(UpgradeActionAsync);
            ShowFreeConnectionsModalCommand = new RelayCommand(ShowFreeConnectionsModalActionAsync);
            ConnectFastest = new RelayCommand(ConnectFastestActionAsync);
            ConnectCountry = new RelayCommand<IServerCollection>(ConnectCountryActionAsync);
            Expand = new RelayCommand<IServerCollection>(ExpandAction);
            ClearSearchCommand = new RelayCommand(ClearSearchAction);
        }

        public QuickSettingsViewModel QuickSettingsViewModel { get; }

        public ICommand Connect { get; }
        public ICommand Upgrade { get; }
        public ICommand ShowFreeConnectionsModalCommand { get; }
        public ICommand ConnectFastest { get; }
        public ICommand ConnectCountry { get; }
        public ICommand Expand { get; set; }
        public ICommand ClearSearchCommand { get; }

        private bool _searchNotEmpty;

        public bool SearchNotEmpty
        {
            get => _searchNotEmpty;
            set => Set(ref _searchNotEmpty, value);
        }

        private string _searchValue;
        public string SearchValue
        {
            get => _searchValue;
            set
            {
                if (Set(ref _searchValue, value))
                {
                    CreateList();
                    SearchNotEmpty = !string.IsNullOrEmpty(_searchValue);
                }
            }
        }

        private bool _secureCore;
        public bool SecureCore
        {
            get => _secureCore;
            set { }
        }

        private bool _portForwarding;
        public bool PortForwarding
        {
            get => _portForwarding;
            set { }
        }

        private ObservableCollection<IServerListItem> _items = new();
        public ObservableCollection<IServerListItem> Items
        {
            get => _items;
            set => Set(ref _items, value);
        }

        public void ExpandCollection(IServerCollection serverCollection)
        {
            if (serverCollection.Expanded)
            {
                return;
            }

            serverCollection.Expanded = true;

            int index = _items.IndexOf(serverCollection) + 1;

            ObservableCollection<IServerListItem> collection =
                new ObservableCollection<IServerListItem>(serverCollection.Servers.Reverse());
            foreach (IServerListItem serverListItem in collection)
            {
                _items.Insert(index, serverListItem);
            }
        }

        public void CollapseCollection(IServerCollection serverCollection)
        {
            if (!serverCollection.Expanded)
            {
                return;
            }
            serverCollection.Expanded = false;

            foreach (IServerListItem item in serverCollection.Servers)
            {
                _items.Remove(item);
            }
        }

        public void Load()
        {
            SetSecureCore();
            SetPortForwarding();
            CreateList();
        }

        public void OnServersUpdated()
        {
            CreateList();
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IAppSettings.SecureCore):
                    SetSecureCore();
                    CreateList();
                    break;

                case nameof(IAppSettings.PortForwardingEnabled):
                case nameof(IAppSettings.FeaturePortForwardingEnabled):
                    SetPortForwarding();
                    CreateList();
                    break;

                case nameof(IAppSettings.Language):
                    CreateList();
                    break;

                case nameof(IAppSettings.FeatureFreeRescopeEnabled):
                    CreateList();
                    break;
            }
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnState = e;

            if (!(e.State.Server is Server server) || (_appSettings.FeatureFreeRescopeEnabled && !_userStorage.GetUser().Paid()))
            {
                return;
            }

            if (e.State.Status == VpnStatus.Connected)
            {
                if (server.IsSecureCore())
                {
                    ExpandScCountry(server.ExitCountry);
                    UpdateVpnState(e.State);
                }
                else
                {
                    ExpandCountry(server);
                    UpdateVpnState(e.State);
                }
            }
            else
            {
                UpdateVpnState(e.State);
            }
        }

        public async Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            CreateList();
        }

        public void OnAnnouncementsChanged()
        {
            CreateList();
        }

        public void OnUserLoggedOut()
        {
            _searchValue = string.Empty;
            NotifyOfPropertyChange(nameof(SearchValue));
        }

        private void SetSecureCore()
        {
            Set(ref _secureCore, _appSettings.SecureCore, nameof(SecureCore));
        }

        private void SetPortForwarding()
        {
            Set(ref _portForwarding, _appSettings.IsPortForwardingEnabled(), nameof(PortForwarding));
        }

        private void CreateList()
        {
            _scheduler.Schedule(() =>
            {
                if (SecureCore)
                {
                    Items = _serverListFactory.BuildSecureCoreList();
                }
                else if (PortForwarding)
                {
                    Items = _serverListFactory.BuildPortForwardingList(SearchValue);
                }
                else
                {
                    Items = _serverListFactory.BuildServerList(SearchValue);
                }

                foreach (IServerListItem item in Items.ToList())
                {
                    if (item is ServersByCountryViewModel serversByCountry && serversByCountry.Expanded)
                    {
                        serversByCountry.Expanded = false;
                        ExpandCollection(serversByCountry);
                    }
                }

                if (_vpnState != null)
                {
                    OnVpnStateChanged(_vpnState);
                }
            });
        }

        private void UpdateVpnState(VpnState state)
        {
            if (Items == null)
            {
                return;
            }

            foreach (IServerListItem item in Items.ToList())
            {
                if (item is ServersByCountryViewModel or ServersByExitNodeViewModel or CountrySeparatorViewModel or CountryB2BSeparatorViewModel)
                {
                    item.OnVpnStateChanged(state);
                }
            }
        }

        private void ExpandCountry(Server server)
        {
            ServersByCountryViewModel countryRow = Items.OfType<ServersByCountryViewModel>()
                .FirstOrDefault(c => c.CountryCode.Equals(server.EntryCountry) && c.IsB2B == server.IsB2B());
            if (countryRow != null)
            {
                ExpandCollection(countryRow);
            }
        }

        private void ExpandScCountry(string countryCode)
        {
            ServersByExitNodeViewModel countryRow = Items.OfType<ServersByExitNodeViewModel>()
                .FirstOrDefault(c => c.CountryCode.Equals(countryCode));
            if (countryRow != null)
            {
                ExpandCollection(countryRow);
            }
        }

        private void ExpandAction(IServerCollection serverCollection)
        {
            _scheduler.Schedule(() =>
            {
                if (!serverCollection.Expanded)
                {
                    ExpandCollection(serverCollection);
                }
                else
                {
                    CollapseCollection(serverCollection);
                }
            });
        }

        private async void ConnectAction(ServerItemViewModel serverItemViewModel)
        {
            if (serverItemViewModel.Connecting || serverItemViewModel.Connected)
            {
                await _serverConnector.Disconnect();
                return;
            }

            await _serverConnector.Connect(serverItemViewModel.Server);
        }

        private async void UpgradeActionAsync(ServerItemViewModel serverItemViewModel)
        {
            if (serverItemViewModel != null && !string.IsNullOrEmpty(serverItemViewModel.Server.ExitCountry) && _appSettings.FeatureFreeRescopeEnabled)
            {
                await _modals.ShowAsync<CountryUpsellModalViewModel>(serverItemViewModel.Server.ExitCountry);
            }
            else
            {
                await _modals.ShowAsync<UpsellModalViewModel>();
            }
        }

        private async void ShowFreeConnectionsModalActionAsync()
        {
            await _modals.ShowAsync<FreeConnectionsUpsellModalViewModel>();
        }

        private async void ConnectFastestActionAsync()
        {
            await _vpnManager.QuickConnectAsync();
        }

        private async void ConnectCountryActionAsync(IServerCollection serverCollection)
        {
            if (serverCollection is ServersByGatewayViewModel serversByGatewayViewModel)
            {
                await ConnectToGatewayAsync(serversByGatewayViewModel);
            }
            else
            {
                await ConnectToCountryAsync(serverCollection);
            }
        }

        private async Task ConnectToGatewayAsync(ServersByGatewayViewModel serversByGatewayViewModel)
        {
            Server currentServer = State.Server;
            if (State.Status.Equals(VpnStatus.Connected) &&
                serversByGatewayViewModel.Servers.Any(s => s.Id == currentServer.Id))
            {
                await _gatewayConnector.Disconnect();
            }
            else
            {
                await _gatewayConnector.ConnectAsync(serversByGatewayViewModel.Name);
            }
            
        }

        private async Task ConnectToCountryAsync(IServerCollection serverCollection)
        {
            Server currentServer = State.Server;
            if (State.Status.Equals(VpnStatus.Connected) &&
                serverCollection.CountryCode.Equals(currentServer?.ExitCountry))
            {
                await _countryConnector.Disconnect();
            }
            else
            {
                await _countryConnector.ConnectAsync(serverCollection.CountryCode);
            }
        }

        private void ClearSearchAction()
        {
            SearchValue = string.Empty;
        }

        private VpnState State => _vpnState.State;
    }
}
