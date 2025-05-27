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

using System.Collections.Generic;

namespace ProtonVPN.Common.Legacy.Vpn;

public static class VpnErrorExtensions
{
    private static readonly List<VpnError> _sessionLimitVpnErrors = new()
    {
        VpnError.SessionLimitReachedBasic,
        VpnError.SessionLimitReachedFree,
        VpnError.SessionLimitReachedPlus,
        VpnError.SessionLimitReachedPro,
        VpnError.SessionLimitReachedVisionary,
        VpnError.SessionLimitReachedUnknown,
    };

    public static bool IsSessionLimitError(this VpnError error)
    {
        return _sessionLimitVpnErrors.Contains(error);
    }

    public static bool IsNetworkAdapterError(this VpnError error)
    {
        return error is
            VpnError.TapAdapterInUseError or
            VpnError.NoTapAdaptersError or
            VpnError.TapRequiresUpdateError or
            VpnError.WireGuardAdapterInUseError or
            VpnError.InterfaceHasForwardingEnabled;
    }
}