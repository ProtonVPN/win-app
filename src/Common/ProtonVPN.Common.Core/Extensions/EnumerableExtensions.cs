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

namespace ProtonVPN.Common.Core.Extensions;

public static class EnumerableExtensions
{
    // Code forked and edited from System.Linq.Enumerable (First.cs)
    public static T? FirstOrNull<T>(this IEnumerable<T> source)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        if (source is IList<T> list)
        {
            if (list.Count > 0)
            {
                return list[0];
            }
        }
        else
        {
            using (IEnumerator<T> e = source.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    return e.Current;
                }
            }
        }

        return null;
    }

    // Code forked and edited from System.Linq.Enumerable (First.cs)
    public static T? FirstOrNull<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        foreach (T element in source)
        {
            if (predicate(element))
            {
                return element;
            }
        }

        return null;
    }
}