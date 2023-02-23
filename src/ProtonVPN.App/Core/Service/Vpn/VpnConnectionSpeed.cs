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
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnConnectionSpeed: IVpnStateAware
    {
        private readonly SerialTaskQueue _lock = new();
        private readonly IVpnServiceManager _vpnServiceManager;
        private readonly DispatcherTimer _timer;

        private InOutBytes _total;
        private InOutBytes _speed;

        public VpnConnectionSpeed(IVpnServiceManager vpnServiceManager)
        {
            _vpnServiceManager = vpnServiceManager;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
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

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (e.State.Status == VpnStatus.ActionRequired)
            {
                return Task.CompletedTask;
            }

            if (e.State.Status == VpnStatus.Connected)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }

            return Task.CompletedTask;
        }

        private async void UpdateSpeed(object sender, EventArgs e)
        {
            InOutBytes total;
            using (await _lock.Lock())
            {
                try
                {
                    total = await _vpnServiceManager.Total();
                }
                catch (CommunicationException)
                {
                    return;
                }
                catch (TimeoutException)
                {
                    return;
                }
            }

            _speed = total - _total;
            _total = total;
        }
    }
}