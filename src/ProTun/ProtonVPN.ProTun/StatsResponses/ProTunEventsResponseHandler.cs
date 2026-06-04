/*
 * Copyright (c) 2026 Proton AG
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

using System.Threading.Channels;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.ProTun.Generated;
using static ProtonVPN.ProTun.Generated.Event;

namespace ProtonVPN.ProTun.StatsResponses;

public class ProTunEventsResponseHandler : IProTunEventsResponseHandler
{
    private readonly ILogger _logger;

    public Channel<NetworkTraffic> TrafficChannel { get; } = Channel.CreateUnbounded<NetworkTraffic>();

    private CancellationToken? _cancellationToken;

    public ProTunEventsResponseHandler(ILogger logger)
    {
        _logger = logger;
    }

    public void SetCancellationToken(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    public async void OnEvent(Event proTunEvent)
    {
        try
        {
            if (proTunEvent is ConnectionStats connectionStatsEvent)
            {
                await OnConnectionStatsEventAsync(connectionStatsEvent);
            }
        }
        catch (Exception ex)
        {
            _logger.Error<ConnectionLog>("Failed to handle ProTun event", ex);
        }
    }

    private async Task OnConnectionStatsEventAsync(ConnectionStats connectionStatsEvent)
    {
        NetworkTraffic traffic = new(connectionStatsEvent.receivedBytes, connectionStatsEvent.sentBytes);
        await InvokeTrafficUpdateAsync(traffic);
    }

    private async Task InvokeTrafficUpdateAsync(NetworkTraffic traffic)
    {
        try
        {
            CancellationToken? cancellationToken = _cancellationToken;
            if (cancellationToken is not null)
            {
                await TrafficChannel.Writer.WriteAsync(traffic, cancellationToken.Value);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}