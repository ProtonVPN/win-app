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

using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Name;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Translations;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProtonVPN.ConnectingScreen
{
    internal class ConnectingViewModel : ViewModel, IVpnStateAware
    {
        private int _percentage;
        private int _animatePercentage;
        private string _message;
        private Server _server;
        private Server _failedServer;

        public ICommand DisconnectCommand { get; set; }

        private readonly IVpnManager _vpnManager;

        private IName _connectionName;
        public IName ConnectionName
        {
            get => _connectionName;
            set => Set(ref _connectionName, value);
        }

        private IName _failedConnectionName;
        public IName FailedConnectionName
        {
            get => _failedConnectionName;
            set => Set(ref _failedConnectionName, value);
        }

        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        private bool _reconnecting;
        public bool Reconnecting
        {
            get => _reconnecting;
            set => Set(ref _reconnecting, value);
        }

        public int Percentage
        {
            get => _percentage;
            set => Set(ref _percentage, value);
        }

        public int AnimatePercentage
        {
            get => _animatePercentage;
            set => Set(ref _animatePercentage, value);
        }

        public ConnectingViewModel(IVpnManager vpnManager)
        {
            _vpnManager = vpnManager;

            DisconnectCommand = new RelayCommand(DisconnectAction);
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            switch (e.State.Status)
            {
                case VpnStatus.Connecting:
                    _server = null;
                    _failedServer = null;
                    Reconnecting = false;
                    SetConnecting(e.State);
                    break;
                case VpnStatus.Reconnecting:
                    SetConnecting(e.State);
                    break;
                case VpnStatus.Waiting:
                    SetConnectingState(20, Message = StringResources.Get("Connecting_VpnStatus_val_Waiting"), e.State.Server);
                    break;
                case VpnStatus.Authenticating:
                    SetConnectingState(40, Message = StringResources.Get("Connecting_VpnStatus_val_Authenticating"), e.State.Server);
                    break;
                case VpnStatus.RetrievingConfiguration:
                    SetConnectingState(60, Message = StringResources.Get("Connecting_VpnStatus_val_RetrievingConfiguration"), e.State.Server);
                    break;
                case VpnStatus.AssigningIp:
                    SetConnectingState(80, Message = StringResources.Get("Connecting_VpnStatus_val_AssigningIp"), e.State.Server);
                    break;
                case VpnStatus.Connected:
                    AnimatePercentage = 100;
                    Reconnecting = false;
                    Message = string.Empty;
                    FailedConnectionName = null;
                    _failedServer = null;
                    break;
                case VpnStatus.Disconnecting:
                case VpnStatus.Disconnected:
                    Percentage = 0;
                    Message = "";
                    Reconnecting = false;
                    ConnectionName = null;
                    FailedConnectionName = null;
                    _failedServer = null;
                    break;
            }

            return Task.CompletedTask;
        }

        private void SetConnecting(VpnState state)
        {
            Percentage = 0;
            SetConnectingState(0, Message = StringResources.Get("Connecting_VpnStatus_val_Connecting"), state.Server);
        }

        private void SetConnectingState(int percentage, string message, Server server)
        {
            AnimatePercentage = percentage;
            Message = message;

            if (server == null)
                return;

            ConnectionName = GetConnectionName(server);

            if (!server.Equals(_server))
            {
                _failedServer = _server;
                if (_failedServer != null)
                {
                    Reconnecting = true;
                    FailedConnectionName = GetConnectionName(_failedServer);
                }
            }

            _server = server;
        }

        private IName GetConnectionName(Server server)
        {
            return server.IsSecureCore() ? server.GetServerName() : server.GetNameWithCountry();
        }

        private async void DisconnectAction()
        {
            await _vpnManager.Disconnect();
        }
    }
}
