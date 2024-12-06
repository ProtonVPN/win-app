/*
 * Copyright (c) 2024 Proton AG
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

namespace ProtonVPN.Client.UI.Main.Home.Details;

public static class EmptyValueExtensions
{
    public const string DEFAULT = "-";

    public static string GetValueOrDefault(this string? text)
    {
        return string.IsNullOrWhiteSpace(text) ? DEFAULT : text;
    }

    public static string TransformValueOrDefault(this string? text, Func<string,string> func)
    {
        return string.IsNullOrWhiteSpace(text) ? DEFAULT : func(text);
    }
}
