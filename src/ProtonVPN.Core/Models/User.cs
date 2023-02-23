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

using ProtonVPN.Core.Servers;

namespace ProtonVPN.Core.Models
{
    public class User
    {
        public string Username { get; set; }
        public string VpnPlan { get; set; }
        public sbyte MaxTier { get; set; }
        public int Services { get; set; }
        public int Delinquent { get; set; }
        public int MaxConnect { get; set; }
        public int Subscribed { get; set; }
        public int HasPaymentMethod { get; set; }
        public int Credit { get; set; }
        public string OriginalVpnPlan { get; set; }
        public string VpnPlanName { get; set; }

        public string GetAccountPlan()
        {
            return (Services & 1) == 0 && (Services & 4) != 0 ? "ProtonVPN Account" : "ProtonMail Account";
        }

        public bool Paid()
        {
            return VpnPlan != null && !VpnPlan.Equals("free");
        }

        public bool IsPlusPlan()
        {
            return VpnPlan is "vpnplus" or "vpn2022";
        }

        public bool Empty()
        {
            return string.IsNullOrEmpty(Username);
        }

        public bool IsDelinquent()
        {
            return IsDelinquent(Delinquent);
        }

        public static bool IsDelinquent(int delinquent)
        {
            return delinquent > 2;
        }

        public static User EmptyUser()
        {
            return new User();
        }

        public bool IsTierPlusOrHigher()
        {
            return MaxTier >= ServerTiers.Plus;
        }

        public bool CanUsePromoCode()
        {
            return Subscribed == 0 && Delinquent == 0 && HasPaymentMethod == 0 && Credit == 0;
        }
    }
}