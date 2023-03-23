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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.DisconnectLogs;
using ProtonVPN.Common.NetShield;
using ProtonVPN.Common.PortForwarding;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Service.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnServiceManager : IVpnServiceManager
    {
        private readonly VpnService _vpnService;
        private readonly MainSettingsProvider _settingsContractProvider;
        private readonly ILogger _logger;
        private readonly IAppController _appController;
        private readonly IEntityMapper _entityMapper;
        private Action<VpnStateChangedEventArgs> _vpnStateCallback;
        private Action<PortForwardingState> _portForwardingStateCallback;
        private Action<ConnectionDetails> _connectionDetailsCallback;
        private Action<NetShieldStatistic> _netShieldStatisticCallback;

        public VpnServiceManager(
            VpnService vpnService,
            MainSettingsProvider settingsContractProvider,
            ILogger logger,
            IAppController appController,
            IEntityMapper entityMapper)
        {
            _vpnService = vpnService;
            _settingsContractProvider = settingsContractProvider;
            _logger = logger;
            _appController = appController;
            _entityMapper = entityMapper;
            _appController.OnVpnStateChanged += OnVpnStateChanged;
            _appController.OnPortForwardingStateChanged += OnPortForwardingStateChanged;
            _appController.OnConnectionDetailsChanged += OnConnectionDetailsChanged;
            _appController.OnNetShieldStatisticChanged += OnNetShieldStatisticChanged;
        }

        private void OnConnectionDetailsChanged(object sender, ConnectionDetailsIpcEntity connectionDetails)
        {
            Action<ConnectionDetails> callback = _connectionDetailsCallback;
            if (callback is not null)
            {
                callback(_entityMapper.Map<ConnectionDetailsIpcEntity, ConnectionDetails>(connectionDetails));
            }
        }

        private void OnVpnStateChanged(object sender, VpnStateIpcEntity state)
        {
            Action<VpnStateChangedEventArgs> callback = _vpnStateCallback;
            if (callback is not null)
            {
                callback(_entityMapper.Map<VpnStateIpcEntity, VpnStateChangedEventArgs>(state));
            }
        }

        private void OnPortForwardingStateChanged(object sender, PortForwardingStateIpcEntity state)
        {
            Action<PortForwardingState> callback = _portForwardingStateCallback;
            if (callback is not null)
            {
                callback(_entityMapper.Map<PortForwardingStateIpcEntity, PortForwardingState>(state));
            }
        }

        private void OnNetShieldStatisticChanged(object sender, NetShieldStatisticIpcEntity stats)
        {
            Action<NetShieldStatistic> callback = _netShieldStatisticCallback;
            if (callback is not null)
            {
                callback(_entityMapper.Map<NetShieldStatisticIpcEntity, NetShieldStatistic>(stats));
            }
        }

        public async Task Connect(VpnConnectionRequest request)
        {
            Ensure.NotNull(request, nameof(request));
            ConnectionRequestIpcEntity connectionRequest = 
                _entityMapper.Map<VpnConnectionRequest, ConnectionRequestIpcEntity>(request);
            connectionRequest.Settings = _settingsContractProvider.Create(request.Config.OpenVpnAdapter);
            await _vpnService.Connect(connectionRequest);
        }

        public async Task UpdateAuthCertificate(string certificate)
        {
            await _vpnService.UpdateAuthCertificate(new AuthCertificateIpcEntity() { Certificate = certificate });
        }

        public async Task<InOutBytes> Total()
        {
            return _entityMapper.Map<TrafficBytesIpcEntity, InOutBytes>(await _vpnService.Total());
        }

        public async Task RepeatState()
        {
            await _vpnService.RepeatState();
        }

        public Task Disconnect(VpnError vpnError = VpnError.None,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _logger.Info<DisconnectTriggerLog>($"Disconnect requested (Error: {vpnError})",
                sourceFilePath: sourceFilePath, sourceMemberName: sourceMemberName, sourceLineNumber: sourceLineNumber);
            DisconnectionRequestIpcEntity disconnectionRequest = new()
            {
                Settings = _settingsContractProvider.Create(),
                ErrorType = (VpnErrorTypeIpcEntity)vpnError
            };
            return _vpnService.Disconnect(disconnectionRequest);
        }

        public void RegisterVpnStateCallback(Action<VpnStateChangedEventArgs> callback)
        {
            _vpnStateCallback = callback;
        }

        public void RegisterPortForwardingStateCallback(Action<PortForwardingState> callback)
        {
            _portForwardingStateCallback = callback;
        }

        public void RegisterConnectionDetailsChangeCallback(Action<ConnectionDetails> callback)
        {
            _connectionDetailsCallback = callback;
        }

        public void RegisterNetShieldStatisticChangeCallback(Action<NetShieldStatistic> callback)
        {
            _netShieldStatisticCallback = callback;
        }
    }
}