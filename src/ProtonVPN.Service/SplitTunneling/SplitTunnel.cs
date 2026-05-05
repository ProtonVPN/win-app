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
using System.Net;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SplitTunnelLogs;
using ProtonVPN.NetworkFilter;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.ProTun.Contracts.Adapters;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;
using ProtonVPN.Vpn.SplitTunnel;
using Action = ProtonVPN.NetworkFilter.Action;

namespace ProtonVPN.Service.SplitTunneling;

public class SplitTunnel : ISplitTunnel
{
    private bool _reverseEnabled;
    private bool _enabled;
    private SplitTunnelContext? _context;

    private readonly ILogger _logger;
    private readonly ISplitTunnelRouting _splitTunnelRouting;
    private readonly INetworkUtilities _networkUtilities;
    private readonly ISystemNetworkInterfaces _networkInterfaces;
    private readonly IConfiguration _config;
    private readonly IServiceSettings _serviceSettings;
    private readonly ISplitTunnelClient _splitTunnelClient;
    private readonly IAppFilter _appFilter;
    private readonly IPermittedRemoteAddress _permittedRemoteAddress;
    private readonly IAdapterDetailsCache _proTunAdapterDetailsCache;

    public SplitTunnel(
        ILogger logger,
        ISplitTunnelRouting splitTunnelRouting,
        INetworkUtilities networkUtilities,
        ISystemNetworkInterfaces networkInterfaces,
        IConfiguration config,
        IServiceSettings serviceSettings,
        ISplitTunnelClient splitTunnelClient,
        IAppFilter appFilter,
        IPermittedRemoteAddress permittedRemoteAddress,
        IAdapterDetailsCache proTunAdapterDetailsCache)
    {
        _logger = logger;
        _splitTunnelRouting = splitTunnelRouting;
        _networkUtilities = networkUtilities;
        _networkInterfaces = networkInterfaces;
        _config = config;
        _serviceSettings = serviceSettings;
        _splitTunnelClient = splitTunnelClient;
        _appFilter = appFilter;
        _permittedRemoteAddress = permittedRemoteAddress;
        _proTunAdapterDetailsCache = proTunAdapterDetailsCache;
    }

    public SplitTunnel(
        bool enabled,
        bool reverseEnabled,
        ILogger logger,
        ISplitTunnelRouting splitTunnelRouting,
        INetworkUtilities networkUtilities,
        ISystemNetworkInterfaces networkInterfaces,
        IConfiguration config,
        IServiceSettings serviceSettings,
        ISplitTunnelClient splitTunnelClient,
        IAppFilter appFilter,
        IPermittedRemoteAddress permittedRemoteAddress,
        IAdapterDetailsCache proTunAdapterDetailsCache) :
        this(logger,
            splitTunnelRouting,
            networkUtilities,
            networkInterfaces,
            config,
            serviceSettings,
            splitTunnelClient,
            appFilter,
            permittedRemoteAddress,
            proTunAdapterDetailsCache)
    {
        _enabled = enabled;
        _reverseEnabled = reverseEnabled;
    }

    public void OnVpnConnecting(VpnState vpnState)
    {
        DisableReversed();
        Disable();

        _appFilter.RemoveAll();
        _permittedRemoteAddress.RemoveAll();

        if (_serviceSettings.SplitTunnelSettings.Mode == SplitTunnelModeIpcEntity.Permit)
        {
            _appFilter.Add(_serviceSettings.SplitTunnelSettings.AppPaths, [
                Tuple.Create(Layer.AppAuthConnectV4, Action.SoftBlock),
                Tuple.Create(Layer.AppAuthConnectV6, Action.SoftBlock),
            ]);
        }
    }

    public void OnVpnConnected(VpnState state)
    {
        if (_serviceSettings.SplitTunnelSettings.Mode == SplitTunnelModeIpcEntity.Disabled)
        {
            return;
        }

        SetUpApps(state);
        SetUpIps(state);
    }

    private void SetUpApps(VpnState state)
    {
        switch (_serviceSettings.SplitTunnelSettings.Mode)
        {
            case SplitTunnelModeIpcEntity.Block:
                DisableReversed();
                Enable(state);
                break;
            case SplitTunnelModeIpcEntity.Permit:
                _appFilter.RemoveAll();
                Disable();
                EnableReversed(state);
                break;
        }
    }

    public void UpdateContext(SplitTunnelContext context)
    {
        _context = context;
    }

    private void SetUpIps(VpnState state)
    {
        if (_context is null)
        {
            _logger.Warn<SplitTunnelLog>("Split tunnel context is missing, routes won't be added.");
            return;
        }

        string? localIpv4Address = state.LocalIp;
        if (string.IsNullOrEmpty(localIpv4Address))
        {
            _logger.Warn<SplitTunnelLog>("Local IPv4 address is missing, split tunneling routes won't be added.");
        }
        else
        {
            bool isIpv6Supported = _context.Config.IsIpv6Enabled && _context.Endpoint.Server.IsIpv6Supported;
            _splitTunnelRouting.SetUpRoutingTable(_context.Config, localIpv4Address, isIpv6Supported);
        }
    }

    public void OnVpnDisconnected(VpnState state)
    {
        if (state.Error == VpnError.None)
        {
            DisableSplitTunnel();
            _appFilter.RemoveAll();
            _context = null;
        }
    }

    private void DisableSplitTunnel()
    {
        Disable();
        DisableReversed();

        if (_context is null)
        {
            _logger.Warn<SplitTunnelLog>("Split tunnel context is missing, routes won't be removed.");
        }
        else
        {
            _splitTunnelRouting.DeleteRoutes(_context.Config);
        }
    }

    private void Enable(VpnState state)
    {
        string excludedHardwareId = _config.GetHardwareId(state.VpnProtocol, _serviceSettings.OpenVpnAdapter);
        IPAddress localIpv4Address = _networkUtilities.GetBestInterfaceIPv4Address(excludedHardwareId);
        INetworkInterface bestInterface = _networkInterfaces.GetBestInterfaceExcludingHardwareId(excludedHardwareId);

        IPAddress? localIpv6Address = null;
        if (!string.IsNullOrEmpty(bestInterface.Id))
        {
            localIpv6Address = bestInterface.GetPreferredIpv6UnicastAddress();
        }

        string[] appPaths = _serviceSettings.SplitTunnelSettings.AppPaths ?? [];

        _splitTunnelClient.EnableExcludeMode(appPaths, localIpv4Address, localIpv6Address);

        if (appPaths.Length > 0)
        {
            List<Tuple<Layer, Action>> appFilters = [
                Tuple.Create(Layer.AppAuthConnectV4, Action.HardPermit),
                Tuple.Create(Layer.AppAuthConnectV6, localIpv6Address is null ? Action.HardBlock : Action.HardPermit),
            ];

            _appFilter.Add(appPaths, [.. appFilters]);
        }

        if (_serviceSettings.SplitTunnelSettings.Ips.Length > 0)
        {
            _permittedRemoteAddress.Add(_serviceSettings.SplitTunnelSettings.Ips, Action.HardPermit);
        }

        _enabled = true;
    }

    private void Disable()
    {
        if (_enabled)
        {
            _splitTunnelClient.Disable();
            _appFilter.RemoveAll();
            _permittedRemoteAddress.RemoveAll();
            _enabled = false;
        }
    }

    private void EnableReversed(VpnState vpnState)
    {
        IPAddress? localIpv6Address = null;
        if (vpnState.VpnProtocol.IsWireGuard())
        {
            IPAddress.TryParse(_config.WireGuard.DefaultClientIpv6Address, out localIpv6Address);
        }
        else if (vpnState.VpnProtocol.IsProTun())
        {
            IPAddress.TryParse(_proTunAdapterDetailsCache.ClientIpv6Address, out localIpv6Address);
        }
        else if (vpnState.VpnProtocol.IsOpenVpn())
        {
            // ProtonVPN's OpenVPN server does not provide GUA IPv6 address, so we block all IPv6 tunnel traffic
            _appFilter.Add(_serviceSettings.SplitTunnelSettings.AppPaths, [Tuple.Create(Layer.AppAuthConnectV6, Action.HardBlock)]);
        }

        string[] appPaths = _serviceSettings.SplitTunnelSettings.AppPaths ?? [];

        _splitTunnelClient.EnableIncludeMode(appPaths, IPAddress.Parse(vpnState.LocalIp), localIpv6Address);

        _reverseEnabled = true;
    }

    private void DisableReversed()
    {
        if (_reverseEnabled)
        {
            _splitTunnelClient.Disable();
            _reverseEnabled = false;
        }
    }
}