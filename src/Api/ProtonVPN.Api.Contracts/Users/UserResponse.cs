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

namespace ProtonVPN.Api.Contracts.Users;

public class UserResponse
{
    [JsonProperty(PropertyName = "ID")]
    public string UserId { get; set; }

    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string Email { get; set; }

    public long CreateTime { get; set; }

    public string GetUsername()
    {
        return !string.IsNullOrEmpty(Name)
            ? Name
            : !string.IsNullOrEmpty(Email)
                ? Email
                : DisplayName;
    }

    public string GetDisplayName()
    {
        return !string.IsNullOrEmpty(DisplayName)
            ? DisplayName
            : !string.IsNullOrEmpty(Name)
                ? Name
                : Email;
    }
}
