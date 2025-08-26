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
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.NetworkAdapters;

namespace ProtonVPN.Vpn.Connection;

internal class NetworkAdapterStatusWrapper : ISingleVpnConnection
{
    private readonly ILogger _logger;
    private readonly IIssueReporter _issueReporter;
    private readonly INetworkInterfaceLoader _networkInterfaceLoader;
    private readonly WintunAdapter _wintunAdapter;
    private readonly TapAdapter _tapAdapter;
    private readonly ISingleVpnConnection _origin;

    private VpnEndpoint _endpoint;
    private VpnCredentials _credentials;
    private VpnConfig _config;

    public NetworkAdapterStatusWrapper(
        ILogger logger,
        IIssueReporter issueReporter,
        INetworkInterfaceLoader networkInterfaceLoader,
        WintunAdapter wintunAdapter,
        TapAdapter tapAdapter,
        ISingleVpnConnection origin)
    {
        _logger = logger;
        _issueReporter = issueReporter;
        _wintunAdapter = wintunAdapter;
        _tapAdapter = tapAdapter;
        _networkInterfaceLoader = networkInterfaceLoader;
        _origin = origin;

        _origin.StateChanged += Origin_StateChanged;
    }

    public event EventHandler<EventArgs<VpnState>> StateChanged;
    public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
    {
        add => _origin.ConnectionDetailsChanged += value;
        remove => _origin.ConnectionDetailsChanged -= value;
    }

    public NetworkTraffic NetworkTraffic => _origin.NetworkTraffic;

    public void Connect(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
    {
        _endpoint = endpoint;
        _credentials = credentials;
        _config = config;

        if (_endpoint.VpnProtocol.IsWireGuard())
        {
            _logger.Info<ConnectLog>("WireGuard protocol selected. No network adapters to check.");
            Connect();
        }
        else
        {
            if (config.OpenVpnAdapter == OpenVpnAdapter.Tun)
            {
                _wintunAdapter.Create();
            }
            else
            {
                _tapAdapter.Create();
            }

            if (IsOpenVpnNetworkAdapterAvailable(config.OpenVpnAdapter))
            {
                _logger.Info<ConnectLog>("Preferred network adapter found. " +
                                         $"(Protocol: {_config.VpnProtocol}, OpenVpnAdapter: {_config.OpenVpnAdapter})");
                Connect();
            }
            else
            {
                HandleOpenVpnAdapterUnavailable();
            }
        }
    }

    private void Connect()
    {
        InvokeConnecting();
        _origin.Connect(_endpoint, _credentials, _config);
    }

    private void InvokeConnecting()
    {
        StateChanged?.Invoke(this,
            new EventArgs<VpnState>(new VpnState(
                VpnStatus.Connecting,
                VpnError.None,
                string.Empty,
                _endpoint.Server.Ip,
                _endpoint.Port,
                _endpoint.VpnProtocol,
                openVpnAdapter: _config.OpenVpnAdapter,
                label: _endpoint.Server.Label)));
    }

    private bool IsOpenVpnNetworkAdapterAvailable(OpenVpnAdapter? openVpnAdapter)
    {
        INetworkInterface networkInterface = _networkInterfaceLoader.GetByOpenVpnAdapter(openVpnAdapter);
        return networkInterface != null && networkInterface.Index != 0;
    }

    private void HandleOpenVpnAdapterUnavailable()
    {
        if (IsOpenVpnNetworkAdapterAvailable(_config.OpenVpnAdapter))
        {
            _logger.Info<ConnectLog>("OpenVPN network adapter successfully enabled " +
                $"({_endpoint.VpnProtocol} - {_config.OpenVpnAdapter}).");
            Connect();
        }
        else if (_config.OpenVpnAdapter == OpenVpnAdapter.Tun)
        {
            HandleNoTunError();
        }
        else
        {
            HandleNoTapError();
        }
    }

    private void HandleNoTunError()
    {
        _logger.Warn<NetworkUnavailableLog>("OpenVPN TUN network adapter not found. Checking if TAP is available.");
        if (IsOpenVpnNetworkAdapterAvailable(OpenVpnAdapter.Tap))
        {
            FallbackToTapAndConnect();
        }
        else
        {
            HandleNoTapError();
        }
    }

    private void FallbackToTapAndConnect()
    {
        _logger.Info<NetworkLog>("OpenVPN TAP network adapter found. Connecting using TAP instead of TUN.");
        _issueReporter.CaptureMessage("TUN adapter not found. Adapter changed to TAP.");
        _config.OpenVpnAdapter = OpenVpnAdapter.Tap;
        Connect();
    }

    private void HandleNoTapError()
    {
        _logger.Error<DisconnectTriggerLog>("OpenVPN TAP network adapter not found. Disconnecting.");

        // The state is directly invoked to Disconnected because:
        // - We don't need a disconnecting procedure as this code is right after pinging and before anything else
        // - The call to Disconnect right below will, most likely, not result in state changes as the VPN state
        //   in the steps ahead (Local Agent, Protocols, etc.) should be Disconnected already
        InvokeDisconnected();

        Disconnect(VpnError.NoTapAdaptersError);
    }

    private void InvokeDisconnected()
    {
        StateChanged?.Invoke(this,
            new EventArgs<VpnState>(new VpnState(
                VpnStatus.Disconnected,
                VpnError.NoTapAdaptersError,
                string.Empty,
                _endpoint.Server.Ip,
                _endpoint.Port,
                _endpoint.VpnProtocol,
                openVpnAdapter: _config.OpenVpnAdapter,
                label: _endpoint.Server.Label)));
    }

    public void Disconnect(VpnError error)
    {
        _origin.Disconnect(error);
    }

    public void SetFeatures(VpnFeatures vpnFeatures)
    {
        _origin.SetFeatures(vpnFeatures);
    }

    public void RequestNetShieldStats()
    {
        _origin.RequestNetShieldStats();
    }

    public void RequestConnectionDetails()
    {
        _origin.RequestConnectionDetails();
    }

    private void Origin_StateChanged(object sender, EventArgs<VpnState> e)
    {
        if (e.Data.Status == VpnStatus.Disconnected && e.Data.VpnProtocol.IsOpenVpn())
        {
            _wintunAdapter.Close();
        }
        else if (e.Data.Error != VpnError.None &&
                 e.Data.Error != VpnError.NoneKeepEnabledKillSwitch &&
                 e.Data.Status == VpnStatus.Disconnecting)
        {
            HandleVpnError(e.Data);
        }

        StateChanged?.Invoke(this, e);
    }

    private void HandleVpnError(VpnState vpnState)
    {
        switch (vpnState.VpnProtocol)
        {
            case VpnProtocol.WireGuardUdp:
            case VpnProtocol.WireGuardTcp:
            case VpnProtocol.WireGuardTls:
                HandleWireGuardError(vpnState);
                break;
            case VpnProtocol.OpenVpnUdp:
            case VpnProtocol.OpenVpnTcp:
                HandleOpenVpnError(vpnState);
                break;
            case VpnProtocol.Smart:
                HandleWireGuardError(vpnState);
                HandleOpenVpnError(vpnState);
                break;
        }
    }

    private void HandleWireGuardError(VpnState vpnState)
    {
        _logger.Warn<NetworkLog>($"Connection error '{vpnState.Error}' while using " +
            $"protocol '{vpnState.VpnProtocol}'.");
    }

    private void HandleOpenVpnError(VpnState vpnState)
    {
        _logger.Warn<NetworkLog>($"Connection error '{vpnState.Error}' while using " +
            $"protocol '{vpnState.VpnProtocol}'.");
    }
}