/*
 * Copyright (c) 2022 Proton Technologies AG
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

using ProtonVPN.Common.Extensions;
using ProtonVPN.Translations;

namespace ProtonVPN.Account
{
    public class VpnPlanHelper
    {
        public static string GetPlanName(string vpnPlan)
        {
            switch (vpnPlan)
            {
                case "free":
                case "vpnbasic":
                case "vpnplus":
                case "visionary":
                    return Translation.Get($"VpnPlan_val_{vpnPlan.FirstCharToUpper()}");
                default:
                    return "Unknown plan";
            }
        }

        public static string GetPlanColor(string vpnPlan)
        {
            switch (vpnPlan)
            {
                case "free":
                    return "White";
                case "vpnbasic":
                    return "#fb7454";
                case "vpnplus":
                    return "#8ec122";
                case "visionary":
                    return "#54d8fd";
                default:
                    return "White";
            }
        }
    }
}
