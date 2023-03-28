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
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Servers;
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
        IServersAware
    {
        private readonly IAppSettings _appSettings;
        private readonly ServerListFactory _serverListFactory;
        private readonly IScheduler _scheduler;
        private readonly ServerConnector _serverConnector;
        private readonly CountryConnector _countryConnector;

        private VpnStateChangedEventArgs _vpnState = new(new VpnState(VpnStatus.Disconnected), VpnError.None, false);

        public CountriesViewModel(
            IAppSettings appSettings,
            ServerListFactory serverListFactory,
            IScheduler scheduler,
            ServerConnector serverConnector,
            CountryConnector countryConnector,
            QuickSettingsViewModel quickSettingsViewModel)
        {
            _appSettings = appSettings;
            _serverListFactory = serverListFactory;
            _scheduler = scheduler;
            _serverConnector = serverConnector;
            _countryConnector = countryConnector;
            QuickSettingsViewModel = quickSettingsViewModel;

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

            _scheduler.Schedule(() =>
            {
                serverCollection.Expanded = true;
                int index = _items.IndexOf(serverCollection) + 1;

                ObservableCollection<IServerListItem> collection = 
                    new ObservableCollection<IServerListItem>(serverCollection.Servers.Reverse());
                foreach (IServerListItem serverListItem in collection)
                {
                    _items.Insert(index, serverListItem);
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
                }
                else
                {
                    ExpandCountry(server.EntryCountry);
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

        private void UpdateVpnState(VpnState state)
        {
            if (Items == null)
            {
                return;
            }

            foreach (IServerListItem item in Items.ToList())
            {
                if (item is ServersByCountryViewModel or ServersByExitNodeViewModel or CountrySeparatorViewModel)
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
