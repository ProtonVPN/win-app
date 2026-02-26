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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Client.Localization.Tests.Helpers;

namespace ProtonVPN.Client.Localization.Tests;

[TestClass]
public class ResourceKeyValidationTests
{
    /// <summary>
    /// Keys matching these prefixes are excluded from the "unused keys" check
    /// </summary>
    private static readonly string[] _dynamicKeyPrefixes =
    {
        "Country_val_",
        "Settings_Theme_",
        "Settings_Connection_Default_",
        "Settings_SelectedProtocol_",
    };

    [TestMethod]
    [Ignore("Temporarily disabled. Enable when required.")]
    public void EnUsResw_ShouldNotContainDuplicateValues()
    {
        Dictionary<string, List<string>> duplicates =
            ReswFileParser.GetDuplicateValues(SourcePathResolver.EnUsReswPath);

        List<string> violations = duplicates
            .OrderBy(kv => kv.Key)
            .Select(kv => $"\"{kv.Key}\" is shared by: {string.Join(", ", kv.Value)}")
            .ToList();

        violations.Should().BeEmpty(
            $"Found {duplicates.Count} duplicated value(s):\n" +
            string.Join("\n", violations.Select(v => $"  • {v}")));
    }

    [TestMethod]
    public void EnUsResw_ShouldNotContainUnusedKeys()
    {
        HashSet<string> definedKeys = ReswFileParser.GetResourceKeys(SourcePathResolver.EnUsReswPath);
        HashSet<string> referencedKeys =
            SourceKeyScanner.CollectReferencedKeys(SourcePathResolver.SourceRoot, definedKeys);

        List<string> unusedKeys = definedKeys
            .Where(key => !IsKeyReferencedOrDynamic(key, referencedKeys))
            .OrderBy(k => k)
            .ToList();

        unusedKeys.Should().BeEmpty(
            $"Found {unusedKeys.Count} unused key(s):\n" +
            string.Join("\n", unusedKeys.Select(k => $"  • {k}")));
    }

    private static bool IsKeyReferencedOrDynamic(string key, HashSet<string> referencedKeys)
    {
        if (referencedKeys.Contains(key))
        {
            return true;
        }

        foreach (string prefix in _dynamicKeyPrefixes)
        {
            if (key.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return PluralKeyHelper.IsPluralVariantOfReferencedKey(key, referencedKeys);
    }
}