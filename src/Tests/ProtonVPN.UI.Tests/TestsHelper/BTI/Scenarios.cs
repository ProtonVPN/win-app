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

namespace ProtonVPN.UI.Tests.TestsHelper.BTI
{
    public class Scenarios
    {
        public const string HARDJAIL_ALL_UNKOWN_ERROR = "enable/sessions_hardjail_all";
        public const string UNHARDJAIL_ALL = "enable/sessions_un_hardjail_all";
        public const string RESET = "reset";
        public const string ATLAS_UNJAIL_ALL = "internal/quark/jail:unban";
        public static string SEED_PLUS_USER(string username)
        {
            return $"/internal/quark/payments:seed-subscriber?username={username}&password=a&plan=vpn2022&cycle=12";
        }

        public static string DOWNGRADE_USER(string userId)
        {
            return $"/internal/quark/payments:subscription:downgrade?userid={userId}";
        }
    }
}
