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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Common.Legacy.Go;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.LocalAgentLogs;
using ProtonVPN.Common.Legacy.NetShield;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Vpn.LocalAgent.Contracts;
using ProtonVPN.Vpn.NetShield;

namespace ProtonVPN.Vpn.LocalAgent
{
    internal class EventReceiver
    {
        private readonly ILogger _logger;
        private readonly INetShieldStatisticEventManager _netShieldStatisticEventManager;

        private Task _loggerTask;
        private ConnectionDetails _connectionDetails;
        private CancellationTokenSource _cancellationTokenSource;

        public EventReceiver(ILogger logger, INetShieldStatisticEventManager netShieldStatisticEventManager)
        {
            _logger = logger;
            _netShieldStatisticEventManager = netShieldStatisticEventManager;
        }

        public event EventHandler<EventArgs<LocalAgentState>> StateChanged;
        public event EventHandler<LocalAgentErrorArgs> ErrorOccurred;
        public event EventHandler<ConnectionDetails> ConnectionDetailsChanged;

        public void Start()
        {
            _cancellationTokenSource = new();
            _loggerTask = Task.Factory.StartNew(() =>
            {
                string message;

                do
                {
                    GoBytes e = PInvoke.GetEvent();
                    message = e.ConvertToString();
                    EventContract eventContract = GetEventContract(message);
                    if (eventContract != null)
                    {
                        HandleEvent(eventContract);
                    }
                } while (!string.IsNullOrEmpty(message));
            }, _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            if (_loggerTask is { IsCompleted: false })
            {
                _cancellationTokenSource.Cancel();
            }
        }

        public void RequestConnectionDetails()
        {
            SendConnectionDetails(_connectionDetails);
        }

        private EventContract GetEventContract(string message)
        {
            try
            {
                return JsonConvert.DeserializeObject<EventContract>(message);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private void HandleEvent(EventContract e)
        {
            switch (e.EventType)
            {
                case "log":
                    _logger.Info<LocalAgentLog>(e.Log);
                    break;
                case "state":
                    HandleStateMessage(e.State);
                    break;
                case "status":
                    HandleStatusMessage(e);
                    break;
                case "error":
                    HandleError(e);
                    break;
                case "stats":
                    HandleStats(e);
                    break;
            }
        }

        private void HandleStats(EventContract eventContract)
        {
            Dictionary<string, Dictionary<string, long>> featuresStatistics;
            try
            {
                featuresStatistics = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, long>>>(
                    eventContract.FeaturesStatistics);
            }
            catch (Exception ex)
            {
                _logger.Error<LocalAgentErrorLog>($"Failed to deserialize JSON object " +
                    $"'{eventContract.FeaturesStatistics}'.", ex);
                return;
            }
            if (featuresStatistics is not null &&
                featuresStatistics.TryGetValue("netshield-level", out Dictionary<string, long> netShieldStats))
            {
                OnNetShieldStatsEvent(netShieldStats);
            }
        }

        private void OnNetShieldStatsEvent(Dictionary<string, long> eventValue)
        {
            NetShieldStatistic netShieldStatistic = new();
            if (eventValue != null)
            {
                netShieldStatistic.NumOfMaliciousUrlsBlocked = eventValue.TryGetValue("DNSBL/1b", out long v1b) ? v1b : 0;
                netShieldStatistic.NumOfAdvertisementUrlsBlocked = eventValue.TryGetValue("DNSBL/2a", out long v2a) ? v2a : 0;
                netShieldStatistic.NumOfTrackingUrlsBlocked = eventValue.TryGetValue("DNSBL/2b", out long v2b) ? v2b : 0;
            }
            _netShieldStatisticEventManager.Invoke(this, netShieldStatistic);
        }

        private void HandleStatusMessage(EventContract e)
        {
            if (e.ConnectionDetails is not null)
            {
                _connectionDetails = new ConnectionDetails
                {
                    ClientIpAddress = e.ConnectionDetails?.DeviceIp,
                    ClientCountryIsoCode = e.ConnectionDetails?.DeviceCountry,
                    ServerIpAddress = new()
                    {
                        Ipv4Address = e.ConnectionDetails?.ServerIpv4Address,
                        Ipv6Address = e.ConnectionDetails?.ServerIpv6Address,
                    }
                };
                SendConnectionDetails(_connectionDetails);
            }
        }

        private void SendConnectionDetails(ConnectionDetails connectionDetails)
        {
            if (connectionDetails is not null)
            {
                ConnectionDetailsChanged?.Invoke(this, connectionDetails);
            }
        }

        private void HandleError(EventContract e)
        {
            VpnError error = Enum.IsDefined(typeof(VpnError), e.Code) ? (VpnError)e.Code : VpnError.Unknown;
            InvokeErrorEvent(new LocalAgentErrorArgs(error, e.Desc));
        }

        private void HandleStateMessage(string message)
        {
            _logger.Info<LocalAgentStateChangeLog>("Local agent: state changed to " + message);

            LocalAgentState? state = message.ToEnumOrNull<LocalAgentState>();
            if (state.HasValue)
            {
                InvokeStateChanged(state.Value);
            }
            else
            {
                _logger.Error<LocalAgentStateChangeLog>("Local agent: unknown state " + message);
            }
        }

        private void InvokeStateChanged(LocalAgentState state)
        {
            StateChanged?.Invoke(this, new EventArgs<LocalAgentState>(state));
        }

        private void InvokeErrorEvent(LocalAgentErrorArgs args)
        {
            ErrorOccurred?.Invoke(this, args);
        }
    }
}