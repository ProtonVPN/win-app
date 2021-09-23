/*
 * Copyright (c) 2021 Proton Technologies AG
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
using ProtonVPN.Common;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.WireGuard
{
    internal class StatusManager
    {
        private const int SkipLogCharacters = 27;

        private readonly ILogger _logger;
        private readonly RingLogger _ringLogger;
        private readonly SingleAction _receiveLogsAction;
        private VpnError _lastError = VpnError.None;

        public StatusManager(ILogger logger, string logPath)
        {
            _logger = logger;
            _ringLogger = new RingLogger(logPath);
            _receiveLogsAction = new SingleAction(ReceiveLogsAction);
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;

        public void Start()
        {
            _ringLogger.Start();
            _receiveLogsAction.Run();
        }

        public void Stop()
        {
            _receiveLogsAction.Cancel();
            _ringLogger.Stop();
        }

        private async Task ReceiveLogsAction(CancellationToken cancellationToken)
        {
            uint cursor = RingLogger.CursorAll;

            while (true)
            {
                List<string> lines = _ringLogger.FollowFromCursor(ref cursor);
                foreach (string line in lines)
                {
                    _logger.Info(GetFormattedMessage(line));

                    if (line.Contains("Interface up"))
                    {
                        InvokeStateChange(VpnStatus.Connected);
                    }
                    else if (line.Contains("Shutting down"))
                    {
                        InvokeStateChange(VpnStatus.Disconnected, _lastError);
                        _lastError = VpnError.None;
                    }
                    else if (line.Contains("Could not install driver"))
                    {
                        _lastError = VpnError.NoTapAdaptersError;
                    }
                }

                try
                {
                    Thread.Sleep(300);
                }
                catch
                {
                    break;
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private void InvokeStateChange(VpnStatus status, VpnError error = VpnError.None)
        {
            StateChanged?.Invoke(this, new EventArgs<VpnState>(new VpnState(status, error, VpnProtocol.WireGuard)));
        }

        private string GetFormattedMessage(string message)
        {
            return message.Length > SkipLogCharacters
                ? message.Substring(SkipLogCharacters, message.Length - SkipLogCharacters).Trim()
                : message;
        }
    }
}