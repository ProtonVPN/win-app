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

using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.NetworkFilter;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.SplitTunneling
{
    public class SplitTunnel : IVpnStateAware
    {
        private bool _reverseEnabled;
        private bool _enabled;

        private readonly IServiceSettings _serviceSettings;
        private readonly IncludeModeApps _reverseSplitTunnelApps;
        private readonly ISplitTunnelClient _splitTunnelClient;
        private readonly IFilterCollection _appFilter;
        private readonly IFilterCollection _permittedRemoteAddress;

        public SplitTunnel(
            IServiceSettings serviceSettings,
            ISplitTunnelClient splitTunnelClient,
            IncludeModeApps reverseSplitTunnelApps,
            IFilterCollection appFilter,
            IFilterCollection permittedRemoteAddress)
        {
            _permittedRemoteAddress = permittedRemoteAddress;
            _appFilter = appFilter;
            _splitTunnelClient = splitTunnelClient;
            _reverseSplitTunnelApps = reverseSplitTunnelApps;
            _serviceSettings = serviceSettings;
        }

        public SplitTunnel(
            bool enabled,
            bool reverseEnabled,
            IServiceSettings serviceSettings,
            ISplitTunnelClient splitTunnelClient,
            IncludeModeApps reverseSplitTunnelApps,
            IFilterCollection appFilter,
            IFilterCollection permittedRemoteAddress) :
            this(serviceSettings,
                splitTunnelClient,
                reverseSplitTunnelApps,
                appFilter,
                permittedRemoteAddress)
        {
            _enabled = enabled;
            _reverseEnabled = reverseEnabled;
        }

        public void OnVpnConnecting(VpnState state)
        {
            DisableReversed();
            Disable();
            _appFilter.RemoveAll();
            _permittedRemoteAddress.RemoveAll();

            if (_serviceSettings.SplitTunnelSettings.Mode == SplitTunnelModeIpcEntity.Permit)
            {
                _appFilter.Add(_serviceSettings.SplitTunnelSettings.AppPaths, Action.SoftBlock);
            }
        }

        public void OnVpnConnected(VpnState state)
        {
            if (_serviceSettings.SplitTunnelSettings.Mode == SplitTunnelModeIpcEntity.Disabled)
            {
                return;
            }

            switch (_serviceSettings.SplitTunnelSettings.Mode)
            {
                case SplitTunnelModeIpcEntity.Block:
                    DisableReversed();
                    Enable();
                    break;
                case SplitTunnelModeIpcEntity.Permit:
                    Disable();
                    EnableReversed(state);
                    _appFilter.RemoveAll();
                    break;
            }
        }

        public void OnVpnDisconnected(VpnState state)
        {
            if (state.Error == VpnError.None)
            {
                DisableSplitTunnel();
                _appFilter.RemoveAll();
            }
        }

        public void AssigningIp(VpnState state)
        {
        }

        private void DisableSplitTunnel()
        {
            Disable();
            DisableReversed();
        }

        private void Enable()
        {
            _splitTunnelClient.EnableExcludeMode(
                _serviceSettings.SplitTunnelSettings.AppPaths,
                _serviceSettings.SplitTunnelSettings.Ips);

            if (_serviceSettings.SplitTunnelSettings.AppPaths.Length > 0)
            {
                _appFilter.Add(_serviceSettings.SplitTunnelSettings.AppPaths, Action.SoftPermit);
            }

            if (_serviceSettings.SplitTunnelSettings.Ips.Length > 0)
            {
                _permittedRemoteAddress.Add(_serviceSettings.SplitTunnelSettings.Ips, Action.SoftPermit);
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

        private void EnableReversed(VpnState state)
        {
            _splitTunnelClient.EnableIncludeMode(
                _reverseSplitTunnelApps.Value(),
                _serviceSettings.SplitTunnelSettings.Ips,
                state.LocalIp);

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
}
