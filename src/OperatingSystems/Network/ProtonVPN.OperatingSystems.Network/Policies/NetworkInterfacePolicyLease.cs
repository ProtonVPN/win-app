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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using Vanara.PInvoke;
using static Vanara.PInvoke.IpHlpApi;
using static Vanara.PInvoke.Ws2_32;

namespace ProtonVPN.OperatingSystems.Network.Policies;

public sealed class NetworkInterfacePolicyLease : INetworkInterfacePolicyLease
{
    private readonly ILogger _logger;
    private readonly uint _interfaceIndex;

    private readonly List<AppliedInterfaceState> _appliedStates = [];

    private bool _disposed;

    public NetworkInterfacePolicyLease(ILogger logger, uint interfaceIndex)
    {
        _logger = logger;
        _interfaceIndex = interfaceIndex;
    }

    public void Apply()
    {
        TryApplyForFamily(ADDRESS_FAMILY.AF_INET);
        TryApplyForFamily(ADDRESS_FAMILY.AF_INET6);
    }

    private void TryApplyForFamily(ADDRESS_FAMILY family)
    {
        MIB_IPINTERFACE_ROW row = new()
        {
            Family = family,
            InterfaceIndex = _interfaceIndex,
        };

        Win32Error result = GetIpInterfaceEntry(ref row);
        if (result.Failed)
        {
            _logger.Warn<NetworkLog>($"Failed to query interface {_interfaceIndex} ({family}) while applying policy.", result.GetException());
            return;
        }

        bool originalWeakHostSend = row.WeakHostSend;
        bool originalWeakHostReceive = row.WeakHostReceive;

        bool wasWeakHostSendModified = false;
        bool wasWeakHostReceiveModified = false;

        if (row.WeakHostSend)
        {
            row.WeakHostSend = false;
            wasWeakHostSendModified = true;
        }

        if (row.WeakHostReceive)
        {
            row.WeakHostReceive = false;
            wasWeakHostReceiveModified = true;
        }

        if (!wasWeakHostSendModified && !wasWeakHostReceiveModified)
        {
            return;
        }

        FixInterfaceRow(ref row);

        result = SetIpInterfaceEntry(row);
        if (result.Succeeded)
        {
            _appliedStates.Add(new AppliedInterfaceState()
            {
                Family = family,
                OriginalWeakHostSend = originalWeakHostSend,
                WasWeakHostSendModified = wasWeakHostSendModified,
                OriginalWeakHostReceive = originalWeakHostReceive,
                WasWeakHostReceiveModified = wasWeakHostReceiveModified,
            });

            _logger.Info<NetworkLog>($"Applied interface policy for index {_interfaceIndex} ({family}).\n" +
                $"Was WeakHostSend disabled: {wasWeakHostSendModified}\n" +
                $"Was WeakHostReceive disabled: {wasWeakHostReceiveModified}");
        }
        else
        {
            _logger.Warn<NetworkLog>($"Failed to apply interface policy for index {_interfaceIndex} ({family}).", result.GetException());
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (AppliedInterfaceState state in _appliedStates)
        {
            Restore(state);
        }

        _disposed = true;
    }

    private void Restore(AppliedInterfaceState state)
    {
        if (!state.WasWeakHostSendModified && !state.WasWeakHostReceiveModified)
        {
            return;
        }

        MIB_IPINTERFACE_ROW row = new()
        {
            Family = state.Family,
            InterfaceIndex = _interfaceIndex,
        };

        Win32Error query = GetIpInterfaceEntry(ref row);
        if (query.Failed)
        {
            _logger.Warn<NetworkLog>($"Failed to query interface {_interfaceIndex} ({state.Family}) while restoring policy.", query.GetException());
            return;
        }

        row.WeakHostSend = state.OriginalWeakHostSend;
        row.WeakHostReceive = state.OriginalWeakHostReceive;

        FixInterfaceRow(ref row);

        Win32Error update = SetIpInterfaceEntry(row);
        if (update.Failed)
        {
            _logger.Warn<NetworkLog>($"Failed to restore interface policy for index {_interfaceIndex} ({state.Family}).", update.GetException());
        }
    }

    private static void FixInterfaceRow(ref MIB_IPINTERFACE_ROW row)
    {
        byte maxPrefix = row.Family == ADDRESS_FAMILY.AF_INET ? (byte)32 : (byte)128;
        if (row.SitePrefixLength > maxPrefix)
        {
            row.SitePrefixLength = maxPrefix;
        }
    }
}