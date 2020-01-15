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

using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Vpn;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ProtonVPN.Core.Service
{
    internal class MonitoredVpnService : IVpnStateAware, IService
    {
        private VpnStatus _vpnStatus;

        private readonly VpnServiceWrapper _service;
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        public MonitoredVpnService(Common.Configuration.Config appConfig, VpnServiceWrapper service)
        {
            _service = service;

            _timer.Interval = appConfig.ServiceCheckInterval;
            _timer.Tick += OnTimerTick;
        }

        public string Name => _service.Name;

        public event EventHandler<string> ServiceStartedHandler
        {
            add => _service.ServiceStartedHandler += value;
            remove => _service.ServiceStartedHandler -= value;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (_vpnStatus == VpnStatus.Disconnected)
            {
                return;
            }

            if (!IsRunning())
            {
                Start();
            }
        }

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

        public bool IsRunning()
        {
            return _service.IsRunning();
        }

        public Result Start()
        {
            return _service.Start();
        }

        public Result Stop()
        {
            return _service.Stop();
        }
    }
}
