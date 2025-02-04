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
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Threading;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;

namespace ProtonVPN.Core.Service
{
    public class MonitoredVpnService : IMonitoredVpnService, IConcurrentService
    {
        private readonly DispatcherTimer _timer = new();
        private readonly VpnSystemService _service;
        private readonly IVpnManager _vpnManager;
        private readonly ILogger _logger;

        public MonitoredVpnService(
            IConfiguration appConfig,
            VpnSystemService service,
            IVpnManager vpnManager,
            ILogger logger)
        {
            _service = service;
            _vpnManager = vpnManager;
            _logger = logger;
            _timer.Interval = appConfig.ServiceCheckInterval;
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        public string Name => _service.Name;

        public ServiceControllerStatus? GetStatus() => _service.GetStatus();

        public bool IsRunning() => _service.IsRunning();

        public bool IsEnabled() => _service.IsEnabled();

        public void Enable() => _service.Enable();

        public Task<Result> StartAsync()
        {
            return _service.StartAsync();
        }

        public Task<Result> StopAsync() => _service.StopAsync();

        private void OnTimerTick(object sender, EventArgs e)
        {
            StartIfNotRunningAsync().Wait();
        }

        public async Task StartIfNotRunningAsync()
        {
            if (IsRunning())
            {
                return;
            }

            _logger.Warn<AppServiceStartLog>($"The service is not running. " +
                "Starting the service and reconnecting.");
            StartAsync();
            _vpnManager.GetStateAsync();
            _vpnManager.ReconnectAsync();
        }
    }
}