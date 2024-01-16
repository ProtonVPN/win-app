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
using System.Text;
using System.Threading.Tasks;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.NetShield;
using ProtonVPN.Common.Legacy.PortForwarding;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Service.Settings;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.NetShield;
using ProtonVPN.Vpn.PortMapping;

namespace ProtonVPN.Service.ProcessCommunication
{
    public class AppControllerCaller : IAppControllerCaller, IServiceSettingsAware
    {
        private readonly KillSwitch.KillSwitch _killSwitch;
        private readonly ILogger _logger;
        private readonly IServiceGrpcClient _grpcClient;
        private readonly IEntityMapper _entityMapper;
        private readonly IVpnConnection _vpnConnection;
        private readonly IPortMappingProtocolClient _portMappingProtocolClient;
        private readonly INetShieldStatisticEventManager _netShieldStatisticEventManager;

        private VpnState _vpnState;
        private PortForwardingState _portForwardingState;
        private ConnectionDetails _connectionDetails;
        private NetShieldStatistic _netShieldStatistic;

        public AppControllerCaller(
            KillSwitch.KillSwitch killSwitch,
            ILogger logger,
            IServiceGrpcClient grpcClient,
            IEntityMapper entityMapper,
            IVpnConnection vpnConnection,
            IPortMappingProtocolClient portMappingProtocolClient,
            INetShieldStatisticEventManager netShieldStatisticEventManager)
        {
            _killSwitch = killSwitch;
            _logger = logger;
            _grpcClient = grpcClient;
            _entityMapper = entityMapper;
            _vpnConnection = vpnConnection;
            _vpnConnection.StateChanged += OnVpnStateChanged;
            _vpnConnection.ConnectionDetailsChanged += OnConnectionDetailsChanged;
            _portMappingProtocolClient = portMappingProtocolClient;
            _portMappingProtocolClient.StateChanged += OnPortForwardingStateChanged;
            _netShieldStatisticEventManager = netShieldStatisticEventManager;
            _netShieldStatisticEventManager.NetShieldStatisticChanged += OnNetShieldStatisticChanged;
        }

        private async Task SendAsync(Func<IAppController, Task> action)
        {
            IAppController controller = _grpcClient.AppController;
            if (controller is null)
            {
                await _grpcClient.RecreateAsync();
                controller = _grpcClient.AppController;
                if (controller is null)
                {
                    return;
                }
            }
            try
            {
                await action(controller);
            }
            catch (Exception e)
            {
                _logger.Error<ProcessCommunicationErrorLog>("Failed to send message to App. " +
                    "Recreating gRPC client and making a second and last try to send message.", e);
                await OnSendAsyncFail(action);
            }
        }

        private async Task OnSendAsyncFail(Func<IAppController, Task> action)
        {
            await _grpcClient.RecreateAsync();
            IAppController controller = _grpcClient.AppController;
            if (controller is not null)
            {
                _logger.Info<ProcessCommunicationLog>("Resending message after recreating gRPC client.");
                try
                {
                    await action(controller);
                }
                catch (Exception e)
                {
                    _logger.Error<ProcessCommunicationErrorLog>("Failed to send message to App. " +
                        "No more retries left.", e);
                }
            }
            else
            {
                _logger.Error<ProcessCommunicationErrorLog>("Cannot resend message " +
                    "after recreating gRPC client because the controller is null.");
            }
        }

        public async Task SendCurrentVpnStateAsync()
        {
            VpnState vpnState = _vpnState;
            if (vpnState is not null)
            {
                await SendStateChangeAsync(vpnState);
            }
        }

        private async void OnVpnStateChanged(object sender, EventArgs<VpnState> e)
        {
            VpnState state = e.Data;
            _vpnState = state;
            await SendStateChangeAsync(state);
        }

        private async Task SendStateChangeAsync(VpnState state)
        {
            _logger.Info<ProcessCommunicationLog>($"Sending VPN Status '{state.Status}', Error: '{state.Error}', " +
                $"LocalIp: '{state.LocalIp}', RemoteIp: '{state.RemoteIp}', Label: '{state.Label}', " +
                $"VpnProtocol: '{state.VpnProtocol}', OpenVpnAdapter: '{state.OpenVpnAdapter}'");
            await SendAsync(appController => appController.VpnStateChange(CreateVpnStateIpcEntity(state)));
        }

        private VpnStateIpcEntity CreateVpnStateIpcEntity(VpnState state)
        {
            bool killSwitchEnabled = _killSwitch.ExpectedLeakProtectionStatus(state);
            if (!killSwitchEnabled)
            {
                _vpnState = new VpnState(state.Status, state.Error, state.VpnProtocol);
            }

            return new VpnStateIpcEntity
            {
                Status = _entityMapper.Map<VpnStatus, VpnStatusIpcEntity>(state.Status),
                Error = _entityMapper.Map<VpnError, VpnErrorTypeIpcEntity>(state.Error),
                EndpointIp = state.RemoteIp,
                NetworkBlocked = killSwitchEnabled,
                OpenVpnAdapterType = _entityMapper.MapNullableStruct<OpenVpnAdapter, OpenVpnAdapterIpcEntity>(state.OpenVpnAdapter),
                VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(state.VpnProtocol),
                Label = state.Label,
            };
        }

        private async void OnConnectionDetailsChanged(object sender, ConnectionDetails connectionDetails)
        {
            _connectionDetails = connectionDetails;
            await SendConnectionDetailsChangeAsync(connectionDetails);
        }

        private async Task SendConnectionDetailsChangeAsync(ConnectionDetails connectionDetails)
        {
            _logger.Info<ProcessCommunicationLog>("Sending ConnectionDetails change while connected " +
                $"to server with IP '{connectionDetails.ServerIpAddress}'");
            ConnectionDetailsIpcEntity connectionDetailsIpcEntity =
                _entityMapper.Map<ConnectionDetails, ConnectionDetailsIpcEntity>(connectionDetails);
            await SendAsync(appController => appController.ConnectionDetailsChange(connectionDetailsIpcEntity));
        }

        public async Task SendCurrentPortForwardingStateAsync()
        {
            await SendPortForwardingStateChangeAsync(_portForwardingState);
        }

        public async Task SendUpdateStateAsync(UpdateStateIpcEntity updateState)
        {
            await SendAsync(appController => appController.UpdateStateChange(updateState));
        }

        private async void OnPortForwardingStateChanged(object sender, EventArgs<PortForwardingState> e)
        {
            PortForwardingState state = e.Data;
            _portForwardingState = state;
            await SendPortForwardingStateChangeAsync(state);
        }

        private async Task SendPortForwardingStateChangeAsync(PortForwardingState state)
        {
            StringBuilder logMessage = new StringBuilder().Append("Sending PortForwarding " +
                $"Status '{state.Status}' triggered at '{state.TimestampUtc}'");
            if (state.MappedPort?.MappedPort is not null)
            {
                TemporaryMappedPort mappedPort = state.MappedPort;
                logMessage.Append($", Port pair {mappedPort.MappedPort}, expiring in " +
                                  $"{mappedPort.Lifetime} at {mappedPort.ExpirationDateUtc}");
            }
            _logger.Info<ProcessCommunicationLog>(logMessage.ToString());
            PortForwardingStateIpcEntity stateIpcEntity = 
                _entityMapper.Map<PortForwardingState, PortForwardingStateIpcEntity>(state);
            await SendAsync(appController => appController.PortForwardingStateChange(stateIpcEntity));
        }

        private async void OnNetShieldStatisticChanged(object sender, NetShieldStatistic stats)
        {
            _netShieldStatistic = stats;
            _logger.Info<ProcessCommunicationLog>($"Sending NetShield statistic triggered at '{stats.TimestampUtc}' " +
                $"[Ads: '{stats.NumOfAdvertisementUrlsBlocked}']" +
                $"[Malware: '{stats.NumOfMaliciousUrlsBlocked}']" +
                $"[Trackers: '{stats.NumOfTrackingUrlsBlocked}']");
            NetShieldStatisticIpcEntity statsIpcEntity =
                _entityMapper.Map<NetShieldStatistic, NetShieldStatisticIpcEntity>(stats);
            await SendAsync(appController => appController.NetShieldStatisticChange(statsIpcEntity));
        }

        public async void OnServiceSettingsChanged(MainSettingsIpcEntity settings)
        {
            VpnState vpnState = _vpnState ?? new VpnState(VpnStatus.Disconnected, VpnProtocol.Smart);
            if (vpnState.Status == VpnStatus.Disconnected)
            {
                _logger.Info<ProcessCommunicationLog>($"Sending VPN Service Settings Change. " +
                    $"Status: '{vpnState.Status}' (Error: '{vpnState.Error}')");
                await SendStateChangeAsync(vpnState);
            }
            else if (vpnState.Status == VpnStatus.Connected)
            {
                _vpnConnection.SetFeatures(CreateVpnFeatures(settings));
            }
        }

        private VpnFeatures CreateVpnFeatures(MainSettingsIpcEntity settings)
        {
            return new()
            {
                SplitTcp = settings.SplitTcp,
                NetShieldMode = settings.NetShieldMode,
                PortForwarding = settings.PortForwarding,
                ModerateNat = settings.ModerateNat,
            };
        }
    }
}
