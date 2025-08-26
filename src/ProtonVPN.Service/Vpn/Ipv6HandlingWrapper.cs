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
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.IPv6.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.IPv6Logs;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.OperatingSystems.Processes.Contracts;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.Vpn;

internal class Ipv6HandlingWrapper : IVpnConnection
{
    private const int MAX_FAKE_IPV6_ADDRESSES = 50;

    private readonly IIpv6 _ipv6;
    private readonly ILogger _logger;
    private readonly IFirewall _firewall;
    private readonly IServiceSettings _serviceSettings;
    private readonly IFakeIPv6AddressGenerator _fakeIPv6AddressGenerator;
    private readonly ICommandLineCaller _commandLineCaller;
    private readonly INetworkInterfaceLoader _networkInterfaceLoader;
    private readonly ISystemNetworkInterfaces _networkInterfaces;
    private readonly IVpnConnection _origin;

    private readonly SemaphoreSlim _networkSemaphore = new(1, 1);
    private readonly SemaphoreSlim _ipv6Semaphore = new(1, 1);

    private IReadOnlyList<VpnHost> _servers;
    private VpnConfig _config;
    private VpnCredentials _credentials;

    private bool _connectRequested;
    private bool _disconnectedReceived;
    private volatile bool _networkChanged;

    private VpnStatus? _vpnStatus = null;
    private VpnProtocol? _lastConnectedProtocol;

    private HashSet<NetworkAddress> _lastGlobalUnicastAddresses = [];
    private List<NetworkAddress> _lastFakeIpv6Addresses = [];

    public Ipv6HandlingWrapper(
        IIpv6 ipv6,
        ILogger logger,
        IFirewall firewall,
        IServiceSettings serviceSettings,
        IFakeIPv6AddressGenerator fakeIPv6AddressGenerator,
        ICommandLineCaller commandLineCaller,
        INetworkInterfaceLoader networkInterfaceLoader,
        ISystemNetworkInterfaces networkInterfaces,
        IObservableNetworkInterfaces observableNetworkInterfaces,
        IVpnConnection origin)
    {
        _ipv6 = ipv6;
        _logger = logger;
        _firewall = firewall;
        _serviceSettings = serviceSettings;
        _fakeIPv6AddressGenerator = fakeIPv6AddressGenerator;
        _commandLineCaller = commandLineCaller;
        _networkInterfaceLoader = networkInterfaceLoader;
        _networkInterfaces = networkInterfaces;
        _origin = origin;

        _origin.StateChanged += OnStateChangedAsync;

        observableNetworkInterfaces.NetworkInterfacesAdded += OnNetworkInterfacesAddedAsync;
        networkInterfaces.NetworkAddressChanged += OnNetworkAddressChangedAsync;
    }

    public event EventHandler<EventArgs<VpnState>> StateChanged;

    public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
    {
        add => _origin.ConnectionDetailsChanged += value;
        remove => _origin.ConnectionDetailsChanged -= value;
    }

    public NetworkTraffic NetworkTraffic => _origin.NetworkTraffic;

    public async void Connect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnCredentials credentials)
    {
        _servers = servers;
        _config = config;
        _credentials = credentials;

        _connectRequested = true;
        _disconnectedReceived = false;

        InvokeConnecting();

        if (_serviceSettings.IsIpv6Enabled)
        {
            await ConnectWithChaosAlgorithmAsync();
        }
        else
        {
            await ConnectDisablingIpv6Async();
        }
    }

    public void ResetConnection()
    {
        _origin.ResetConnection();
    }

    public void Disconnect(VpnError error)
    {
        _connectRequested = false;
        _disconnectedReceived = false;

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

    private async Task ConnectWithChaosAlgorithmAsync()
    {
        if (!_ipv6.IsEnabled)
        {
            await RunIpv6ActionAsync(_ipv6.EnableAsync);
        }

        if (!_serviceSettings.Ipv6LeakProtection)
        {
            _lastFakeIpv6Addresses.Clear();
            ConnectOrigin();
            return;
        }

        HashSet<NetworkAddress> globalUnicastAddresses = GetGlobalUnicastAddresses();

        if (globalUnicastAddresses.Count == 0)
        {
            _lastFakeIpv6Addresses.Clear();
            ConnectOrigin();
            return;
        }

        LogGuaAddresses(globalUnicastAddresses);

        _lastGlobalUnicastAddresses = globalUnicastAddresses;

        List<NetworkAddress> fakeIpv6Addresses = await _fakeIPv6AddressGenerator.GenerateAddressesAsync(
            _serviceSettings.Ipv6Fragments,
            globalUnicastAddresses.Select(a => a.ToString()).ToList(),
            MAX_FAKE_IPV6_ADDRESSES);

        if (fakeIpv6Addresses.Count == 0)
        {
            return;
        }

        _lastFakeIpv6Addresses = fakeIpv6Addresses;
        ConnectOrigin();
    }

    private void ConnectOrigin()
    {
        _connectRequested = false;
        _origin.Connect(_servers, _config, _credentials);
    }

    private async Task ConnectDisablingIpv6Async()
    {
        await _ipv6.EnableOnVPNInterfaceAsync();

        if (_ipv6.IsEnabled && _serviceSettings.Ipv6LeakProtection)
        {
            await RunIpv6ActionAsync(_ipv6.DisableAsync);
        }
        else if (!_ipv6.IsEnabled && !_serviceSettings.Ipv6LeakProtection)
        {
            await RunIpv6ActionAsync(_ipv6.EnableAsync);
        }

        _networkChanged = false;

        ConnectOrigin();
    }

    private async void OnStateChangedAsync(object sender, EventArgs<VpnState> e)
    {
        VpnState state = e.Data;

        if (state.Status == _vpnStatus)
        {
            return;
        }

        _vpnStatus = state.Status;
        _lastConnectedProtocol = state.Status == VpnStatus.Connected
            ? state.VpnProtocol
            : null;

        if (_connectRequested)
        {
            InvokeConnecting();
            return;
        }

        InvokeStateChanged(state);

        _disconnectedReceived = state.Status == VpnStatus.Disconnected;

        if (_disconnectedReceived)
        {
            await DisconnectedAsync();
        }

        if (_serviceSettings.IsIpv6Enabled)
        {
            switch (state.Status)
            {
                case VpnStatus.Connected when _lastFakeIpv6Addresses.Count > 0:
                    INetworkInterface tunnelInterface = GetTunnelInterface();
                    await AddInterfaceIpv6AddressesAsync(_lastFakeIpv6Addresses, tunnelInterface.Index);
                    break;
                case VpnStatus.Disconnected:
                    _lastFakeIpv6Addresses.Clear();
                    break;
            }
        }
    }

    private async Task DisconnectedAsync()
    {
        if (!_disconnectedReceived)
        {
            return;
        }

        if ((!_firewall.LeakProtectionEnabled || _serviceSettings.IsIpv6Enabled) && !_ipv6.IsEnabled)
        {
            _networkChanged = false;
            await RunIpv6ActionAsync(_ipv6.EnableAsync);
        }
        else
        {
            _disconnectedReceived = false;
        }
    }

    private async void OnNetworkInterfacesAddedAsync(object sender, EventArgs e)
    {
        if (_networkChanged || !_connectRequested)
        {
            return;
        }

        _networkChanged = false;

        if (!_ipv6.IsEnabled)
        {
            await RunIpv6ActionAsync(_ipv6.DisableAsync);
        }
    }

    private async void OnNetworkAddressChangedAsync(object sender, EventArgs e)
    {
        await _networkSemaphore.WaitAsync();

        try
        {
            if (_vpnStatus != VpnStatus.Connected)
            {
                return;
            }

            HashSet<NetworkAddress> globalUnicastAddresses = GetGlobalUnicastAddresses();
            if (_lastGlobalUnicastAddresses.SetEquals(globalUnicastAddresses))
            {
                return;
            }

            if (globalUnicastAddresses.Count > 0)
            {
                LogGuaAddresses(globalUnicastAddresses);
            }

            INetworkInterface tunnelInterface = GetTunnelInterface();

            if (globalUnicastAddresses.Count == 0 && _lastGlobalUnicastAddresses.Count > 0)
            {
                _logger.Info<IPv6Log>("No GUA addresses detected after network addresses changed. Clearing previuos fake IPv6 addresses.");

                _lastGlobalUnicastAddresses.Clear();

                await DeleteInterfaceIpv6AddressesAsync(_lastFakeIpv6Addresses, tunnelInterface.Index);
                _lastFakeIpv6Addresses.Clear();
                return;
            }

            _lastGlobalUnicastAddresses = globalUnicastAddresses;

            List<NetworkAddress> ipv6AddressesToRemove = _lastFakeIpv6Addresses.ToList();

            await ApplyFakeIpv6AddressesAsync(tunnelInterface.Index);
            await DeleteInterfaceIpv6AddressesAsync(ipv6AddressesToRemove, tunnelInterface.Index);
        }
        finally
        {
            _networkSemaphore.Release();
        }
    }

    private void LogGuaAddresses(HashSet<NetworkAddress> globalUnicastAddresses)
    {
        _logger.Debug<NetworkLog>($"GUA addresses detected: {string.Join(", ", globalUnicastAddresses)}");
    }

    private async Task ApplyFakeIpv6AddressesAsync(uint tunnelInterfaceIndex)
    {
        List<NetworkAddress> fakeIpv6Addresses = await _fakeIPv6AddressGenerator.GenerateAddressesAsync(
            _serviceSettings.Ipv6Fragments,
            _lastGlobalUnicastAddresses.Select(a => a.ToString()).ToList(),
            MAX_FAKE_IPV6_ADDRESSES);

        if (fakeIpv6Addresses.Count > 0)
        {
            await AddInterfaceIpv6AddressesAsync(fakeIpv6Addresses, tunnelInterfaceIndex);

            _lastFakeIpv6Addresses = fakeIpv6Addresses;
        }
    }

    private async Task AddInterfaceIpv6AddressesAsync(List<NetworkAddress> addresses, uint interfaceIndex)
    {
        _logger.Info<IPv6Log>($"Adding {addresses.Count} fake IPv6 addresses to interface with index {interfaceIndex}.");

        List<string> commands = addresses
            .ToList()
            .Select(address => $"netsh interface ipv6 add address {interfaceIndex} {address} skipassource=true")
            .ToList();

        await _commandLineCaller.ExecuteMultipleAsync(commands);
    }

    private async Task DeleteInterfaceIpv6AddressesAsync(List<NetworkAddress> addresses, uint interfaceIndex)
    {
        if (addresses.Count == 0)
        {
            return;
        }

        _logger.Info<IPv6Log>($"Deleting {addresses.Count} fake IPv6 addresses from interface with index {interfaceIndex}.");

        List<string> commands = addresses
            .ToList()
            .Select(address => $"netsh interface ipv6 delete address {interfaceIndex} {address}")
            .ToList();

        await _commandLineCaller.ExecuteMultipleAsync(commands);
    }

    private HashSet<NetworkAddress> GetGlobalUnicastAddresses()
    {
        INetworkInterface tunnelInterface = GetTunnelInterface();

        return _networkInterfaces
            .GetInterfaces()
            .Where(i => !i.Equals(tunnelInterface))
            .SelectMany(i => i.GetUnicastAddresses())
            .Where(a => a.IsGlobalUnicastAddress())
            .ToHashSet();
    }

    private INetworkInterface GetTunnelInterface()
    {
        return _networkInterfaceLoader.GetByVpnProtocol(_lastConnectedProtocol ?? _config.VpnProtocol, _config.OpenVpnAdapter);
    }

    private void InvokeConnecting()
    {
        VpnHost server = _servers.FirstOrDefault();
        if (!server.IsEmpty())
        {
            InvokeStateChanged(new VpnState(VpnStatus.Pinging, VpnError.None, string.Empty, server.Ip, 0,
                _config.VpnProtocol, openVpnAdapter: _config.OpenVpnAdapter, label: server.Label));
        }
    }

    private void InvokeStateChanged(VpnState state)
    {
        StateChanged?.Invoke(this, new EventArgs<VpnState>(state));
    }

    private async Task RunIpv6ActionAsync(Func<Task> action)
    {
        await _ipv6Semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            await action().ConfigureAwait(false);
        }
        finally
        {
            _ipv6Semaphore.Release();
        }
    }
}