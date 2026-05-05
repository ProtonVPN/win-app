/*
 * Copyright (c) 2025 Proton AG
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.WireGuardDriver;

namespace ProtonVPN.Vpn.WireGuard;

public class NtTrafficManager : INtTrafficManager
{
    private readonly string _adapterName;
    private readonly ILogger _logger;

    public NtTrafficManager(IStaticConfiguration config, ILogger logger)
    {
        _adapterName = config.WireGuard.ConfigFileName;
        _logger = logger;
    }

    public async IAsyncEnumerable<NetworkTraffic> WatchTrafficAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Adapter adapter;
        try
        {
            adapter = new Adapter(_adapterName);
        }
        catch (Win32Exception e)
        {
            _logger.Error<AppLog>("Failed to open adapter.", e);
            yield break;
        }

        using (adapter)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ulong rx = 0;
                ulong tx = 0;

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
                    // Can be safely ignored as it's only thrown when WireGuard Service is stopped due to the app exit
                    // or computer is put to sleep.
                }

                yield return new NetworkTraffic(rx, tx);

                try
                {
                    await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    yield break;
                }
            }
        }
    }
}
