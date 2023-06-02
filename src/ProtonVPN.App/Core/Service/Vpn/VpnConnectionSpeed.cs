﻿/*
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
using System.ServiceModel;
using System.Threading.Tasks;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnConnectionSpeed: IVpnStateAware
    {
        private readonly SerialTaskQueue _lock = new();
        private readonly IVpnServiceManager _vpnServiceManager;
        private readonly ISchedulerTimer _timer;

        private InOutBytes _total;
        private InOutBytes _speed;

        public VpnConnectionSpeed(IVpnServiceManager vpnServiceManager,
            IScheduler scheduler)
        {
            _vpnServiceManager = vpnServiceManager;

            _timer = scheduler.Timer();
            _timer.IsEnabled = false;
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += UpdateSpeed;
        }

        public VpnSpeed Speed()
        {
            return new(Math.Max(0, _speed.BytesIn), Math.Max(0, _speed.BytesOut));
        }

        public double TotalDownloaded()
        {
            return _total.BytesIn;
        }

        public double TotalUploaded()
        {
            return _total.BytesOut;
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (e.State.Status == VpnStatus.ActionRequired)
            {
                return;
            }

            if (e.State.Status == VpnStatus.Connected)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }

        private async void UpdateSpeed(object sender, EventArgs e)
        {
            InOutBytes total;
            using (await _lock.Lock())
            {
                total = await _vpnServiceManager.GetTrafficBytes();
            }

            _speed = total - _total;
            _total = total;
        }
    }
}