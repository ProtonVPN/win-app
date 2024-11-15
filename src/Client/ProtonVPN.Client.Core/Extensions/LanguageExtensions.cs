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

using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace ProtonVPN.Client.Core.Extensions;

public static class LanguageExtensions
{
    private static readonly List<string> _rightToLeftLanguages = ["fa-IR"];

    public static FlowDirection GetFlowDirection(this string language)
    {
        return _rightToLeftLanguages.Contains(language)
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
    }
}