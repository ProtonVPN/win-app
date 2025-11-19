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

using ProtonVPN.StatisticalEvents.Dimensions.Constants;

namespace ProtonVPN.StatisticalEvents.Dimensions.Extensions;

public static class CountDimensionExtensions
{
    public static string ToDnsCountDimension(this int count)
    {
        return count switch
        {
            0 => CountDimensionConstants.ZERO,
            1 => CountDimensionConstants.ONE,
            >= 2 and <= 4 => CountDimensionConstants.TWO_TO_FOUR,
            _ => CountDimensionConstants.FIVE_OR_MORE
        };
    }

    public static string ToSplitTunnelingCountDimension(this int count)
    {
        return count switch
        {
            0 => CountDimensionConstants.ZERO,
            1 => CountDimensionConstants.ONE,
            >= 2 and <= 4 => CountDimensionConstants.TWO_TO_FOUR,
            >= 5 and <= 9 => CountDimensionConstants.FIVE_TO_NINE,
            >= 10 and <= 19 => CountDimensionConstants.TEN_TO_NINETEEN,
            _ => CountDimensionConstants.TWENTY_OR_MORE
        };
    }
}