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
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Events;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppServiceLogs;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectionLogs;
using ProtonVPN.Common.Logging.Categorization.Events.OperatingSystemLogs;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Service;
using ProtonVPN.Common.ServiceModel.Server;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service
{
    internal partial class VpnService : ServiceBase
    {
        private readonly ILogger _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly IConfiguration _config;
        private readonly IOsProcesses _osProcesses;
        private readonly IVpnConnection _vpnConnection;
        private readonly List<ServiceHostFactory> _serviceHostsFactories;
        private readonly List<SafeServiceHost> _hosts;
        private readonly Ipv6 _ipv6;
        private bool _isConnected;

        public VpnService(
            ILogger logger,
            IEventPublisher eventPublisher,
            IConfiguration config,
            IOsProcesses osProcesses,
            IEnumerable<ServiceHostFactory> serviceHostsFactories,
            IVpnConnection vpnConnection,
            Ipv6 ipv6)
        {
            _logger = logger;
            _eventPublisher = eventPublisher;
            _config = config;
            _osProcesses = osProcesses;
            _serviceHostsFactories = new(serviceHostsFactories);
            _vpnConnection = vpnConnection;
            _ipv6 = ipv6;
            _vpnConnection.StateChanged += OnVpnStateChanged;

            _hosts = new();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                foreach (ServiceHostFactory factory in _serviceHostsFactories)
                {
                    SafeServiceHost host = factory.Create();
                    host.Open();
                    _hosts.Add(host);
                }
            }
            catch (Exception ex)
            {
                _logger.Error<AppServiceStartFailedLog>("An error occurred when starting VPN Service.", ex);
                LogEvent($"OnStart: {ex}");
                _eventPublisher.CaptureError(ex);
            }
        }

        protected override void OnStop()
        {
            try
            {
                _logger.Info<AppServiceStopLog>("Service is stopping");
                LogEvent("Service is stopping");

                _vpnConnection.Disconnect();
                StopWireGuardService();

                if (!_ipv6.Enabled)
                {
                    _ipv6.Enable();
                }

                foreach (SafeServiceHost host in _hosts.ToList())
                {
                    _hosts.Remove(host);
                    host.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.Error<AppServiceStopFailedLog>("An error occurred when stopping VPN Service.", ex);
                LogEvent($"OnStop: {ex}");
                _eventPublisher.CaptureError(ex);
            }
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            _logger.Info<OperatingSystemLog>($"Power status changed to {powerStatus}");
            if (powerStatus == PowerBroadcastStatus.ResumeSuspend && _isConnected)
            {
                _logger.Info<ConnectionLog>("Disconnecting due to resume from sleep.");
                _vpnConnection.Disconnect(VpnError.Unknown);
            }

            return true;
        }

        private void StopWireGuardService()
        {
            SystemService wireGuardService = new(_config.WireGuard.ServiceName, _osProcesses);
            if (wireGuardService.Running())
            {
                wireGuardService.StopAsync(new CancellationToken()).Wait();
            }
        }

        private void LogEvent(string message)
        {
            try
            {
                EventLog.WriteEntry(message.Replace('%', '_'));
            }
            catch (Exception e) when (e is InvalidOperationException or Win32Exception)
            {
            }
        }

        private void OnVpnStateChanged(object sender, Common.EventArgs<VpnState> e)
        {
            _isConnected = e.Data.Status == VpnStatus.Connected;
        }
    }
}