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

using Autofac;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Legacy.KillSwitch;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.KillSwitch;

public class KillSwitch : IVpnStateAware, IServiceSettingsAware, IStartable
{
    private readonly IFirewall _firewall;
    private readonly IServiceSettings _serviceSettings;
    private readonly INetworkInterfaceLoader _networkInterfaceLoader;
    private VpnState _lastVpnState = new(VpnStatus.Disconnected, default);
    private KillSwitchMode _killSwitchMode;

    public KillSwitch(
        IFirewall firewall,
        IServiceSettings serviceSettings,
        INetworkInterfaceLoader networkInterfaceLoader)
    {
        _firewall = firewall;
        _serviceSettings = serviceSettings;
        _networkInterfaceLoader = networkInterfaceLoader;
    }

    public void Start()
    {
        _killSwitchMode = _serviceSettings.KillSwitchMode;
    }

    public void OnVpnConnecting(VpnState state)
    {
        _lastVpnState = state;
        UpdateLeakProtectionStatus(state);
    }

    public void OnVpnConnected(VpnState state)
    {
        _lastVpnState = state;

        if (_lastVpnState.VpnProtocol.IsWireGuard())
        {
            UpdateLeakProtectionStatus(state);
        }
    }

    public void OnVpnDisconnected(VpnState state)
    {
        _lastVpnState = state;
        UpdateLeakProtectionStatus(state);
    }

    public bool ExpectedLeakProtectionStatus(VpnState state)
    {
        return UpdatedLeakProtectionStatus(state) ?? _firewall.LeakProtectionEnabled;
    }

    public void AssigningIp(VpnState state)
    {
        // AssigningIp VPN status for WireGuard is fired when WireGuard finishes its startup "Startup complete"
        // Only then the interface is up and we can get its index to permit it on the firewall.
        if (state.VpnProtocol.IsWireGuard())
        {
            EnableLeakProtection();
        }
    }

    private void UpdateLeakProtectionStatus(VpnState state)
    {
        switch (UpdatedLeakProtectionStatus(state))
        {
            case true:
                EnableLeakProtection();
                break;
            case false:
                _firewall.DisableLeakProtection();
                break;
        }
    }

    public void OnServiceSettingsChanged(MainSettingsIpcEntity settings)
    {
        KillSwitchMode killSwitchMode = (KillSwitchMode)settings.KillSwitchMode;
        if (_killSwitchMode != killSwitchMode)
        {
            HandleKillSwitchModeChange(killSwitchMode);
        }
        else
        {
            if (killSwitchMode == KillSwitchMode.Hard && !_firewall.LeakProtectionEnabled)
            {
                EnableLeakProtection();
            }
        }

        _killSwitchMode = killSwitchMode;

        if (_firewall.IsLocalAreaNetworkAccessEnabled.HasValue &&
            settings.IsLocalAreaNetworkAccessEnabled != _firewall.IsLocalAreaNetworkAccessEnabled &&
            _lastVpnState.Status == VpnStatus.Connected)
        {
            EnableLeakProtection();
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

    private void EnableLeakProtection()
    {
        bool dnsLeakOnly = _serviceSettings.SplitTunnelSettings.Mode == SplitTunnelModeIpcEntity.Permit;
        bool persistent = _serviceSettings.KillSwitchMode == KillSwitchMode.Hard;
        INetworkInterface networkInterface = _networkInterfaceLoader.GetByVpnProtocol(_lastVpnState.VpnProtocol, _lastVpnState.OpenVpnAdapter);
        uint interfaceIndex = networkInterface?.Index ?? 0;
        FirewallParams firewallParams = new()
        {
            ServerIp = _lastVpnState.RemoteIp,
            DnsLeakOnly = dnsLeakOnly,
            InterfaceIndex = interfaceIndex,
            AddInterfaceFilters = interfaceIndex > 0,
            Persistent = persistent,
            IsLocalAreaNetworkAccessEnabled = _serviceSettings.IsLocalAreaNetworkAccessEnabled,
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
            case VpnStatus.Connected when _lastVpnState.VpnProtocol.IsWireGuard():
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