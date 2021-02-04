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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Servers;
using ProtonVPN.Settings;
using ProtonVPN.Sidebar.QuickSettings;
using ProtonVPN.Trial;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.Sidebar
{
    internal class CountriesViewModel :
        Screen,
        IVpnPlanAware,
        IVpnStateAware,
        ISettingsAware,
        ITrialStateAware,
        ILogoutAware,
        IServersAware
    {
        private readonly IAppSettings _appSettings;
        private readonly ServerListFactory _serverListFactory;
        private readonly App _app;
        private readonly ServerConnector _serverConnector;
        private readonly CountryConnector _countryConnector;
        private readonly IVpnReconnector _vpnReconnector;

        private VpnStateChangedEventArgs _vpnState = new VpnStateChangedEventArgs(new VpnState(VpnStatus.Disconnected), VpnError.None, false);

        public CountriesViewModel(
            IAppSettings appSettings,
            ServerListFactory serverListFactory,
            App app,
            ServerConnector serverConnector,
            CountryConnector countryConnector,
            QuickSettingsViewModel quickSettingsViewModel,
            IVpnReconnector vpnReconnector)
        {
            _appSettings = appSettings;
            _serverListFactory = serverListFactory;
            _app = app;
            _serverConnector = serverConnector;
            _countryConnector = countryConnector;
            QuickSettingsViewModel = quickSettingsViewModel;
            _vpnReconnector = vpnReconnector;

            Connect = new RelayCommand<ServerItemViewModel>(ConnectAction);
            ConnectCountry = new RelayCommand<IServerCollection>(ConnectCountryAction);
            Expand = new RelayCommand<IServerCollection>(ExpandAction);
            ClearSearchCommand = new RelayCommand(ClearSearchAction);
        }

        public QuickSettingsViewModel QuickSettingsViewModel { get; }

        public ICommand Connect { get; }
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

        private ObservableCollection<IServerListItem> _items = new ObservableCollection<IServerListItem>();
        public ObservableCollection<IServerListItem> Items
        {
            get => _items;
            set => Set(ref _items, value);
        }

        public void ExpandCollection(IServerCollection serverCollection)
        {
            if (serverCollection.Expanded || !serverCollection.HasAvailableServers())
            {
                return;
            }

            serverCollection.Expanded = true;

            int index = _items.IndexOf(serverCollection) + 1;
            _app.Dispatcher?.Invoke(() =>
            {
                ObservableCollection<IServerListItem> collection = 
                    new ObservableCollection<IServerListItem>(serverCollection.Servers.Reverse());
                foreach (IServerListItem serverListItem in collection)
                {
                    if (serverListItem is ServerItemViewModel server)
                    {
                        _items.Insert(index, server);
                    }
                }
            });
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
                if (item is ServerItemViewModel server)
                {
                    _items.Remove(server);
                }
            }
        }

        public void Load()
        {
            SetSecureCore(_appSettings.SecureCore);
            SetPortForwarding(_appSettings);
            CreateList();
        }

        public void OnServersUpdated()
        {
            CreateList();
            if (!State.Status.Equals(VpnStatus.Connected) ||
                !(State.Server is Server server))
            {
                return;
            }

            ExpandCountry(server.EntryCountry);
            UpdateVpnState(State);

            if (server.IsSecureCore())
            {
                NotifyScServerRowsOfConnectedState(server, true);
            }
            else
            {
                NotifyServerRowsOfConnectedState(server, true);
            }
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IAppSettings.SecureCore):
                    SetSecureCore(_appSettings.SecureCore);
                    CreateList();
                    break;

                case nameof(IAppSettings.PortForwardingEnabled):
                case nameof(IAppSettings.FeaturePortForwardingEnabled):
                    SetPortForwarding(_appSettings);
                    CreateList();
                    break;

                case nameof(IAppSettings.Language):
                    CreateList();
                    break;
            }
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnState = e;

            if (!(e.State.Server is Server server))
            {
                return;
            }

            if (e.State.Status == VpnStatus.Connected)
            {
                if (server.IsSecureCore())
                {
                    ExpandScCountry(server.ExitCountry);
                    UpdateVpnState(e.State);
                    NotifyScServerRowsOfConnectedState(server, true);
                }
                else
                {
                    ExpandCountry(server.EntryCountry);
                    UpdateVpnState(e.State);
                    NotifyServerRowsOfConnectedState(server, true);
                }
            }
            else
            {
                UpdateVpnState(e.State);
            }
        }

        public async Task OnVpnPlanChangedAsync(string plan)
        {
            CreateList();
        }

        public async Task OnTrialStateChangedAsync(PlanStatus status)
        {
            if (status == PlanStatus.Expired)
            {
                _appSettings.SecureCore = false;
                await ReconnectAsync();
            }
        }

        private async Task ReconnectAsync()
        {
            await _vpnReconnector.ReconnectAsync();
        }

        public void OnUserLoggedOut()
        {
            _searchValue = string.Empty;
            NotifyOfPropertyChange(nameof(SearchValue));
        }

        private void SetSecureCore(bool value)
        {
            Set(ref _secureCore, value, nameof(SecureCore));
        }

        private void SetPortForwarding(IAppSettings appSettings)
        {
            bool isPortForwardingEnabled = appSettings.PortForwardingEnabled && appSettings.FeaturePortForwardingEnabled;
            Set(ref _portForwarding, isPortForwardingEnabled, nameof(PortForwarding));
        }

        private void CreateList()
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
        }

        private void NotifyServerRowsOfConnectedState(Server server, bool connected)
        {
            IEnumerable<ServerItemViewModel> serverRows = Items.OfType<ServerItemViewModel>()
                .Where(c => c.Server.EntryCountry.Equals(server.EntryCountry));

            foreach (ServerItemViewModel row in serverRows)
            {
                row.SetConnectedToCountry(connected);
            }
        }

        private void NotifyScServerRowsOfConnectedState(Server server, bool connected)
        {
            IEnumerable<SecureCoreItemViewModel> serverRows = Items.OfType<SecureCoreItemViewModel>()
                .Where(c => c.Server.IsSecureCore() && c.Server.ExitCountry.Equals(server.ExitCountry));

            foreach (SecureCoreItemViewModel row in serverRows)
            {
                row.SetConnectedToCountry(connected);
            }
        }

        private void UpdateVpnState(VpnState state)
        {
            if (Items == null)
            {
                return;
            }

            foreach (IServerListItem item in Items)
            {
                if (item is ServersByCountryViewModel || item is ServersByExitNodeViewModel || item is CountrySeparatorViewModel)
                {
                    item.OnVpnStateChanged(state);
                }
            }
        }

        private void ExpandCountry(string countryCode)
        {
            ServersByCountryViewModel countryRow = Items.OfType<ServersByCountryViewModel>()
                .FirstOrDefault(c => c.CountryCode.Equals(countryCode));
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
            if (!serverCollection.Expanded)
            {
                ExpandCollection(serverCollection);
            }
            else
            {
                CollapseCollection(serverCollection);
            }
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

        private async void ConnectCountryAction(IServerCollection serverCollection)
        {
            Server currentServer = State.Server;

            if (State.Status.Equals(VpnStatus.Connected) &&
                serverCollection.CountryCode.Equals(currentServer?.ExitCountry))
            {
                await _countryConnector.Disconnect();
            }
            else
            {
                await _countryConnector.Connect(serverCollection.CountryCode);
            }
        }

        private void ClearSearchAction()
        {
            SearchValue = string.Empty;
        }

        private VpnState State => _vpnState.State;
    }
}
