/*
 * Copyright (c) 2023 Proton AG
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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Resource.Colors;

namespace ProtonVPN.Core.Profiles
{
    public class ColorProvider
    {
        private static readonly Random Random = new();
        private static readonly string[] ColorResourceKeys =
        {
            "ProfileColorRed", "ProfileColorMagenta", "ProfileColorViolet", "ProfileColorPurple",
            "ProfileColorNavy", "ProfileColorBlue", "ProfileColorCyan", "ProfileColorTeal",
            "ProfileColorGreen", "ProfileColorLime", "ProfileColorYellow", "ProfileColorOrange",
            "ProfileColorGold", "ProfileColorGray",
        };

        private readonly Lazy<string[]> _colorCodes;
        private readonly IColorPalette _colorPalette;

        public ColorProvider(IColorPalette colorPalette)
        {
            _colorPalette = colorPalette;
            _colorCodes = new(() => CreateColorEnumerable().ToArray());
        }

        private IEnumerable<string> CreateColorEnumerable()
        {
            foreach (string colorResourceKey in ColorResourceKeys)
            {
                yield return _colorPalette.GetStringByResourceName(colorResourceKey);
            }
        }

        public string GetRandomColor() => _colorCodes.Value[Random.Next(GetNumOfColors)];

        public int GetNumOfColors => _colorCodes.Value.Length;

        public string[] GetColors() => _colorCodes.Value;
        
        public string GetRandomColorIfInvalid(string colorCode)
        {
            return colorCode.IsColorCodeValid() ? colorCode : GetRandomColor();
        }
    }
}