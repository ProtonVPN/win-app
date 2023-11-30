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

using ProtonVPN.Client.Logic.Auth.Contracts.Enums;

namespace ProtonVPN.Client.Logic.Auth.Contracts.Models;

public class SsoAuthResult : AuthResult
{
    public string SsoChallengeToken { get; init; }

    protected internal SsoAuthResult(AuthError value, bool success, string error)
            : base(value, success, error)
    {
        SsoChallengeToken = string.Empty;
    }

    public static SsoAuthResult FromAuthResult(AuthResult result)
    {
        return new(result.Value, result.Success, result.Error);
    }

    public static SsoAuthResult Ok(string ssoChallengeToken)
    {
        return new(AuthError.None, true, string.Empty)
        {
            SsoChallengeToken = ssoChallengeToken,
        };
    }
}