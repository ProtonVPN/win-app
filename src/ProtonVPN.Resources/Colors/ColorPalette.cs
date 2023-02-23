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
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace ProtonVPN.Resource.Colors
{
    public class ColorPalette : IColorPalette
    {
        private static readonly Regex HexArgb32Regex = new("^#[0-9a-fA-F]{8}$");

        private readonly IApplicationResourcesLoader _applicationResourcesLoader;

        public ColorPalette(IApplicationResourcesLoader applicationResourcesLoader)
        {
            _applicationResourcesLoader = applicationResourcesLoader;
        }

        public Color GetColorByResourceName(string resourceName)
        {
            return _applicationResourcesLoader.GetByName<Color>(resourceName);
        }

        public SolidColorBrush GetSolidColorBrushByResourceName(string resourceName)
        {
            return _applicationResourcesLoader.GetByName<SolidColorBrush>(resourceName);
        }

        public string GetStringByResourceName(string resourceName)
        {
            object resource = _applicationResourcesLoader.GetByName(resourceName);
            if (resource is SolidColorBrush or Color)
            {
                return ConvertArgb32ToRgb32IfAlphaIsNotUsed(resource.ToString());
            }

            throw new Exception($"The resource '{resourceName}' exists but its not a SolidColorBrush or a Color.");
        }

        private string ConvertArgb32ToRgb32IfAlphaIsNotUsed(string colorHex)
        {
            return IsArgb32AlphaNotUsed(colorHex) ? colorHex.Replace("#FF", "#") : colorHex;
        }

        private bool IsArgb32AlphaNotUsed(string colorHex)
        {
            return IsArgb32(colorHex) && colorHex.StartsWith("#FF");
        }

        private bool IsArgb32(string colorHex)
        {
            return HexArgb32Regex.IsMatch(colorHex);
        }
    }
}