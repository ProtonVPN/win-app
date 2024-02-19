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

using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Crypto.Contracts;

namespace ProtonVPN.Client.Logic.Auth;

public class UserHashGenerator : IUserHashGenerator
{
    private readonly IGlobalSettings _globalSettings;
    private readonly ISha1Calculator _sha1Calculator;

    public UserHashGenerator(
        IGlobalSettings globalSettings,
        ISha1Calculator sha1Calculator)
    {
        _globalSettings = globalSettings;
        _sha1Calculator = sha1Calculator;
    }

    public string? Generate()
    {
        string? userId = _globalSettings.UserId;
        return userId is null ? null : _sha1Calculator.Hash(userId);
    }
}