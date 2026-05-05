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

using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Service.KillSwitch;
using ProtonVPN.Service.SplitTunneling;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Vpn.PortMapping;

namespace ProtonVPN.Service.StateMachine.SideEffects;

internal sealed class VpnStateSideEffects : IVpnStateSideEffects
{
    private readonly ILogger _logger;
    private readonly IKillSwitch _killSwitch;
    private readonly ISplitTunnel _splitTunnel;
    private readonly IIPv6Manager _ipv6Manager;
    private readonly IPortMappingProtocolClient _portMappingProtocolClient;

    private VpnStatus? _vpnStatus;
    private State? _state;

    public VpnStateSideEffects(
        ILogger logger,
        IKillSwitch killSwitch,
        ISplitTunnel splitTunnel,
        IIPv6Manager ipv6Manager,
        IPortMappingProtocolClient portMappingProtocolClient)
    {
        _logger = logger;
        _killSwitch = killSwitch;
        _splitTunnel = splitTunnel;
        _ipv6Manager = ipv6Manager;
        _portMappingProtocolClient = portMappingProtocolClient;
    }

    public async Task ApplyAsync(VpnState state, State stateMachineState)
    {
        await HandlePortForwardingAsync(state);

        if (_vpnStatus == state.Status && _state == stateMachineState)
        {
            return;
        }

        _vpnStatus = state.Status;
        _state = stateMachineState;

        switch (state.Status)
        {
            case VpnStatus.Connecting:
                _killSwitch.OnVpnConnecting(state);
                _splitTunnel.OnVpnConnecting(state);
                break;
            case VpnStatus.AssigningIp:
                _killSwitch.AssigningIp(state);
                break;
            case VpnStatus.Connected:
                _killSwitch.OnVpnConnected(state);
                _splitTunnel.OnVpnConnected(state);
                break;
            case VpnStatus.Disconnected:
                if (stateMachineState == State.Disconnected)
                {
                    _killSwitch.OnVpnDisconnected(state);
                }

                _splitTunnel.OnVpnDisconnected(state);
                break;
        }

        await _ipv6Manager.OnVpnStatusChangedAsync(state.Status);
    }

    public void UpdateSplitTunnelContext(SplitTunnelContext context)
    {
        _splitTunnel.UpdateContext(context);
    }

    private async Task HandlePortForwardingAsync(VpnState state)
    {
        _portMappingProtocolClient.SetVpnState(state);

        if (IsToStartPortMappingProtocolClient(state))
        {
            _logger.Debug<ConnectLog>("Requesting NAT-PMP client to start.");
            await _portMappingProtocolClient.StartAsync();
        }
        else if (IsToStopPortMappingProtocolClient(state))
        {
            await StopPortMappingProtocolClientAsync();
        }
    }

    private static bool IsToStartPortMappingProtocolClient(VpnState state)
    {
        return state.Status == VpnStatus.Connected && state.PortForwarding;
    }

    private static bool IsToStopPortMappingProtocolClient(VpnState state)
    {
        return state.Status != VpnStatus.Connected || !state.PortForwarding;
    }

    private async Task StopPortMappingProtocolClientAsync()
    {
        _logger.Debug<ConnectLog>("Requesting NAT-PMP client to stop.");
        await _portMappingProtocolClient.StopAsync();
    }
}