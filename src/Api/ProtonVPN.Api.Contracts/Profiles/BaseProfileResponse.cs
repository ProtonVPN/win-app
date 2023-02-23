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

using Newtonsoft.Json;

namespace ProtonVPN.Api.Contracts.Profiles
{
    public class BaseProfileResponse
    {
        public string Name { get; set; }

        public int Protocol { get; set; }

        public string Color { get; set; }

        public int Features { get; set; }

        public int Type { get; set; }

        public string Country { get; set; }

        [JsonProperty("LogicalID")]
        public string LogicalId { get; set; }
    }
}