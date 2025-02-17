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

using System.Text.RegularExpressions;

namespace ProtonVPN.Client.Common.Extensions;

public static class StringExtensions
{
    public static int GetServerNumber(this string serverName)
    {
        Match match = new Regex(@"#(\d+)").Match(serverName);
        if (match.Success)
        {
            string numberString = match.Groups[1].Value;
            if (int.TryParse(numberString, out int serverNumber))
            {
                return serverNumber;
            }
        }

        return 0;
    }

    public static string GetServerTag(this string serverName)
    {
        Match match = new Regex(@"^(.*?)(?=#)").Match(serverName);
        return match.Success
            ? match.Groups[1].Value
            : string.Empty;
    }
}