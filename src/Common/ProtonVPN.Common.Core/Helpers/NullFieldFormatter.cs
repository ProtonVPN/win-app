/*
 * Copyright (c) 2026 Proton AG
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

namespace ProtonVPN.Common.Core.Helpers;

public static class NullFieldFormatter
{
    /// <summary>
    /// Returns a formatted string listing the names of null fields, or <c>null</c> if all values are non-null.
    /// Each parameter is a tuple of (name, value).
    /// </summary>
    public static string? FormatNullFields(params (string Name, object? Value)[] fields)
    {
        string[] nullFields = fields
            .Where(f => f.Value is null)
            .Select(f => f.Name)
            .ToArray();

        return nullFields.Length > 0 ? string.Join(", ", nullFields) : null;
    }
}