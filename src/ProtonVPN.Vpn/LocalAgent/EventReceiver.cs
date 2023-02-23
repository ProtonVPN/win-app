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
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtonVPN.Common;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Go;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.LocalAgentLogs;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.LocalAgent.Contracts;

namespace ProtonVPN.Vpn.LocalAgent
{
    internal class EventReceiver
    {
        private Task _loggerTask;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;

        public EventReceiver(ILogger logger)
        {
            _logger = logger;
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
            }
        }

        private void HandleStatusMessage(EventContract e)
        {
            ConnectionDetailsChanged?.Invoke(this,
                new ConnectionDetails
                {
                    ClientIpAddress = e.ConnectionDetails?.DeviceIp,
                    ClientCountryIsoCode = e.ConnectionDetails?.DeviceCountry,
                    ServerIpAddress = e.ConnectionDetails?.ServerIpv4,
                });
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