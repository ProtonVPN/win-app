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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Api;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectLogs;
using ProtonVPN.Common.Logging.Categorization.Events.DisconnectLogs;
using ProtonVPN.Common.Logging.Categorization.Events.GuestHoleLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.GuestHoles.FileStoraging;

namespace ProtonVPN.Vpn.Connectors
{
    public class GuestHoleConnector
    {
        private int _reconnects;

        private readonly Random _random = new();

        private readonly IVpnServiceManager _vpnServiceManager;
        private readonly IAppSettings _appSettings;
        private readonly GuestHoleState _guestHoleState;
        private readonly IConfiguration _config;
        private readonly IGuestHoleServersFileStorage _guestHoleServersFileStorage;
        private readonly INetworkAdapterValidator _networkAdapterValidator;
        private readonly ILogger _logger;

        public GuestHoleConnector(
            IVpnServiceManager vpnServiceManager,
            IAppSettings appSettings,
            GuestHoleState guestHoleState,
            IConfiguration config,
            IGuestHoleServersFileStorage guestHoleServersFileStorage,
            INetworkAdapterValidator networkAdapterValidator, 
            ILogger logger)
        {
            _vpnServiceManager = vpnServiceManager;
            _appSettings = appSettings;
            _guestHoleState = guestHoleState;
            _config = config;
            _guestHoleServersFileStorage = guestHoleServersFileStorage;
            _networkAdapterValidator = networkAdapterValidator;
            _logger = logger;
        }

        public async Task Connect()
        {
            if (_networkAdapterValidator.IsOpenVpnAdapterAvailable())
            {
                _logger.Info<GuestHoleLog>("OpenVPN adapters are available. Proceeding with guest hole connection.");

                VpnConnectionRequest request = new(
                    Servers(),
                    VpnProtocol.Smart,
                    VpnConfig(),
                    CreateVpnCredentials());

                _logger.Info<ConnectTriggerLog>("Guest hole connection requested.");
                await _vpnServiceManager.Connect(request);
            }
            else
            {
                _logger.Info<GuestHoleLog>("OpenVPN adapters are unavailable. Disconnecting.");
                _logger.Info<DisconnectTriggerLog>("Guest hole disconnection requested.");
                await Disconnect();
            }
        }

        private VpnCredentials CreateVpnCredentials()
        {
            string username = AddSuffixToUsername(_config.GuestHoleVpnUsername);
            string password = _config.GuestHoleVpnPassword;
            return new(username, password);
        }

        private string AddSuffixToUsername(string username)
        {
            return username + _config.VpnUsernameSuffix;
        }

        public async Task Disconnect()
        {
            await _vpnServiceManager.Disconnect(VpnError.NoneKeepEnabledKillSwitch);
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (!_guestHoleState.Active)
            {
                return;
            }

            if (e.State.Status == VpnStatus.Connected)
            {
                _reconnects = 0;
                return;
            }

            if (e.State.Status == VpnStatus.Reconnecting)
            {
                _reconnects++;
            }

            if (_reconnects >= _config.MaxGuestHoleRetries)
            {
                _reconnects = 0;
                await Disconnect();
            }
        }

        public IReadOnlyList<VpnHost> Servers()
        {
            return _guestHoleServersFileStorage
                   .Get()
                   .Select(server => new VpnHost(server.Host, server.Ip, server.Label, null, server.Signature))
                   .OrderBy(_ => _random.Next())
                   .ToList();
        }

        private VpnConfig VpnConfig()
        {
            Dictionary<VpnProtocol, IReadOnlyCollection<int>> portConfig = new()
            {
                {VpnProtocol.OpenVpnUdp, _appSettings.OpenVpnUdpPorts},
                {VpnProtocol.OpenVpnTcp, _appSettings.OpenVpnTcpPorts},
            };

            return new VpnConfig(new VpnConfigParameters
            {
                Ports = portConfig,
                OpenVpnAdapter = OpenVpnAdapter.Tap,
                PreferredProtocols = new List<VpnProtocol>
                {
                    VpnProtocol.OpenVpnUdp,
                    VpnProtocol.OpenVpnTcp,
                },
            });
        }
    }
}