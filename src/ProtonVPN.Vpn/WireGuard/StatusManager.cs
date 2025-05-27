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
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.ProtocolLogs;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.WireGuard;

public class StatusManager
{
    private const int SKIP_LOG_CHARACTERS = 27;
    private const int MAX_SOCKET_ERRORS = 5;
    private const string NT_HANDSHAKE_SUCCESS_MESSAGE = "Receiving handshake response from peer";
    private const string WINTUN_HANDSHAKE_SUCCESS_MESSAGE = "Received handshake response";

    private readonly ILogger _logger;
    private readonly RingLogger _ringLogger;
    private readonly SingleAction _receiveLogsAction;
    private VpnError _lastError = VpnError.None;
    private bool _isHandshakeResponseHandled;
    private int _socketErrorCount;

    public StatusManager(ILogger logger, string logPath)
    {
        _logger = logger;
        _ringLogger = new RingLogger(logPath);
        _receiveLogsAction = new SingleAction(ReceiveLogsActionAsync);
    }

    public event EventHandler<EventArgs<VpnState>> StateChanged;

    public void Start()
    {
        _socketErrorCount = 0;
        _ringLogger.Start();
        _isHandshakeResponseHandled = false;
        _receiveLogsAction.Run();
    }

    public void Stop()
    {
        _receiveLogsAction.Cancel();
        _socketErrorCount = 0;
        _isHandshakeResponseHandled = false;
        _ringLogger.Stop();
    }

    private async Task ReceiveLogsActionAsync(CancellationToken cancellationToken)
    {
        uint cursor = RingLogger.CursorAll;

        while (true)
        {
            List<string> lines = _ringLogger.FollowFromCursor(ref cursor);
            foreach (string line in lines)
            {
                _logger.Info<ProtocolLog>(GetFormattedMessage(line));

                bool isHandshakeSuccess = line.Contains(NT_HANDSHAKE_SUCCESS_MESSAGE) ||
                                          line.Contains(WINTUN_HANDSHAKE_SUCCESS_MESSAGE);
                if (isHandshakeSuccess && !_isHandshakeResponseHandled)
                {
                    _logger.Info<ConnectConnectedLog>("Invoking connected state after receiving successful handshake response.");
                    InvokeStateChange(VpnStatus.Connected);
                    _isHandshakeResponseHandled = true;
                }
                else if (line.Contains("Shutting down"))
                {
                    InvokeStateChange(VpnStatus.Disconnected, _lastError);
                    _lastError = VpnError.None;
                }
                else if (line.Contains("The RPC server is unavailable"))
                {
                    _lastError = VpnError.RpcServerUnavailable;
                }
                else if (line.Contains("Could not install driver"))
                {
                    _lastError = VpnError.NoTapAdaptersError;
                }
                else if (line.Contains("Unable to configure adapter network settings: unable to set ips: The object already exists"))
                {
                    _lastError = VpnError.WireGuardAdapterInUseError;
                }
                else if (line.Contains("interface has Forwarding/WeakHostSend enabled"))
                {
                    InvokeStateChange(VpnStatus.Disconnected, VpnError.InterfaceHasForwardingEnabled);
                }
                else if (line.Contains("SOCKET ERROR:"))
                {
                    if (_socketErrorCount >= MAX_SOCKET_ERRORS)
                    {
                        _logger.Info<ConnectConnectedLog>($"Invoking disconnected state after {MAX_SOCKET_ERRORS} socket errors.");
                        _socketErrorCount = 0;
                        InvokeStateChange(VpnStatus.Disconnected, VpnError.Unknown);
                    }
                    else
                    {
                        _socketErrorCount++;
                    }
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
        StateChanged?.Invoke(this, new EventArgs<VpnState>(new VpnState(status, error, VpnProtocol.WireGuardUdp)));
    }

    private string GetFormattedMessage(string message)
    {
        return message.Length > SKIP_LOG_CHARACTERS
            ? message.Substring(SKIP_LOG_CHARACTERS, message.Length - SKIP_LOG_CHARACTERS).Trim()
            : message;
    }
}