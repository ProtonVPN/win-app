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

using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Vpn.Connectors;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ProtonVPN.Map.ViewModels.Pins
{
    internal class ExitNodePinViewModel : AbstractPinViewModel
    {
        private readonly PinFactory _pinFactory;
        private readonly ServerManager _serverManager;
        private readonly MapLineManager _mapLineManager;
        private readonly ServerConnector _serverConnector;

        private List<ExitServerViewModel> _servers;
        public List<ExitServerViewModel> Servers
        {
            get => _servers;
            set => Set(ref _servers, value);
        }

        public ICommand ToggleServersCommand { get; set; }
        public ICommand ExactServerConnectCommand { get; set; }
        public ICommand HandleConnectEventCommand { get; set; }

        public ExitNodePinViewModel(
            string countryCode,
            ServerManager serverManager,
            MapLineManager mapLineManager,
            ServerConnector serverConnector,
            PinFactory pinFactory) : base(countryCode)
        {
            _mapLineManager = mapLineManager;
            _serverConnector = serverConnector;
            _pinFactory = pinFactory;
            _serverManager = serverManager;

            ExactServerConnectCommand = new RelayCommand<ExitServerViewModel>(ExactServerConnectAction);
            SetServers();
        }

        public override void OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            Connected = e.State.Server is Server server
                        && e.State.Status == VpnStatus.Connected
                        && server.ExitCountry.EqualsIgnoringCase(CountryCode);

            foreach (var s in Servers)
            {
                s.Connected = e.State.Status == VpnStatus.Connected &&
                              e.State.Server.Equals(s.Server);
            }
        }

        protected override bool BeforeShowTooltip()
        {
            _pinFactory.HideSecureCorePins();
            _pinFactory.HideExitPins();
            _mapLineManager.HideExitLines();
            _mapLineManager.HideHomeLines();

            if (State.Equals(PinStates.Disconnected))
            {
                _mapLineManager.SetEntryLinesVisibility(CountryCode, true);
                _mapLineManager.SetHomeLineVisibilityByExitNode(CountryCode, true);
                _mapLineManager.SetSecureCoreLinesVisibility(false);
                _pinFactory.ShowSecureCoreTooltipByExitNode(CountryCode);
            }

            return true;
        }

        protected override bool BeforeHideTooltip()
        {
            _mapLineManager.SetEntryLinesVisibility(CountryCode, false);
            _mapLineManager.SetHomeLineVisibilityByExitNode(CountryCode, false);
            _mapLineManager.SetSecureCoreLinesVisibility(true);
            _pinFactory.HideSecureCorePins();
            return true;
        }

        protected override void ShowTooltipAction()
        {
            ShowTooltip = !ShowTooltip;
        }

        protected override void ConnectAction()
        {
            throw new NotImplementedException();
        }

        private void SetServers()
        {
            var list = new List<ExitServerViewModel>();
            var servers = _serverManager.GetServers(new SecureCoreServer() && new ExitCountryServer(CountryCode));
            foreach (var server in servers)
            {
                list.Add(new ExitServerViewModel(server));
            }

            Servers = list;
        }

        private async void ExactServerConnectAction(ExitServerViewModel vm)
        {
            if (Connected)
            {
                await _serverConnector.Disconnect();
            }
            else
            {
                await _serverConnector.Connect(vm.Server);
            }
        }
    }
}
