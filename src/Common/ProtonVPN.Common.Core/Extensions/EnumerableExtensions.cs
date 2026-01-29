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

    /// <summary>
    /// Runs <paramref name="action"/> on each element in <paramref name="source"/> sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to run <paramref name="action"/> on.</param>
    /// <param name="action">An action to run on each element of <paramref name="source"/> sequence.</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T? item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Returns distinct elements from a sequence by using a specified selector to compare values.
    /// </summary>
    /// <typeparam name="T">The type of elements in a sequence.</typeparam>
    /// <typeparam name="TSelected">The type of the value returned by the selector to determine unique elements.</typeparam>
    /// <param name="source">The sequence of source elements.</param>
    /// <param name="selector">A function that selects a value to determine unique elements by.</param>
    /// <returns>An <see cref="T:System.Collections.Generic.IEnumerable"/> that contains distinct elements from the source sequence.</returns>
    public static IEnumerable<T> Distinct<T, TSelected>(this IEnumerable<T> source, Func<T, TSelected> selector)
    {
        HashSet<TSelected> set = [];

        foreach (T? item in source)
        {
            TSelected? selectedValue = selector(item);

            if (set.Add(selectedValue))
            {
                yield return item;
            }
        }
    }
}