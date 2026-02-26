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

using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtonVPN.Client.Localization.Tests.Helpers;

public static class PluralKeyHelper
{
    private static readonly string[] _pluralSuffixes =
    {
        "_Zero",
        "_One",
        "_Two",
        "_Few",
        "_Many",
        "_Other",
    };

    public static HashSet<string> GetPluralBaseKeys(HashSet<string> allKeys)
    {
        Dictionary<string, int> baseCounts = new();

        foreach (string key in allKeys)
        {
            foreach (string suffix in _pluralSuffixes)
            {
                if (key.EndsWith(suffix, StringComparison.Ordinal))
                {
                    string baseKey = key[..^suffix.Length];
                    baseCounts.TryGetValue(baseKey, out int count);
                    baseCounts[baseKey] = count + 1;
                    break;
                }
            }
        }

        return baseCounts
            .Where(kv => kv.Value >= 2)
            .Select(kv => kv.Key)
            .ToHashSet();
    }

    public static bool IsPluralVariantOfReferencedKey(string key, HashSet<string> referencedKeys)
    {
        foreach (string suffix in _pluralSuffixes)
        {
            if (key.EndsWith(suffix, StringComparison.Ordinal))
            {
                string baseKey = key[..^suffix.Length];
                return referencedKeys.Contains(baseKey);
            }
        }

        return false;
    }
}
