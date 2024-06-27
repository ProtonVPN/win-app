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

using ProtonVPN.Client.Common.Enums;

namespace ProtonVPN.Client.Common.Helpers;

public static class ByteConversionHelper
{
    private const double KILOBYTES_SIZE_IN_BYTES = 1000.0;

    public static (double size, ByteMetrics metric) CalculateSize(long sizeInBytes)
    {
        if (sizeInBytes == 0)
        {
            return (0, ByteMetrics.Bytes);
        }

        long bytes = Math.Abs(sizeInBytes);
        int index = Math.Min(6, Convert.ToInt32(Math.Floor(Math.Log(bytes, KILOBYTES_SIZE_IN_BYTES))));

        double result = bytes / Math.Pow(KILOBYTES_SIZE_IN_BYTES, index);

        // Due to rounding, the size might need to be converted to the next metric instead.
        if(Math.Round(result) >= KILOBYTES_SIZE_IN_BYTES)
        {
            index++;
            result /= KILOBYTES_SIZE_IN_BYTES;
        }

        // When 2 digits or above, round to the next integer. Otherwise, display one decimal.
        int rounding = result < 10 ? 1 : 0;

        return (Math.Sign(sizeInBytes) * Math.Round(result, rounding), (ByteMetrics)index);
    }

    public static double GetScaleFactor(ByteMetrics metric)
    {
        return Math.Pow(1000, (int)metric);
    }
}