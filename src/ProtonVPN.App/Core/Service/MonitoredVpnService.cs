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
using System.Threading.Tasks;
using System.Windows.Threading;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppServiceLogs;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core.Service
{
    internal class MonitoredVpnService : IVpnStateAware, IConcurrentService
    {
        private VpnStatus _vpnStatus;
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
        }

        public string Name => _service.Name;

        public bool Running() => _service.Running();

        public bool Enabled() => _service.Enabled();

        public void Enable() => _service.Enable();

        public Task<Result> StartAsync()
        {
            return _service.StartAsync();
        }

        public Task<Result> StopAsync() => _service.StopAsync();

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            if (!_timer.IsEnabled && _vpnStatus != VpnStatus.Disconnected)
            {
                _timer.Start();
            }

            if (_timer.IsEnabled && _vpnStatus == VpnStatus.Disconnected)
            {
                _timer.Stop();
            }

            return Task.CompletedTask;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (_vpnStatus == VpnStatus.Disconnected || _vpnStatus == VpnStatus.Disconnecting || Running())
            {
                return;
            }

            _logger.Warn<AppServiceStartLog>($"The service is not running and the VPN status is '{_vpnStatus}'. " +
                "Starting the service and reconnecting.");
            StartAsync();
            _vpnManager.ReconnectAsync();
        }
    }
}