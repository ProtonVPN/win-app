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

using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.Extensions;

public static class VpnErrorTypeIpcEntityExtensions
{
    public static bool HasError(this VpnErrorTypeIpcEntity? error)
    {
        return error is not null 
            && error.Value.HasError();
    }

    public static bool HasError(this VpnErrorTypeIpcEntity error)
    {
        return error is not VpnErrorTypeIpcEntity.None
                    and not VpnErrorTypeIpcEntity.NoneKeepEnabledKillSwitch;
    }

    public static bool IsTwoFactorError(this VpnErrorTypeIpcEntity? error)
    {
        return error is not null && error.Value.IsTwoFactorError();
    }

    public static bool IsTwoFactorError(this VpnErrorTypeIpcEntity error)
    {
        return error is VpnErrorTypeIpcEntity.TwoFactorRequiredReasonUnknown
                     or VpnErrorTypeIpcEntity.TwoFactorExpired
                     or VpnErrorTypeIpcEntity.TwoFactorNewConnection;
    }
}