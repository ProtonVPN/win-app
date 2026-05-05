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
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Go;
using ProtonVPN.Common.Legacy.NetShield;
using ProtonVPN.Common.Legacy.Restrictions;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.LocalAgentLogs;
using ProtonVPN.Vpn.LocalAgent.Contracts;

namespace ProtonVPN.Vpn.LocalAgent;

public class LocalAgentEventReceiver : ILocalAgentEventReceiver
{
    private readonly ILogger _logger;

    private ConnectionDetails? _connectionDetails;

    public Channel<LocalAgentState> StateChannel { get; } = Channel.CreateUnbounded<LocalAgentState>();
    public Channel<ConnectionDetails> ConnectionDetailsChannel { get; } = Channel.CreateUnbounded<ConnectionDetails>();
    public Channel<VpnError> ErrorChannel { get; } = Channel.CreateUnbounded<VpnError>();
    public Channel<NetShieldStatistic> NetShieldStatsChannel { get; } = Channel.CreateUnbounded<NetShieldStatistic>();
    public Channel<RestrictionsList> RestrictionsChannel { get; } = Channel.CreateUnbounded<RestrictionsList>();

    public LocalAgentEventReceiver(ILogger logger)
    {
        _logger = logger;
    }

    public async Task WatchEventsAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            GoBytes e;
            try
            {
                e = PInvoke.GetEvent();
            }
            catch
            {
                break;
            }

            string message = e.ConvertToString();
            if (string.IsNullOrEmpty(message))
            {
                break;
            }

            // Even after cancellation, keep draining PInvoke.GetEvent(); otherwise Go's Close() can block
            // writing to its buffered event channel if it isn't being drained.
            if (cancellationToken.IsCancellationRequested)
            {
                continue;
            }

            LocalAgentEvent? eventContract = DeserializeMessage(message);
            if (eventContract != null)
            {
                try
                {
                    await HandleEventAsync(eventContract, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }
    }

    public async Task RequestConnectionDetailsAsync(CancellationToken cancellationToken)
    {
        await SendConnectionDetailsAsync(_connectionDetails, cancellationToken);
    }

    private LocalAgentEvent? DeserializeMessage(string message)
    {
        try
        {
            return JsonConvert.DeserializeObject<LocalAgentEvent>(message);
        }
        catch (JsonException ex)
        {
            _logger.Error<LocalAgentLog>($"Failed to deserialize local agent event: {message}", ex);
            return null;
        }
    }

    private async Task HandleEventAsync(LocalAgentEvent e, CancellationToken cancellationToken)
    {
        switch (e.EventType)
        {
            case "log":
                _logger.Info<LocalAgentLog>(e.Log);
                break;
            case "state":
                await HandleStateMessageAsync(e.State, cancellationToken);
                break;
            case "status":
                await HandleConnectionDetailsAsync(e, cancellationToken);
                break;
            case "error":
                await HandleErrorAsync(e, cancellationToken);
                break;
            case "stats":
                await HandleStatsAsync(e, cancellationToken);
                break;
            case "restrictions":
                await HandleRestrictionsAsync(e, cancellationToken);
                break;
        }
    }

    private async Task HandleStateMessageAsync(string message, CancellationToken cancellationToken)
    {
        LocalAgentState? state = message.ToEnumOrNull<LocalAgentState>();
        if (state.HasValue)
        {
            _logger.Info<LocalAgentStateChangeLog>($"State changed to {message}");
            await StateChannel.Writer.WriteAsync(state.Value, cancellationToken);
        }
        else
        {
            _logger.Warn<LocalAgentStateChangeLog>($"Unknown state {message}");
        }
    }

    private async Task HandleConnectionDetailsAsync(LocalAgentEvent e, CancellationToken cancellationToken)
    {
        if (e.ConnectionDetails is not null)
        {
            _connectionDetails = new ConnectionDetails
            {
                ClientIpAddress = e.ConnectionDetails?.DeviceIp,
                ClientCountryIsoCode = e.ConnectionDetails?.DeviceCountry,
                ServerIpAddress = new()
                {
                    Ipv4Address = e.ConnectionDetails?.ServerIpv4Address ?? string.Empty,
                    Ipv6Address = e.ConnectionDetails?.ServerIpv6Address ?? string.Empty,
                }
            };

            await SendConnectionDetailsAsync(_connectionDetails, cancellationToken);
        }
    }

    private async Task SendConnectionDetailsAsync(ConnectionDetails? connectionDetails, CancellationToken cancellationToken)
    {
        if (connectionDetails is not null)
        {
            await ConnectionDetailsChannel.Writer.WriteAsync(connectionDetails, cancellationToken);
        }
    }

    private async Task HandleErrorAsync(LocalAgentEvent e, CancellationToken cancellationToken)
    {
        VpnError error = Enum.IsDefined(typeof(VpnError), e.Code)
            ? (VpnError)e.Code
            : VpnError.Unknown;

        await ErrorChannel.Writer.WriteAsync(error, cancellationToken);
    }

    private async Task HandleStatsAsync(LocalAgentEvent eventContract, CancellationToken cancellationToken)
    {
        try
        {
            Dictionary<string, Dictionary<string, long>>? featuresStatistics =
                JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, long>>>(eventContract.FeaturesStatistics);

            if (featuresStatistics is not null &&
                featuresStatistics.TryGetValue("netshield-level", out Dictionary<string, long>? netShieldStats))
            {
                await OnNetShieldStatsEventAsync(netShieldStats, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.Error<LocalAgentErrorLog>($"Failed to deserialize JSON object " +
                $"'{eventContract.FeaturesStatistics}'.", ex);
        }
    }

    private async Task HandleRestrictionsAsync(LocalAgentEvent eventContract, CancellationToken cancellationToken)
    {
        try
        {
            List<Restriction> restrictions = eventContract.Restrictions
                .Select(r => Enum.TryParse(r, true, out Restriction val) ? (Restriction?)val : null)
                .OfType<Restriction>()
                .ToList();

            if (restrictions.Count == 0)
            {
                return;
            }

            await RestrictionsChannel.Writer.WriteAsync(new RestrictionsList()
            {
                Restrictions = restrictions
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error<LocalAgentErrorLog>($"Failed to process restrictions.", ex);
        }
    }

    private async Task OnNetShieldStatsEventAsync(Dictionary<string, long> eventValue, CancellationToken cancellationToken)
    {
        NetShieldStatistic netShieldStatistic = new();
        if (eventValue != null)
        {
            netShieldStatistic.NumOfMaliciousUrlsBlocked = eventValue.TryGetValue("DNSBL/1b", out long v1b) ? v1b : 0;
            netShieldStatistic.NumOfAdvertisementUrlsBlocked = eventValue.TryGetValue("DNSBL/2a", out long v2a) ? v2a : 0;
            netShieldStatistic.NumOfTrackingUrlsBlocked = eventValue.TryGetValue("DNSBL/2b", out long v2b) ? v2b : 0;
            netShieldStatistic.NumOfAdultContentUrlsBlocked = eventValue.TryGetValue("DNSBL/3a", out long v3a) ? v3a : 0;
        }

        await NetShieldStatsChannel.Writer.WriteAsync(netShieldStatistic, cancellationToken);
    }
}