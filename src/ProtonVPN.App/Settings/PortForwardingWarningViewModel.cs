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

using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Settings
{
    public class PortForwardingWarningViewModel : Screen, IVpnStateAware, ISettingsAware
    {
        private readonly IAppSettings _appSettings;
        private readonly IVpnConnector _vpnConnector;

        private Server _lastServer = Server.Empty();
        private VpnStatus _lastVpnStatus = VpnStatus.Disconnected;

        public bool IsToShowPortForwardingWarningLabel =>
            _appSettings.IsPortForwardingEnabled() &&
            (_lastVpnStatus is VpnStatus.Disconnected or VpnStatus.Disconnecting ||
             _lastVpnStatus == VpnStatus.Connected &&
             !_lastServer.IsFeatureSupported(Features.P2P));

        public ICommand ConnectToBestP2PServerCommand { get; set; }

        public PortForwardingWarningViewModel(IAppSettings appSettings, IVpnConnector vpnConnector)
        {
            _appSettings = appSettings;
            _vpnConnector = vpnConnector;
            ConnectToBestP2PServerCommand = new RelayCommand(ConnectToBestP2PServer);
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _lastServer = e.State.Server;
            _lastVpnStatus = e.State.Status;
            NotifyOfPropertyChange(nameof(IsToShowPortForwardingWarningLabel));
            return Task.CompletedTask;
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(IAppSettings.PortForwardingEnabled)
                or nameof(IAppSettings.FeaturePortForwardingEnabled))
            {
                NotifyOfPropertyChange(nameof(IsToShowPortForwardingWarningLabel));
            }
        }

        private async void ConnectToBestP2PServer()
        {
            await _vpnConnector.QuickConnectAsync();
        }
    }
}