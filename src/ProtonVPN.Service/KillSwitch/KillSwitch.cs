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

using Autofac;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.KillSwitch;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;

namespace ProtonVPN.Service.KillSwitch;

public class KillSwitch : IKillSwitch, IServiceSettingsAware, IStartable
{
    private readonly IFirewall _firewall;
    private readonly IServiceSettings _serviceSettings;
    private readonly object _stateLock = new();
    private readonly INetworkInterfaceProvider _networkInterfaceProvider;
    private VpnState _lastVpnState = new(VpnStatus.Disconnected, default);
    private KillSwitchMode _killSwitchMode;

    private VpnProtocol _lastConnectedProtocol;

    public KillSwitch(
        IFirewall firewall,
        IServiceSettings serviceSettings,
        INetworkInterfaceProvider networkInterfaceProvider)
    {
        _firewall = firewall;
        _serviceSettings = serviceSettings;
        _networkInterfaceProvider = networkInterfaceProvider;
    }

    public void Start()
    {
        _killSwitchMode = _serviceSettings.KillSwitchMode;
    }

    public void OnVpnConnecting(VpnState state)
    {
        lock (_stateLock)
        {
            _lastVpnState = state;
        }
        UpdateLeakProtectionStatus(state);
    }

    public void OnVpnConnected(VpnState state)
    {
        bool hasVpnProtocolChanged;
        lock (_stateLock)
        {
            _lastVpnState = state;
            hasVpnProtocolChanged = state.VpnProtocol != _lastConnectedProtocol;
            _lastConnectedProtocol = state.VpnProtocol;
        }
        UpdateLeakProtectionStatus(state, hasVpnProtocolChanged);
    }

    public void OnVpnDisconnected(VpnState state)
    {
        lock (_stateLock)
        {
            _lastVpnState = state;
        }
        UpdateLeakProtectionStatus(state);
    }

    public bool GetExpectedLeakProtectionStatus(VpnState state)
    {
        return UpdatedLeakProtectionStatus(state) ?? _firewall.LeakProtectionEnabled;
    }

    public void AssigningIp(VpnState state)
    {
        lock (_stateLock)
        {
            _lastVpnState = state;
        }

        // AssigningIp VPN status for WireGuard is fired when WireGuard finishes its startup "Startup complete"
        // Only then the interface is up and we can get its index to permit it on the firewall.
        if (state.VpnProtocol.IsProTunOrWireGuard())
        {
            EnableLeakProtection(state);
        }
    }

    private void UpdateLeakProtectionStatus(VpnState state, bool hasVpnProtocolChanged = false)
    {
        switch (UpdatedLeakProtectionStatus(state))
        {
            case true:
                EnableLeakProtection(state, hasVpnProtocolChanged);
                break;
            case false:
                _firewall.DisableLeakProtection();
                break;
        }
    }

    public void OnServiceSettingsChanged(MainSettingsIpcEntity settings)
    {
        VpnState lastVpnState;
        lock (_stateLock)
        {
            lastVpnState = _lastVpnState;
        }

        KillSwitchMode killSwitchMode = (KillSwitchMode)settings.KillSwitchMode;
        if (_killSwitchMode != killSwitchMode)
        {
            HandleKillSwitchModeChange(killSwitchMode);
        }
        else
        {
            if (killSwitchMode == KillSwitchMode.Hard && !_firewall.LeakProtectionEnabled)
            {
                EnableLeakProtection(lastVpnState);
            }
        }

        _killSwitchMode = killSwitchMode;

        if (_firewall.IsLocalAreaNetworkAccessEnabled.HasValue &&
            settings.IsLocalAreaNetworkAccessEnabled != _firewall.IsLocalAreaNetworkAccessEnabled &&
            lastVpnState.Status == VpnStatus.Connected)
        {
            EnableLeakProtection(lastVpnState);
        }
    }

    private void HandleKillSwitchModeChange(KillSwitchMode killSwitchMode)
    {
        switch (killSwitchMode)
        {
            case KillSwitchMode.Off when _lastVpnState.Status != VpnStatus.Connected:
                _firewall.DisableLeakProtection();
                break;
            case KillSwitchMode.Off when _lastVpnState.Status == VpnStatus.Connected:
            case KillSwitchMode.Soft when _lastVpnState.Status == VpnStatus.Connected:
            case KillSwitchMode.Hard:
                EnableLeakProtection();
                break;
            case KillSwitchMode.Soft:
                if (_lastVpnState.Error != VpnError.NoneKeepEnabledKillSwitch)
                {
                    _firewall.DisableLeakProtection();
                }

                break;
        }
    }

    private void EnableLeakProtection(bool hasVpnProtocolChanged = false)
    {
        VpnState lastVpnState;
        lock (_stateLock)
        {
            lastVpnState = _lastVpnState;
        }

        EnableLeakProtection(lastVpnState, hasVpnProtocolChanged);
    }

    private void EnableLeakProtection(VpnState state, bool hasVpnProtocolChanged = false)
    {
        bool dnsLeakOnly = _serviceSettings.SplitTunnelSettings.Mode == SplitTunnelModeIpcEntity.Permit && state.Status == VpnStatus.Connected;
        bool persistent = _serviceSettings.KillSwitchMode == KillSwitchMode.Hard;
        INetworkInterface networkInterface = _networkInterfaceProvider.GetByVpnProtocol(state.VpnProtocol, state.OpenVpnAdapter);
        uint interfaceIndex = networkInterface?.Index ?? 0;
        FirewallParams firewallParams = new()
        {
            ServerIp = state.RemoteIp,
            DnsLeakOnly = dnsLeakOnly,
            InterfaceIndex = interfaceIndex,
            AddInterfaceFilters = interfaceIndex > 0,
            Persistent = persistent,
            IsLocalAreaNetworkAccessEnabled = _serviceSettings.IsLocalAreaNetworkAccessEnabled,
            DnsBlockMode = _serviceSettings.DnsBlockMode,
            ForceRecreateDnsBlock = hasVpnProtocolChanged,
        };
        _firewall.EnableLeakProtection(firewallParams);
    }

    private bool? UpdatedLeakProtectionStatus(VpnState state)
    {
        switch (state.Status)
        {
            case VpnStatus.Pinging:
            case VpnStatus.Connecting:
            case VpnStatus.Reconnecting:
            case VpnStatus.Connected:
                return true;
            case VpnStatus.Disconnecting:
            case VpnStatus.Disconnected:
                if (state.Error == VpnError.PlanNeedsToBeUpgraded)
                {
                    // Since PlanNeedsToBeUpgraded is received only when connected, we don't want to
                    // disable firewall while reconnecting, so keep the current firewall state.
                    return null;
                }

                if (state.Error == VpnError.None || state.Error.IsSessionLimitError() || state.Error.IsNetworkAdapterError())
                {
                    return _serviceSettings.KillSwitchMode == KillSwitchMode.Hard;
                }

                return _serviceSettings.KillSwitchMode != KillSwitchMode.Off;
        }

        return null;
    }
}