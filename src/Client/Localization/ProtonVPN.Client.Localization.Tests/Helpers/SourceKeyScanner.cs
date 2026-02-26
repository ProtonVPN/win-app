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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProtonVPN.Client.Localization.Tests.Helpers;

public static partial class SourceKeyScanner
{
    [GeneratedRegex(@"_?[Ll]ocalizer\s*\.\s*Get(?:Format|Plural(?:Format)?)?\s*\(\s*""([^""]+)""")]
    private static partial Regex CsLocalizerPattern();

    [GeneratedRegex(@"Localizer\.Get(?:Format|Plural(?:Format)?)?\('([^']+)'")]
    private static partial Regex XamlLocalizerPattern();

    [GeneratedRegex(@"""([A-Z][A-Za-z0-9_]+)""")]
    private static partial Regex CsStringLiteralPattern();

    [GeneratedRegex(@"'([A-Z][A-Za-z0-9_]+)'")]
    private static partial Regex XamlStringLiteralPattern();

    public static HashSet<string> CollectReferencedKeys(string sourceRoot, HashSet<string> definedKeys)
    {
        HashSet<string> keys = new();
        HashSet<string> pluralBaseKeys = PluralKeyHelper.GetPluralBaseKeys(definedKeys);

        foreach (string file in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
                     .Where(path => !IsIgnoredPath(path)))
        {
            string content = File.ReadAllText(file);
            ExtractMatches(content, CsLocalizerPattern(), keys);
            ExtractDefinedLiterals(content, CsStringLiteralPattern(), definedKeys, pluralBaseKeys, keys);
        }

        foreach (string file in Directory.EnumerateFiles(sourceRoot, "*.xaml", SearchOption.AllDirectories)
                     .Where(path => !IsIgnoredPath(path)))
        {
            string content = File.ReadAllText(file);
            ExtractMatches(content, XamlLocalizerPattern(), keys);
            ExtractDefinedLiterals(content, XamlStringLiteralPattern(), definedKeys, pluralBaseKeys, keys);
        }

        return keys;
    }

    private static void ExtractMatches(string content, Regex pattern, HashSet<string> keys)
    {
        foreach (Match match in pattern.Matches(content))
        {
            keys.Add(match.Groups[1].Value);
        }
    }

    private static void ExtractDefinedLiterals(
        string content,
        Regex pattern,
        HashSet<string> definedKeys,
        HashSet<string> pluralBaseKeys,
        HashSet<string> keys)
    {
        foreach (Match match in pattern.Matches(content))
        {
            string literal = match.Groups[1].Value;
            if (definedKeys.Contains(literal) || pluralBaseKeys.Contains(literal))
            {
                keys.Add(literal);
            }
        }
    }

    private static bool IsIgnoredPath(string path)
    {
        return path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}