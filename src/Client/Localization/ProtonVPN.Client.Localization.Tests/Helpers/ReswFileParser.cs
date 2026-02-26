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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ProtonVPN.Client.Localization.Tests.Helpers;

public static class ReswFileParser
{
    public static HashSet<string> GetResourceKeys(string reswPath)
    {
        XDocument doc = XDocument.Load(reswPath);
        return doc.Descendants("data")
            .Select(e => e.Attribute("name")?.Value)
            .Where(name => name != null)
            .ToHashSet()!;
    }

    public static Dictionary<string, List<string>> GetDuplicateValues(string reswPath)
    {
        XDocument doc = XDocument.Load(reswPath);
        return doc.Descendants("data")
            .Where(e => e.Attribute("name") != null && e.Element("value") != null)
            .GroupBy(
                e => e.Element("value")!.Value.Trim(),
                e => e.Attribute("name")!.Value)
            .Where(g => g.Count() > 1)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
