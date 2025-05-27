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

using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.Vpn;

public class NetworkSettings : IVpnStateAware
{
    private readonly ILogger _logger;
    private readonly INetworkInterfaceLoader _networkInterfaceLoader;
    private readonly INetworkUtilities _networkUtilities;
    private readonly WintunRegistryFixer _wintunRegistryFixer;

    public NetworkSettings(
        ILogger logger,
        INetworkInterfaceLoader networkInterfaceLoader,
        INetworkUtilities networkUtilities,
        WintunRegistryFixer wintunRegistryFixer)
    {
        _logger = logger;
        _networkInterfaceLoader = networkInterfaceLoader;
        _networkUtilities = networkUtilities;
        _wintunRegistryFixer = wintunRegistryFixer;
    }

    public void OnVpnDisconnected(VpnState state)
    {
        if (state.VpnProtocol.IsOpenVpn())
        {
            RestoreNetworkSettings(state.VpnProtocol, state.OpenVpnAdapter);
        }
    }

    public void OnVpnConnected(VpnState state)
    {
    }

    public void AssigningIp(VpnState state)
    {
    }

    public void OnVpnConnecting(VpnState state)
    {
        if (state.VpnProtocol == VpnProtocol.OpenVpnTcp || state.VpnProtocol == VpnProtocol.OpenVpnUdp)
        {
            ApplyNetworkSettings(state.VpnProtocol, state.OpenVpnAdapter);
            _wintunRegistryFixer.EnsureTunAdapterRegistryIsCorrect();
        }
    }

    private void ApplyNetworkSettings(VpnProtocol vpnProtocol, OpenVpnAdapter? openVpnAdapter)
    {
        uint interfaceIndex = _networkInterfaceLoader.GetByVpnProtocol(vpnProtocol, openVpnAdapter).Index;

        try
        {
            _logger.Info<NetworkLog>("Setting interface metric...");
            _networkUtilities.SetLowestTapMetric(interfaceIndex);
            _logger.Info<NetworkLog>("Interface metric set.");
        }
        catch (NetworkUtilException e)
        {
            _logger.Error<NetworkLog>("Failed to apply network settings. Error code: " + e.Code);
        }
    }

    private void RestoreNetworkSettings(VpnProtocol vpnProtocol, OpenVpnAdapter? openVpnAdapter)
    {
        uint interfaceIndex = _networkInterfaceLoader.GetByVpnProtocol(vpnProtocol, openVpnAdapter)?.Index ?? 0;
        if (interfaceIndex == 0)
        {
            return;
        }

        try
        {
            _logger.Info<NetworkLog>("Restoring interface metric...");
            _networkUtilities.RestoreDefaultTapMetric(interfaceIndex);
            _logger.Info<NetworkLog>("Interface metric restored.");
        }
        catch (NetworkUtilException e)
        {
            _logger.Error<NetworkLog>("Failed restore network settings. Error code: " + e.Code);
        }
    }
}