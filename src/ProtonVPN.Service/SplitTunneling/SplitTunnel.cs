using System;
using System.Collections.Generic;
using System.Net;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.NetworkFilter;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Vpn.Common;
using Action = ProtonVPN.NetworkFilter.Action;

namespace ProtonVPN.Service.SplitTunneling;

public class SplitTunnel : IVpnStateAware, IServiceSettingsAware
{
    private bool _reverseEnabled;
    private bool _enabled;
    private VpnState _lastVpnState = new(VpnStatus.Disconnected, default);

    private readonly INetworkUtilities _networkUtilities;
    private readonly ISystemNetworkInterfaces _networkInterfaces;
    private readonly IConfiguration _config;
    private readonly IServiceSettings _serviceSettings;
    private readonly ISplitTunnelClient _splitTunnelClient;
    private readonly IAppFilter _appFilter;
    private readonly IPermittedRemoteAddress _permittedRemoteAddress;

    public SplitTunnel(
        INetworkUtilities networkUtilities,
        ISystemNetworkInterfaces networkInterfaces,
        IConfiguration config,
        IServiceSettings serviceSettings,
        ISplitTunnelClient splitTunnelClient,
        IAppFilter appFilter,
        IPermittedRemoteAddress permittedRemoteAddress)
    {
        _networkUtilities = networkUtilities;
        _networkInterfaces = networkInterfaces;
        _config = config;
        _permittedRemoteAddress = permittedRemoteAddress;
        _appFilter = appFilter;
        _splitTunnelClient = splitTunnelClient;
        _serviceSettings = serviceSettings;
    }

    public SplitTunnel(
        bool enabled,
        bool reverseEnabled,
        INetworkUtilities networkUtilities,
        ISystemNetworkInterfaces networkInterfaces,
        IConfiguration config,
        IServiceSettings serviceSettings,
        ISplitTunnelClient splitTunnelClient,
        IAppFilter appFilter,
        IPermittedRemoteAddress permittedRemoteAddress)
        : this(networkUtilities, networkInterfaces, config, serviceSettings, splitTunnelClient, appFilter, permittedRemoteAddress)
    {
        _enabled = enabled;
        _reverseEnabled = reverseEnabled;
    }

    public void OnVpnConnecting(VpnState vpnState)
    {
        _lastVpnState = vpnState;
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
        _lastVpnState = state;
        ApplySplitTunnelSettings(state);
    }

    public void OnVpnDisconnected(VpnState state)
    {
        _lastVpnState = state;
        if (state.Error == VpnError.None)
        {
            DisableSplitTunnel();
            _appFilter.RemoveAll();
            _permittedRemoteAddress.RemoveAll();
        }
    }

    public void AssigningIp(VpnState state)
    {
        _lastVpnState = state;
    }

    public void OnServiceSettingsChanged(MainSettingsIpcEntity settings)
    {
        if (_lastVpnState.Status == VpnStatus.Connected)
        {
            ApplySplitTunnelSettings(_lastVpnState);
        }
    }

    private void ApplySplitTunnelSettings(VpnState state)
    {
        switch (_serviceSettings.SplitTunnelSettings.Mode)
        {
            case SplitTunnelModeIpcEntity.Disabled:
                DisableSplitTunnel();
                _appFilter.RemoveAll();
                _permittedRemoteAddress.RemoveAll();
                break;
            case SplitTunnelModeIpcEntity.Block:
                DisableReversed();
                Disable();
                Enable();
                break;
            case SplitTunnelModeIpcEntity.Permit:
                _appFilter.RemoveAll();
                _permittedRemoteAddress.RemoveAll();
                Disable();
                EnableReversed(state);
                break;
        }
    }

    private void DisableSplitTunnel()
    {
        Disable();
        DisableReversed();
    }

    private void Enable()
    {
        string excludedHardwareId = _config.GetHardwareId(_serviceSettings.OpenVpnAdapter);
        IPAddress localIpv4Address = _networkUtilities.GetBestInterfaceIPv4Address(excludedHardwareId);
        INetworkInterface bestInterface = _networkInterfaces.GetBestInterfaceExcludingHardwareId(excludedHardwareId);

        IPAddress localIpv6Address = null;
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
        IPAddress localIpv6Address = null;
        if (vpnState.VpnProtocol.IsWireGuard())
        {
            localIpv6Address = IPAddress.Parse(_config.WireGuard.DefaultClientIpv6Address);
        }
        else if (vpnState.VpnProtocol.IsOpenVpn())
        {
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
