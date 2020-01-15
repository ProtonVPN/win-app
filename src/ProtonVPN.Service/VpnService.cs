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

using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Service;
using ProtonVPN.Common.ServiceModel.Server;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;
using ProtonVPN.Vpn.Common;
using Sentry;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;

namespace ProtonVPN.Service
{
    internal partial class VpnService : ServiceBase
    {
        private readonly ILogger _logger;
        private readonly IVpnConnection _vpnConnection;
        private readonly List<ServiceHostFactory> _serviceHostsFactories;
        private readonly List<SafeServiceHost> _hosts;
        private readonly Ipv6 _ipv6;
        private readonly IServiceSettings _serviceSettings;

        public VpnService(
            ILogger logger,
            IEnumerable<ServiceHostFactory> serviceHostsFactories,
            IVpnConnection vpnConnection,
            Ipv6 ipv6,
            IServiceSettings serviceSettings)
        {
            _serviceSettings = serviceSettings;
            _ipv6 = ipv6;
            _logger = logger;
            _vpnConnection = vpnConnection;
            _serviceHostsFactories = new List<ServiceHostFactory>(serviceHostsFactories);
            _hosts = new List<SafeServiceHost>();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                foreach (var factory in _serviceHostsFactories)
                {
                    var host = factory.Create();
                    host.Open();
                    _hosts.Add(host);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                LogEvent($"OnStart: {ex}");
                SentrySdk.WithScope(scope =>
                {
                    scope.Level = SentryLevel.Error;
                    scope.SetTag("captured_in", "Service_OnStart");
                    SentrySdk.CaptureException(ex);
                });
            }
        }

        protected override void OnStop()
        {
            try
            {
                LogEvent("Service is stopping");
                _vpnConnection.Disconnect();
                if (!_ipv6.Enabled && _serviceSettings.Ipv6LeakProtection)
                {
                    _ipv6.Enable();
                }

                foreach (var host in _hosts.ToList())
                {
                    host.Dispose();
                    _hosts.Remove(host);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                LogEvent($"OnStop: {ex}");
                SentrySdk.WithScope(scope =>
                {
                    scope.Level = SentryLevel.Error;
                    scope.SetTag("captured_in", "Service_OnStop");
                    SentrySdk.CaptureException(ex);
                });
            }
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            _logger.Info($"Power status changed to {powerStatus}");
            return true;
        }

        private void LogEvent(string message)
        {
            try
            {
                EventLog.WriteEntry(message.Replace('%', '_'));
            }
            catch (Exception e) when (e is InvalidOperationException || e is Win32Exception)
            {
            }
        }
    }
}
