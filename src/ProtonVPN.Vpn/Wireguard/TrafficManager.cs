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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.WireGuardDriver;

namespace ProtonVPN.Vpn.WireGuard
{
    public class TrafficManager
    {
        private readonly string _adapterName;
        private readonly SingleAction _updateBytesTransferredAction;
        private readonly ILogger _logger;

        public event EventHandler<InOutBytes> TrafficSent;

        public TrafficManager(string adapterName, ILogger logger)
        {
            _adapterName = adapterName;
            _logger = logger;
            _updateBytesTransferredAction = new SingleAction(UpdateBytesTransferred);
        }

        public void Start()
        {
            _updateBytesTransferredAction.Run();
        }

        public void Stop()
        {
            _updateBytesTransferredAction.Cancel();
        }

        private async Task UpdateBytesTransferred(CancellationToken cancellationToken)
        {
            try
            {
                using Adapter adapter = new(_adapterName);
                while (!cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    double rx = 0;
                    double tx = 0;

                    try
                    {
                        Interface iface = adapter.GetConfiguration();
                        foreach (Peer peer in iface.Peers)
                        {
                            rx += peer.RxBytes;
                            tx += peer.TxBytes;
                        }
                    }
                    catch (Win32Exception)
                    {
                        //can be safely ignored as it's only thrown when WireGuard Service is stopped due to the app exit
                        //or computer is put to sleep.
                    }

                    TrafficSent?.Invoke(this, new InOutBytes(rx, tx));

                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (Win32Exception e)
            {
                _logger.Error<AppLog>("Failed to receive interface configuration.", e);
            }
        }
    }
}