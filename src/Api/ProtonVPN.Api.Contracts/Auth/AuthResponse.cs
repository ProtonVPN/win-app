﻿/*
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
using ProtonVPN.Api.Contracts.Common;

namespace ProtonVPN.Api.Contracts.Auth
{
    public class AuthResponse : BaseResponse
    {
        public string AccessToken { get; set; }

        public string Scope { get; set; }

        [JsonProperty("UID")]
        public string UniqueSessionId { get; set; }

        public string RefreshToken { get; set; }

        public string ServerProof { get; set; }

        [JsonProperty(PropertyName = "2FA")]
        public TwoFactorAuthResponse TwoFactor { get; set; }
    }
}