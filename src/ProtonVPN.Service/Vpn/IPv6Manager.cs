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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.IPv6.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.IPv6Logs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.OperatingSystems.Processes.Contracts;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;

namespace ProtonVPN.Service.Vpn;

internal class IPv6Manager : IIPv6Manager
{
    private const int MAX_FAKE_IPV6_ADDRESSES = 50;

    private readonly IIpv6 _ipv6;
    private readonly ILogger _logger;
    private readonly IFirewall _firewall;
    private readonly IServiceSettings _serviceSettings;
    private readonly IFakeIPv6AddressGenerator _fakeIPv6AddressGenerator;
    private readonly ICommandLineCaller _commandLineCaller;
    private readonly INetworkInterfaceProvider _networkInterfaceProvider;
    private readonly ISystemNetworkInterfaces _networkInterfaces;

    private readonly SemaphoreSlim _networkSemaphore = new(1, 1);
    private readonly SemaphoreSlim _ipv6Semaphore = new(1, 1);

    private HashSet<NetworkAddress> _lastGlobalUnicastAddresses = [];
    private List<NetworkAddress> _lastFakeIpv6Addresses = [];

    private volatile bool _wereInterfacesAdded;
    private VpnProtocol? _vpnProtocol;
    private VpnStatus? _vpnStatus;
    private OpenVpnAdapter? _openVpnAdapter;

    public IPv6Manager(
        IIpv6 ipv6,
        ILogger logger,
        IFirewall firewall,
        IServiceSettings serviceSettings,
        IFakeIPv6AddressGenerator fakeIPv6AddressGenerator,
        ICommandLineCaller commandLineCaller,
        INetworkInterfaceProvider networkInterfaceProvider,
        ISystemNetworkInterfaces networkInterfaces,
        IObservableNetworkInterfaces observableNetworkInterfaces)
    {
        _ipv6 = ipv6;
        _logger = logger;
        _firewall = firewall;
        _serviceSettings = serviceSettings;
        _fakeIPv6AddressGenerator = fakeIPv6AddressGenerator;
        _commandLineCaller = commandLineCaller;
        _networkInterfaceProvider = networkInterfaceProvider;
        _networkInterfaces = networkInterfaces;

        observableNetworkInterfaces.NetworkInterfacesAdded += OnNetworkInterfacesAddedAsync;
        networkInterfaces.NetworkAddressChanged += OnNetworkAddressChangedAsync;
    }

    public async Task HandleIPv6OnConnectAsync(VpnProtocol vpnProtocol, OpenVpnAdapter openVpnAdapter)
    {
        _vpnProtocol = vpnProtocol;
        _openVpnAdapter = openVpnAdapter;

        if (_serviceSettings.IsIpv6Enabled)
        {
            await HandleChaosAlgorithmAsync(vpnProtocol);
        }
        else
        {
            await DisableIpv6Async(vpnProtocol);
        }
    }

    public async Task OnVpnStatusChangedAsync(VpnStatus vpnStatus)
    {
        _vpnStatus = vpnStatus;

        if (_vpnProtocol is not null)
        {
            await HandleIPv6InterfacesAsync(vpnStatus, _vpnProtocol.Value);
        }

        if (_serviceSettings.IsIpv6Enabled)
        {
            HandleIPv6ChaosAsync(vpnStatus).FireAndForget();
        }
    }

    private async Task HandleIPv6InterfacesAsync(VpnStatus vpnStatus, VpnProtocol vpnProtocol)
    {
        if (vpnStatus == VpnStatus.Disconnected)
        {
            _wereInterfacesAdded = false;

            if ((!_firewall.LeakProtectionEnabled || _serviceSettings.IsIpv6Enabled) && !_ipv6.IsEnabled)
            {
                await RunIpv6ActionAsync(() => _ipv6.EnableAsync(vpnProtocol));
            }
        }
    }

    private async Task HandleIPv6ChaosAsync(VpnStatus vpnStatus)
    {
        switch (vpnStatus)
        {
            case VpnStatus.Connected when _lastFakeIpv6Addresses.Count > 0:
                INetworkInterface? tunnelInterface = GetTunnelInterface();
                if (tunnelInterface is not null)
                {
                    await AddInterfaceIpv6AddressesAsync(_lastFakeIpv6Addresses, tunnelInterface.Index);
                }
                break;
            case VpnStatus.Disconnected:
                _lastFakeIpv6Addresses.Clear();
                break;
        }
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

    private async Task HandleChaosAlgorithmAsync(VpnProtocol vpnProtocol)
    {
        if (!_ipv6.IsEnabled)
        {
            await RunIpv6ActionAsync(() => _ipv6.EnableAsync(vpnProtocol));
        }

        if (!_serviceSettings.Ipv6LeakProtection)
        {
            _lastFakeIpv6Addresses.Clear();
            return;
        }

        HashSet<NetworkAddress> globalUnicastAddresses = GetGlobalUnicastAddresses();

        if (globalUnicastAddresses.Count == 0)
        {
            _lastFakeIpv6Addresses.Clear();
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
    }

    private void LogGuaAddresses(HashSet<NetworkAddress> globalUnicastAddresses)
    {
        _logger.Debug<IPv6Log>($"GUA addresses detected: {string.Join(", ", globalUnicastAddresses)}");
    }

    private HashSet<NetworkAddress> GetGlobalUnicastAddresses()
    {
        INetworkInterface? tunnelInterface = GetTunnelInterface();
        if (tunnelInterface is null)
        {
            return [];
        }

        return _networkInterfaces
            .GetInterfaces()
            .Where(i => !i.Equals(tunnelInterface))
            .SelectMany(i => i.GetUnicastAddresses())
            .Where(a => a.IsGlobalUnicastAddress())
            .ToHashSet();
    }

    private INetworkInterface? GetTunnelInterface()
    {
        if (_vpnProtocol is null || _openVpnAdapter is null)
        {
            _logger.Error<IPv6Log>("Failed to get tunnel interface due to missing VPN protocol or OpenVPN adapter.");
            return null;
        }

        return _networkInterfaceProvider.GetByVpnProtocol(_vpnProtocol.Value, _openVpnAdapter.Value);
    }

    private async Task DisableIpv6Async(VpnProtocol vpnProtocol)
    {
        await _ipv6.EnableOnVPNInterfaceAsync(vpnProtocol);

        if (_ipv6.IsEnabled && _serviceSettings.Ipv6LeakProtection)
        {
            await RunIpv6ActionAsync(() => _ipv6.DisableAsync(vpnProtocol));
        }
        else if (!_ipv6.IsEnabled && !_serviceSettings.Ipv6LeakProtection)
        {
            await RunIpv6ActionAsync(() => _ipv6.EnableAsync(vpnProtocol));
        }
    }

    private async void OnNetworkInterfacesAddedAsync(object? sender, EventArgs e)
    {
        if (_wereInterfacesAdded || _vpnProtocol is null || _vpnStatus != VpnStatus.Connected)
        {
            return;
        }

        _wereInterfacesAdded = true;

        if (!_ipv6.IsEnabled)
        {
            await RunIpv6ActionAsync(() => _ipv6.DisableAsync(_vpnProtocol.Value));
        }
    }

    private async void OnNetworkAddressChangedAsync(object? sender, EventArgs e)
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

            INetworkInterface? tunnelInterface = GetTunnelInterface();
            if (tunnelInterface is null)
            {
                return;
            }

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
}