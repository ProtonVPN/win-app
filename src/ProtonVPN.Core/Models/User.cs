/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Core.User;

namespace ProtonVPN.Core.Models
{
    public class User
    {
        public string VpnUsername { get; set; }
        public string Username { get; set; }
        public string VpnPassword { get; set; }
        public string VpnPlan { get; set; }
        public sbyte MaxTier { get; set; }
        public int Services { get; set; }
        public int ExpirationTime { get; set; }
        public int Delinquent { get; set; }
        public int MaxConnect { get; set; }

        public string GetAccountPlan()
        {
            switch (Services)
            {
                case 1:
                case 5:
                    return "ProtonMail Account";
                case 4:
                    return "ProtonVPN Account";
                default:
                    return "?";
            }
        }

        public PlanStatus TrialStatus()
        {
            switch (VpnPlan)
            {
                case null:
                    return PlanStatus.Free;
                case "trial" when ExpirationTime == 0:
                    return PlanStatus.TrialNotStarted;
                case "trial" when ExpirationTime > 0:
                    return PlanStatus.TrialStarted;
                default:
                    return Paid() ? PlanStatus.Paid : PlanStatus.Free;
            }
        }

        public bool Paid()
        {
            return VpnPlan != null && !VpnPlan.Equals("trial") && !VpnPlan.Equals("free");
        }

        public bool IsTrial()
        {
            return VpnPlan != null && VpnPlan.Equals("trial");
        }

        public long TrialExpirationTimeInSeconds()
        {
            var now = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            var secondsLeft = ExpirationTime - now;
            return secondsLeft < 0 ? 0 : secondsLeft;
        }

        public bool Empty()
        {
            return string.IsNullOrEmpty(Username);
        }

        public static User EmptyUser()
        {
            return new User();
        }
    }
}
