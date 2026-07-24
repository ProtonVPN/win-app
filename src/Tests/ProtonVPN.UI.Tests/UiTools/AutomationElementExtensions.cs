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
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FlaUI.Core.Exceptions;
using FlaUI.Core.AutomationElements;

namespace ProtonVPN.UI.Tests.UiTools;

public static class AutomationElementExtensions
{
    public static string DescribeAvailableElements(AutomationElement container)
    {
        try
        {
            AutomationElement[] descendants = container.FindAllDescendants();

            List<string> descriptions = descendants
                .Where(IsVisible)
                .Select(DescribeSingleElement)
                .Where(description => !string.IsNullOrEmpty(description))
                .ToList();

            return descriptions.Count == 0
                ? "none found."
                : string.Join(", ", descriptions);
        }
        catch (COMException)
        {
            return "unable to enumerate descendants (COM exception).";
        }
        catch (PropertyNotSupportedException)
        {
            return "unable to enumerate descendants (property not supported).";
        }
    }

    private static bool IsVisible(AutomationElement element)
    {
        try
        {
            return !element.IsOffscreen;
        }
        catch (COMException)
        {
            return false;
        }
        catch (PropertyNotSupportedException)
        {
            return false;
        }
    }

    private static string DescribeSingleElement(AutomationElement child)
    {
        string? name = TryGetProperty(() => child.Name);
        string? automationId = TryGetProperty(() => child.AutomationId);

        List<string> parts = new();

        if (!string.IsNullOrEmpty(name))
        {
            parts.Add($"Name='{name}'");
        }

        if (!string.IsNullOrEmpty(automationId))
        {
            parts.Add($"AutomationId='{automationId}'");
        }

        return parts.Count == 0 ? "" : $"[{string.Join(", ", parts)}]";
    }

    private static string? TryGetProperty(Func<string?> propertyAccessor)
    {
        try
        {
            return propertyAccessor();
        }
        catch (COMException)
        {
            return null;
        }
        catch (PropertyNotSupportedException)
        {
            return null;
        }
    }
}