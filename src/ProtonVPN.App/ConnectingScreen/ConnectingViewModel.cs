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

using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Name;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Translations;

namespace ProtonVPN.ConnectingScreen
{
    internal class ConnectingViewModel : ViewModel, IVpnStateAware
    {
        private bool _reconnecting;
        private bool _hasPercentage;
        private int? _percentage;
        private int? _animatePercentage;
        private string _message;
        private Server _server;
        private Server _failedServer;

        private readonly IVpnManager _vpnManager;

        public ConnectingViewModel(IVpnManager vpnManager)
        {
            _vpnManager = vpnManager;

            DisconnectCommand = new RelayCommand(DisconnectAction);
        }

        public ICommand DisconnectCommand { get; set; }

        private IName _connectionName;

        public IName ConnectionName
        {
            get => _connectionName;
            set
            {
                Set(ref _connectionName, value);
                OnPropertyChanged(nameof(IsConnectionNameNotNull));
            }
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

        public bool Reconnecting
        {
            get => _reconnecting;
            set => Set(ref _reconnecting, value);
        }

        public bool HasPercentage
        {
            get => _hasPercentage;
            set => Set(ref _hasPercentage, value);
        }

        public int? Percentage
        {
            get => _percentage;
            set
            {
                Set(ref _percentage, value);
                HasPercentage = _percentage.HasValue;
            } 
        }

        public int? AnimatePercentage
        {
            get => _animatePercentage;
            set
            {
                Set(ref _animatePercentage, value);
                HasPercentage = _animatePercentage.HasValue;
            } 
        }

        public bool IsConnectionNameNotNull
        {
            get => ConnectionName != null;
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            switch (e.State.Status)
            {
                case VpnStatus.Pinging:
                    SetStartStep();
                    SetPinging();
                    break;
                case VpnStatus.Connecting:
                    _server = null;
                    ResetReconnectingAndFailedServer();
                    SetStartStep();
                    SetConnecting(e.State);
                    break;
                case VpnStatus.Reconnecting:
                    SetConnectingStateAndPercentage(0, e.State.Server,
                        Message = Translation.Get("Connecting_VpnStatus_val_Connecting"));
                    break;
                case VpnStatus.Waiting:
                    SetConnectingStateAndPercentage(20, e.State.Server,
                        Message = Translation.Get("Connecting_VpnStatus_val_Waiting"));
                    break;
                case VpnStatus.Authenticating:
                    SetConnectingStateAndPercentage(40, e.State.Server,
                        Message = Translation.Get("Connecting_VpnStatus_val_Authenticating"));
                    break;
                case VpnStatus.RetrievingConfiguration:
                    SetConnectingStateAndPercentage(60, e.State.Server,
                        Message = Translation.Get("Connecting_VpnStatus_val_RetrievingConfiguration"));
                    break;
                case VpnStatus.AssigningIp:
                    SetConnectingStateAndPercentage(80, e.State.Server,
                        Message = Translation.Get("Connecting_VpnStatus_val_AssigningIp"));
                    break;
                case VpnStatus.Connected:
                    AnimatePercentage = 100;
                    ResetConnectingState();
                    break;
                case VpnStatus.Disconnecting:
                case VpnStatus.Disconnected:
                    Percentage = 0;
                    ConnectionName = null;
                    ResetConnectingState();
                    break;
            }
        }

        private void ResetReconnectingAndFailedServer()
        {
            Reconnecting = false;
            _failedServer = null;
        }

        private void ResetConnectingState()
        {
            ResetReconnectingAndFailedServer();
            Message = string.Empty;
            FailedConnectionName = null;
        }

        private void SetStartStep()
        {
            _server = null;
            _failedServer = null;
            Reconnecting = false;
        }

        private void SetPinging()
        {
            ConnectionName = null;
            FailedConnectionName = null;
            Percentage = 0;
            SetConnectingStateAndPercentage(0, null, Message = Translation.Get("Connecting_VpnStatus_val_Pinging"));
        }

        private void SetConnectingStateAndPercentage(int percentage, Server server, string message)
        {
            AnimatePercentage = percentage;
            SetConnectingState(server, message);
        }

        private void SetConnecting(VpnState state)
        {
            Percentage = 0;
            SetConnectingStateAndPercentage(0, state.Server, Message = Translation.Get("Connecting_VpnStatus_val_Connecting"));
        }

        private void SetConnectingState(Server server, string message)
        {
            Message = message;

            if (server == null)
            {
                return;
            }

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
            await _vpnManager.DisconnectAsync();
        }
    }
}