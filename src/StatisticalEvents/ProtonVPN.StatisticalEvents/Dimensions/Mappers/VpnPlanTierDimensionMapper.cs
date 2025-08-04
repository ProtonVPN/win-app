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

using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers;

public class VpnPlanTierDimensionMapper : DimensionMapperBase, IVpnPlanTierDimensionMapper
{
    private const string NON_USER = "non-user";
    private const string FREE = "free";
    private const string PAID = "paid";
    private const string INTERNAL = "internal";
    private const string CREDENTIAL_LESS = "credential-less"; // Not used on Windows

    public string Map(VpnPlan? vpnPlan)
    {
        return vpnPlan?.MaxTier switch
        {
            null => NON_USER,
            0 => FREE,
            1 or 2 => PAID,
            3 => INTERNAL,
            _ => NOT_AVAILABLE
        };
    }
}