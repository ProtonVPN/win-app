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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.ProtocolLogs;

namespace ProtonVPN.Vpn.WireGuard;

public class WireGuardStateMonitor : IWireGuardStateMonitor
{
    private const int SKIP_LOG_CHARACTERS = 27;
    private const int MAX_SOCKET_ERRORS = 5;
    private const string NT_HANDSHAKE_SUCCESS_MESSAGE = "Receiving handshake response from peer";
    private const string WINTUN_HANDSHAKE_SUCCESS_MESSAGE = "Received handshake response";
    private const string ADAPTER_IN_USE_MESSAGE = "Unable to configure adapter network settings: unable to set ips: The object already exists";

    private readonly ILogger _logger;
    private readonly RingLogger _ringLogger;
    private VpnError _lastError = VpnError.None;
    private bool _isHandshakeResponseHandled;
    private int _socketErrorCount;

    public WireGuardStateMonitor(ILogger logger, IStaticConfiguration config)
    {
        _logger = logger;
        _ringLogger = new RingLogger(config.WireGuard.LogFilePath);
    }

    public async IAsyncEnumerable<VpnState> WatchStatesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _socketErrorCount = 0;
        _lastError = VpnError.None;
        _ringLogger.Start();
        _isHandshakeResponseHandled = false;

        await foreach (VpnState state in ReceiveLogsAsync(cancellationToken).WithCancellation(cancellationToken))
        {
            yield return state;
        }
    }

    private async IAsyncEnumerable<VpnState> ReceiveLogsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        uint cursor = RingLogger.CursorAll;
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(300));

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                List<string> lines = _ringLogger.FollowFromCursor(ref cursor);
                foreach (VpnState state in CreateStates(lines))
                {
                    yield return state;
                }

                if (!await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
                {
                    yield break;
                }
            }
        }
        finally
        {
            _ringLogger.Stop();
        }
    }

    private List<VpnState> CreateStates(List<string> lines)
    {
        bool isAdapterInUse = IsAdapterInUse(lines);
        List<VpnState> states = [];

        foreach (string line in lines)
        {
            _logger.Info<WireGuardProtocolLog>(GetFormattedMessage(line));

            if (TryCreateState(line, out VpnState? vpnState) && vpnState is not null)
            {
                // When a native WireGuard client is connected, we receive "Startup complete" and then immediately fail with
                // "unable to set ips: The object already exists" before shutting down with WireGuardAdapterInUseError.
                // The interface never actually comes up, so we don't want to send AssigningIp state in this case.
                if (isAdapterInUse && vpnState.Status == VpnStatus.AssigningIp)
                {
                    continue;
                }

                states.Add(vpnState);
            }
        }

        return states;
    }

    private static bool IsAdapterInUse(List<string> lines)
    {
        foreach (string line in lines)
        {
            if (line.Contains(ADAPTER_IN_USE_MESSAGE))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryCreateState(string line, out VpnState? vpnState)
    {
        vpnState = null;

        bool isHandshakeSuccess = line.Contains(NT_HANDSHAKE_SUCCESS_MESSAGE) ||
                                  line.Contains(WINTUN_HANDSHAKE_SUCCESS_MESSAGE);
        if (isHandshakeSuccess && !_isHandshakeResponseHandled)
        {
            _logger.Info<ConnectConnectedLog>("Invoking connected state after receiving successful handshake response.");
            _isHandshakeResponseHandled = true;
            vpnState = CreateState(VpnStatus.Connected);
            return true;
        }

        if (line.Contains("Shutting down"))
        {
            vpnState = CreateState(VpnStatus.Disconnected, _socketErrorCount > 0 ? VpnError.Unknown : _lastError);
            _lastError = VpnError.None;
            _socketErrorCount = 0;
            return true;
        }

        if (line.Contains("The RPC server is unavailable"))
        {
            _lastError = VpnError.RpcServerUnavailable;
            return false;
        }

        if (line.Contains("Could not install driver"))
        {
            _lastError = VpnError.NoTapAdaptersError;
            return false;
        }

        if (line.Contains(ADAPTER_IN_USE_MESSAGE))
        {
            _lastError = VpnError.WireGuardAdapterInUseError;
            return false;
        }

        if (line.Contains("interface has Forwarding/WeakHostSend enabled"))
        {
            vpnState = CreateState(VpnStatus.Disconnected, VpnError.InterfaceHasForwardingEnabled);
            return true;
        }

        if (line.Contains("Startup complete"))
        {
            vpnState = CreateState(VpnStatus.AssigningIp);
            return true;
        }

        if (line.Contains("SOCKET ERROR:"))
        {
            if (_socketErrorCount >= MAX_SOCKET_ERRORS)
            {
                _logger.Info<ConnectConnectedLog>($"Invoking disconnected state after {MAX_SOCKET_ERRORS} socket errors.");
                _socketErrorCount = 0;
                vpnState = CreateState(VpnStatus.Disconnected, VpnError.Unknown);
                return true;
            }

            _socketErrorCount++;
        }

        return false;
    }

    private VpnState CreateState(VpnStatus status, VpnError error = VpnError.None)
    {
        return new VpnState(status, error, VpnProtocol.WireGuardUdp);
    }

    private string GetFormattedMessage(string message)
    {
        return message.Length > SKIP_LOG_CHARACTERS
            ? message.Substring(SKIP_LOG_CHARACTERS, message.Length - SKIP_LOG_CHARACTERS).Trim()
            : message;
    }
}