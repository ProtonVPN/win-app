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

using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers.Settings;

public class WindowSizeCategoryDimensionMapper : DimensionMapperBase, IWindowSizeCategoryDimensionMapper
{
    public const string FULLSCREEN = "fullscreen";
    public const string NORMAL = "normal";
    public const string NARROW_WIDTH = "narrow_width";
    public const string NARROW_HEIGHT = "narrow_height";
    public const string NARROW = "narrow";

    private const double NARROW_WIDTH_THRESHOLD = 0.80;
    private const double NARROW_HEIGHT_THRESHOLD = 0.95;

    public string Map(int windowWidth, int windowHeight, bool isMaximized)
    {
        if (isMaximized)
        {
            return FULLSCREEN;
        }

        bool isWidthNarrow = windowWidth < (int)(DefaultSettings.WindowWidth * NARROW_WIDTH_THRESHOLD);
        bool isHeightNarrow = windowHeight < (int)(DefaultSettings.WindowHeight * NARROW_HEIGHT_THRESHOLD);

        if (isWidthNarrow && isHeightNarrow)
        {
            return NARROW;
        }

        if (isWidthNarrow)
        {
            return NARROW_WIDTH;
        }

        if (isHeightNarrow)
        {
            return NARROW_HEIGHT;
        }

        return NORMAL;
    }
}