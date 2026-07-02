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
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProtonVPN.UI.Tests.Enums;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class LanguageHelper
{
    public static Language CurrentLanguage { get; set; } = Language.English;

    public static readonly bool ForceLanguageChange = false;

    private static readonly bool _isSli = AppDomain.CurrentDomain.BaseDirectory.Contains("performance-testing");

    private static readonly string _stringsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _isSli ? @"..\..\..\win-app\src" : @"..\..", @"Client\Localization\ProtonVPN.Client.Localization\Strings");

    public static string GetTranslatedString(string key, Language? language = null)
    {
        language ??= CurrentLanguage;

        string filePath = Path.Combine(_stringsPath, language.Value.GetCode(), "Resources.resw");
        XDocument doc = XDocument.Load(filePath);

        return doc.Descendants("data")
            .Where(x => x.Attribute("name")?.Value == key)
            .Select(x => x.Element("value")?.Value)
            .FirstOrDefault() ?? throw new KeyNotFoundException($"Key '{key}' not found for language '{language}'");
    }

    public static string GetCode(this Language language)
    {
        return language.GetType().GetField(language.ToString())!.GetCustomAttribute<DisplayAttribute>()!.ShortName!;
    }

    public static string GetFullName(this Language language)
    {
        return language.GetType().GetField(language.ToString())!.GetCustomAttribute<DisplayAttribute>()!.Name!;
    }
}