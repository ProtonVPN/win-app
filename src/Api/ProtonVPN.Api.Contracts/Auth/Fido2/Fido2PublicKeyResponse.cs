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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace ProtonVPN.Api.Contracts.Auth.Fido2;

public class Fido2PublicKeyResponse
{
    [JsonProperty(PropertyName = "timeout")]
    public int Timeout { get; set; }

    [JsonProperty(PropertyName = "challenge")]
    public List<byte> Challenge { get; set; }

    [JsonProperty(PropertyName = "userVerification")]
    public string UserVerification { get; set; }

    [JsonProperty(PropertyName = "rpId")]
    public string RpId { get; set; }

    [JsonProperty(PropertyName = "allowCredentials")]
    public List<Fido2PublicKeyAllowCredentialsResponse> AllowCredentials { get; set; }
}
