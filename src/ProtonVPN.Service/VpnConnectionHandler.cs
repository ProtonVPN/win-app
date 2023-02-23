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
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ProtonVPN.Common;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectionLogs;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectLogs;
using ProtonVPN.Common.Logging.Categorization.Events.DisconnectLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.PortForwarding;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Service.Contract.PortForwarding;
using ProtonVPN.Service.Contract.Settings;
using ProtonVPN.Service.Contract.Vpn;
using ProtonVPN.Service.Settings;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.PortMapping;

namespace ProtonVPN.Service
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Single)]
    public class VpnConnectionHandler : IVpnConnectionContract, IServiceSettingsAware
    {
        private readonly object _callbackLock = new();
        private readonly List<IVpnEventsContract> _callbacks = new();

        private readonly KillSwitch.KillSwitch _killSwitch;
        private readonly IVpnConnection _vpnConnection;
        private readonly ILogger _logger;
        private readonly IServiceSettings _serviceSettings;
        private readonly ITaskQueue _taskQueue;
        private readonly IPortMappingProtocolClient _portMappingProtocolClient;

        private VpnState _state = new(VpnStatus.Disconnected, default);

        public VpnConnectionHandler(
            KillSwitch.KillSwitch killSwitch,
            IVpnConnection vpnConnection,
            ILogger logger,
            IServiceSettings serviceSettings,
            ITaskQueue taskQueue,
            IPortMappingProtocolClient portMappingProtocolClient)
        {
            _killSwitch = killSwitch;
            _vpnConnection = vpnConnection;
            _logger = logger;
            _serviceSettings = serviceSettings;
            _taskQueue = taskQueue;
            _portMappingProtocolClient = portMappingProtocolClient;

            _vpnConnection.StateChanged += VpnConnection_StateChanged;
            _vpnConnection.ConnectionDetailsChanged += OnConnectionDetailsChanged;
            _portMappingProtocolClient.StateChanged += PortForwarding_StateChanged;
        }

        private void OnConnectionDetailsChanged(object sender, ConnectionDetails e)
        {
            Callback(callback => callback.OnConnectionDetailsChanged(Map(e)));
        }

        public async Task Connect(VpnConnectionRequestContract connectionRequest)
        {
            Ensure.NotNull(connectionRequest, nameof(connectionRequest));

            _logger.Info<ConnectLog>("Connect requested");

            _serviceSettings.Apply(connectionRequest.Settings);

            VpnConfig config = Map(connectionRequest.VpnConfig);
            IReadOnlyList<VpnHost> endpoints = Map(connectionRequest.Servers);
            VpnCredentials credentials = Map(connectionRequest.Credentials);
            _vpnConnection.Connect(endpoints, config, credentials);
        }

        public Task UpdateAuthCertificate(string certificate)
        {
            _vpnConnection.UpdateAuthCertificate(certificate);
            return Task.CompletedTask;
        }

        public Task Disconnect(SettingsContract settings, VpnErrorTypeContract vpnError)
        {
            _logger.Info<DisconnectLog>($"Disconnect requested (Error: {vpnError})");

            _serviceSettings.Apply(settings);

            _vpnConnection.Disconnect(Map(vpnError));

            return Task.CompletedTask;
        }

        public Task RepeatState()
        {
            _taskQueue.Enqueue(() =>
            {
                CallbackStateChanged(_state);
            });

            return Task.CompletedTask;
        }

        private void CallbackStateChanged(VpnState state)
        {
            _logger.Info<ConnectionStateChangeLog>($"Callbacking VPN state '{state.Status}' [Error: '{state.Error}', " +
                $"LocalIp: '{state.LocalIp}', RemoteIp: '{state.RemoteIp}', Label: '{state.Label}', " +
                $"VpnProtocol: '{state.VpnProtocol}', OpenVpnAdapter: '{state.OpenVpnAdapter}']");
            Callback(callback => callback.OnStateChanged(Map(state)));
        }

        private void Callback(Action<IVpnEventsContract> action)
        {
            lock (_callbackLock)
            {
                foreach (IVpnEventsContract callback in _callbacks.ToList())
                {
                    try
                    {
                        action(callback);
                    }
                    catch (Exception ex) when (ex.IsServiceCommunicationException())
                    {
                        _logger.Warn<ConnectionLog>($"Callback failed: {ex.Message}");
                        _callbacks.Remove(callback);
                    }
                    catch (TimeoutException)
                    {
                        _logger.Warn<ConnectionLog>("Callback timed out");
                    }
                }
            }
        }

        public Task<InOutBytesContract> Total()
        {
            return Map(_vpnConnection.Total).AsTask();
        }

        public Task RepeatPortForwardingState()
        {
            _portMappingProtocolClient.RepeatState();
            return Task.CompletedTask;
        }

        public Task RegisterCallback()
        {
            lock (_callbackLock)
            {
                _callbacks.Add(OperationContext.Current.GetCallbackChannel<IVpnEventsContract>());
            }

            return Task.CompletedTask;
        }

        public Task UnRegisterCallback()
        {
            _logger.Info<ConnectionLog>("Unregister callback requested");

            lock (_callbackLock)
            {
                _callbacks.Remove(OperationContext.Current.GetCallbackChannel<IVpnEventsContract>());
            }

            return Task.CompletedTask;
        }

        public void OnServiceSettingsChanged(SettingsContract settings)
        {
            if (_state.Status == VpnStatus.Disconnected)
            {
                _logger.Info<ConnectionLog>($"Callbacking VPN service settings change. Current state: {_state.Status} (Error: {_state.Error})");
                Callback(callback => callback.OnServiceSettingsStateChanged(CreateServiceSettingsState()));
            }
            else if (_state.Status == VpnStatus.Connected)
            {
                _vpnConnection.SetFeatures(CreateVpnFeatures(settings));
            }
        }

        private VpnFeatures CreateVpnFeatures(SettingsContract settings)
        {
            return new()
            {
                SplitTcp = settings.SplitTcp,
                NetShieldMode = settings.NetShieldMode,
                AllowNonStandardPorts = settings.AllowNonStandardPorts,
                PortForwarding = settings.PortForwarding,
                ModerateNat = settings.ModerateNat,
            };
        }

        private ServiceSettingsStateContract CreateServiceSettingsState()
        {
            return new(Map(_state));
        }

        private void VpnConnection_StateChanged(object sender, EventArgs<VpnState> e)
        {
            _state = e.Data;
            CallbackStateChanged(_state);
        }

        private void PortForwarding_StateChanged(object sender, EventArgs<PortForwardingState> e)
        {
            PortForwardingState state = e.Data;

            StringBuilder logMessage = new StringBuilder().Append("Callbacking PortForwarding " +
                $"state '{state.Status}' triggered at '{state.TimestampUtc}'");
            if (state.MappedPort?.MappedPort is not null)
            {
                TemporaryMappedPort mappedPort = state.MappedPort;
                logMessage.Append($", Port pair {mappedPort.MappedPort}, expiring in " +
                                  $"{mappedPort.Lifetime} around {mappedPort.ExpirationDateUtc}");
            }
            _logger.Info<ConnectionLog>(logMessage.ToString());

            Callback(callback => callback.OnPortForwardingStateChanged(Map(state)));
        }

        private PortForwardingStateContract Map(PortForwardingState state)
        {
            return new()
            {
                MappedPort = CreateTemporaryMappedPortContract(state.MappedPort),
                Status = (PortMappingStatusContract)state.Status,
                TimestampUtc = state.TimestampUtc
            };
        }

        private TemporaryMappedPortContract CreateTemporaryMappedPortContract(TemporaryMappedPort mappedPort)
        {
            if (mappedPort?.MappedPort is null)
            {
                return null;
            }

            return new()
            {
                InternalPort = mappedPort.MappedPort.InternalPort,
                ExternalPort = mappedPort.MappedPort.ExternalPort,
                Lifetime = mappedPort.Lifetime,
                ExpirationDateUtc = mappedPort.ExpirationDateUtc
            };
        }

        private VpnStateContract Map(VpnState state)
        {
            bool killSwitchEnabled = _killSwitch.ExpectedLeakProtectionStatus(state);
            if (!killSwitchEnabled)
            {
                _state = new VpnState(state.Status, state.Error, state.VpnProtocol);
            }

            return new VpnStateContract(
                Map(state.Status),
                Map(state.Error),
                state.RemoteIp,
                killSwitchEnabled,
                state.OpenVpnAdapter,
                Map(state.VpnProtocol),
                state.Label);
        }

        private ConnectionDetailsContract Map(ConnectionDetails connectionDetails)
        {
            return new ConnectionDetailsContract
            {
                ClientIpAddress = connectionDetails.ClientIpAddress,
                ClientCountryIsoCode = connectionDetails.ClientCountryIsoCode,
                ServerIpAddress = connectionDetails.ServerIpAddress,
            };
        }

        private static VpnStatusContract Map(VpnStatus vpnStatus)
        {
            return (VpnStatusContract)vpnStatus;
        }

        private static VpnCredentials Map(VpnCredentialsContract credentials)
        {
            return credentials.ClientCertPem.IsNullOrEmpty() || credentials.ClientKeyPair == null
                ? new(credentials.Username, credentials.Password)
                : new(credentials.ClientCertPem, credentials.ClientKeyPair.ConvertBack());
        }

        private static IReadOnlyList<VpnHost> Map(IEnumerable<VpnHostContract> servers)
        {
            return servers.Select(Map).ToList();
        }

        private static VpnHost Map(VpnHostContract server)
        {
            return new(server.Name, server.Ip, server.Label, server.X25519PublicKey?.ConvertBack(), server.Signature);
        }

        private VpnConfig Map(VpnConfigContract config)
        {
            Dictionary<VpnProtocol, IReadOnlyCollection<int>> portConfig =
                config.Ports.ToDictionary(p => Map(p.Key), p => (IReadOnlyCollection<int>)p.Value.ToList());
            return new VpnConfig(
                new VpnConfigParameters
                {
                    Ports = portConfig,
                    CustomDns = config.CustomDns,
                    SplitTunnelMode = config.SplitTunnelMode,
                    SplitTunnelIPs = config.SplitTunnelIPs,
                    OpenVpnAdapter = _serviceSettings.OpenVpnAdapter,
                    VpnProtocol = Map(config.VpnProtocol),
                    PreferredProtocols = Map(config.PreferredProtocols),
                    ModerateNat = config.ModerateNat,
                    NetShieldMode = config.NetShieldMode,
                    SplitTcp = config.SplitTcp,
                    AllowNonStandardPorts = config.AllowNonStandardPorts,
                    PortForwarding = config.PortForwarding,
                });
        }

        private IList<VpnProtocol> Map(IList<VpnProtocolContract> protocols)
        {
            return protocols.Select(Map).ToList();
        }

        private VpnProtocol Map(VpnProtocolContract contract)
        {
            return contract switch
            {
                VpnProtocolContract.OpenVpnTcp => VpnProtocol.OpenVpnTcp,
                VpnProtocolContract.OpenVpnUdp => VpnProtocol.OpenVpnUdp,
                VpnProtocolContract.WireGuard => VpnProtocol.WireGuard,
                _ => VpnProtocol.Smart,
            };
        }

        private VpnProtocolContract Map(VpnProtocol protocol)
        {
            return protocol switch
            {
                VpnProtocol.OpenVpnTcp => VpnProtocolContract.OpenVpnTcp,
                VpnProtocol.OpenVpnUdp => VpnProtocolContract.OpenVpnUdp,
                VpnProtocol.WireGuard => VpnProtocolContract.WireGuard,
                VpnProtocol.Smart => VpnProtocolContract.Smart,
                _ => throw new NotImplementedException("VpnProtocol has an unknown value."),
            };
        }

        private static InOutBytesContract Map(InOutBytes bytes)
        {
            return new(bytes.BytesIn, bytes.BytesOut);
        }

        private static VpnError Map(VpnErrorTypeContract errorType)
        {
            return (VpnError)errorType;
        }

        private static VpnErrorTypeContract Map(VpnError errorType)
        {
            return (VpnErrorTypeContract)errorType;
        }
    }
}