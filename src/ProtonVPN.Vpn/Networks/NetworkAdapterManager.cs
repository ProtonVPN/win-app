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
using System.Collections.Generic;
using System.Linq;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.NetworkLogs;
using ProtonVPN.Vpn.Networks.Adapters;

namespace ProtonVPN.Vpn.Networks
{
    public class NetworkAdapterManager : INetworkAdapterManager
    {
        private const string WIREGUARD_NETWORK_ADAPTER_NAME = "ProtonVPN";
        private const string WIREGUARD_TUN_NAME = "WireGuard Tunnel";
        private const string PROTON_VPN_TAP_NAME = "TAP-ProtonVPN";
        private const string PROTON_VPN_TUN_NAME = "ProtonVPN Tunnel";
        private const string PROTON_VPN_TUN_NET_CONNECTION_ID = "ProtonVPN TUN";

        private readonly ILogger _logger;
        private readonly INetworkAdaptersLoader _networkAdaptersLoader;

        public NetworkAdapterManager(ILogger logger, INetworkAdaptersLoader networkAdaptersLoader)
        {
            _logger = logger;
            _networkAdaptersLoader = networkAdaptersLoader;
        }

        public int DisableDuplicatedWireGuardAdapters()
        {
            _logger.Info<NetworkLog>("Checking for duplicate WireGuard adapters.");
            int disabledAdapters = 0;
            try
            {
                IList<INetworkAdapter> duplicatedWireGuardAdapters = GetDuplicatedWireGuardAdapters();
                if (duplicatedWireGuardAdapters.Any())
                {
                    _logger.Warn<NetworkLog>($"Found {duplicatedWireGuardAdapters.Count} " +
                                 "duplicated WireGuard network adapter(s) to be disabled.");
                    disabledAdapters = DisableAdapters(duplicatedWireGuardAdapters);
                    _logger.Info<NetworkLog>($"Disabled {disabledAdapters} duplicated WireGuard network adapter(s).");
                }
            }
            catch (Exception e)
            {
                _logger.Error<NetworkLog>("An error occurred when disabling duplicated WireGuard network adapters.", e);
                disabledAdapters = 0;
            }

            return disabledAdapters;
        }

        private IList<INetworkAdapter> GetDuplicatedWireGuardAdapters()
        {
            IList<INetworkAdapter> networkAdapters = _networkAdaptersLoader.GetAll();
            IList<INetworkAdapter> duplicatedWireGuardAdapters = new List<INetworkAdapter>();
            foreach (INetworkAdapter networkAdapter in networkAdapters)
            {
                if (IsDuplicatedWireGuardAdapter(networkAdapter))
                {
                    duplicatedWireGuardAdapters.Add(networkAdapter);
                }
            }

            return duplicatedWireGuardAdapters;
        }

        private bool IsDuplicatedWireGuardAdapter(INetworkAdapter networkAdapter)
        {
            return IsAdapterNetConnectionIdDuplicated(networkAdapter) &&
                   IsWireGuardAdapter(networkAdapter);
        }

        private bool IsAdapterNetConnectionIdDuplicated(INetworkAdapter networkAdapter)
        {
            return networkAdapter.NetConnectionId.IsNotNullAndContains(WIREGUARD_NETWORK_ADAPTER_NAME) &&
                   networkAdapter.NetConnectionId != WIREGUARD_NETWORK_ADAPTER_NAME;
        }

        private bool IsWireGuardAdapter(INetworkAdapter networkAdapter)
        {
            return IsAtLeastOnePropertyContainingKey(WIREGUARD_TUN_NAME,
                networkAdapter.Name, networkAdapter.Description, networkAdapter.ProductName);
        }

        private bool IsAtLeastOnePropertyContainingKey(string key, params string[] properties)
        {
            return properties?.FirstOrDefault(p => p != null && p.Contains(key)) != null;
        }

        private int DisableAdapters(IList<INetworkAdapter> networkAdapters)
        {
            int disabledAdapters = 0;
            foreach (INetworkAdapter adapter in networkAdapters)
            {
                bool wasAdapterDisabled = DisableAdapter(adapter);
                if (wasAdapterDisabled)
                {
                    disabledAdapters++;
                }
            }

            return disabledAdapters;
        }

        private bool DisableAdapter(INetworkAdapter networkAdapter)
        {
            string adapterDescription = networkAdapter.GenerateLoggingDescription();
            bool wasAdapterDisabled = false;
            if (networkAdapter.NetConnectionStatus == null || 
                networkAdapter.NetConnectionStatus != NetConnectionStatus.Disconnected)
            {
                try
                {
                    networkAdapter.Disable();
                    _logger.Warn<NetworkLog>($"Disabled network adapter. {adapterDescription}");
                    wasAdapterDisabled = true;
                }
                catch (Exception e)
                {
                    _logger.Error<NetworkLog>($"Failed to disable network adapter. {adapterDescription}", e);
                }
            }
            else
            {
                _logger.Info<NetworkLog>($"The network adapter is already Disconnected. {adapterDescription}");
            }

            return wasAdapterDisabled;
        }

        public int EnableOpenVpnAdapters()
        {
            _logger.Info<NetworkLog>("Checking for OpenVPN adapters.");
            int enabledAdapters = 0;
            try
            {
                IList<INetworkAdapter> openVpnAdapters = GetOpenVpnAdapters();
                if (openVpnAdapters.Any())
                {
                    _logger.Info<NetworkLog>($"Found {openVpnAdapters.Count} OpenVPN network adapter(s). " +
                                             $"Attempting to enable the disconnected ones.");
                    enabledAdapters = EnableAdapters(openVpnAdapters);
                    _logger.Info<NetworkLog>($"Enabled {enabledAdapters} OpenVPN network adapter(s).");
                }
            }
            catch (Exception e)
            {
                _logger.Error<NetworkLog>("An error occurred when enabling OpenVPN network adapters.", e);
                enabledAdapters = 0;
            }

            return enabledAdapters;
        }

        private IList<INetworkAdapter> GetOpenVpnAdapters()
        {
            IList<INetworkAdapter> networkAdapters = _networkAdaptersLoader.GetAll();
            IList<INetworkAdapter> openVpnAdapters = new List<INetworkAdapter>();
            foreach (INetworkAdapter networkAdapter in networkAdapters)
            {
                if (IsOpenVpnAdapter(networkAdapter))
                {
                    openVpnAdapters.Add(networkAdapter);
                }
            }

            return openVpnAdapters;
        }

        private bool IsOpenVpnAdapter(INetworkAdapter networkAdapter)
        {
            return IsOpenVpnTapAdapter(networkAdapter) || IsOpenVpnTunAdapter(networkAdapter);
        }

        private bool IsOpenVpnTapAdapter(INetworkAdapter networkAdapter)
        {
            return IsAtLeastOnePropertyContainingKey(PROTON_VPN_TAP_NAME, 
                networkAdapter.Name, networkAdapter.Description, networkAdapter.ProductName);
        }

        private bool IsOpenVpnTunAdapter(INetworkAdapter networkAdapter)
        {
            return networkAdapter.NetConnectionId.IsNotNullAndContains(PROTON_VPN_TUN_NET_CONNECTION_ID) ||
                   networkAdapter.Name.IsNotNullAndContains(PROTON_VPN_TUN_NAME);
        }

        private int EnableAdapters(IList<INetworkAdapter> networkAdapters)
        {
            int enabledAdapters = 0;
            foreach (INetworkAdapter networkAdapter in networkAdapters)
            {
                bool wasAdapterEnabled = EnableAdapter(networkAdapter);
                if (wasAdapterEnabled)
                {
                    enabledAdapters++;
                }
            }

            return enabledAdapters;
        }

        private bool EnableAdapter(INetworkAdapter networkAdapter)
        {
            string adapterDescription = networkAdapter.GenerateLoggingDescription();
            bool wasAdapterEnabled = false;
            if (networkAdapter.NetConnectionStatus == null || 
                networkAdapter.NetConnectionStatus != NetConnectionStatus.Connected)
            {
                try
                {
                    networkAdapter.Enable();
                    _logger.Warn<NetworkLog>($"Enabled network adapter. {adapterDescription}");
                    wasAdapterEnabled = true;
                }
                catch (Exception e)
                {
                    _logger.Error<NetworkLog>($"Failed to enable network adapter. {adapterDescription}", e);
                }
            }
            else
            {
                _logger.Info<NetworkLog>($"The network adapter is not disconnected. " +
                                         $"Its status is '{networkAdapter.NetConnectionStatus}'. {adapterDescription}");
            }

            return wasAdapterEnabled;
        }
    }
}
