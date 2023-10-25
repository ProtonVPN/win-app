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

namespace ProtonVPN.Common.Legacy.Extensions;

public static class ExponentialExtensions
{
    public static string ToShortString(this long number, string unit, UnitSpacing spacing)
    {
        int numOfDigits = Math.Abs(number).ToString().Length;
        double shortValue = GenerateShortValue(number);

        if (Math.Abs(shortValue).ToString().Length > 3) // Ex.: number=999500, shortValue=1000
        {
            numOfDigits++;
            shortValue = GenerateShortValue(shortValue);
        }

        int thousandsExponent = (numOfDigits - 1) / 3;
        string metricPrefix = GetMetricPrefix(thousandsExponent);

        return $"{shortValue}{(spacing is UnitSpacing.WithSpace ? " " : "")}{metricPrefix}{unit}";
    }

    /// <summary>
    /// Returns the highest value digits, already prepared for the metric prefix
    /// Ex.: 9876 > 9.9, 54321 > 54, 123456 > 123, Corner case: 999500 > 1000
    /// </summary>
    private static double GenerateShortValue(double number)
    {
        int numOfDigits = Math.Abs(number).ToString().Length;
        int remainderOfNumOfDigits = numOfDigits % 3;
        if (remainderOfNumOfDigits == 0)
        {
            return Math.Round(number / Math.Pow(10, numOfDigits - 3));
        }
        else if (remainderOfNumOfDigits == 1)
        {
            return Math.Round(number / Math.Pow(10, numOfDigits - 2)) / 10;
        }
        else // remainderOfNumOfDigits == 2
        {
            return Math.Round(number / Math.Pow(10, numOfDigits - 2));
        }
    }

    public enum UnitSpacing
    {
        WithSpace,
        WithoutSpace,
    }

    /// <summary>Returns the metric prefix for 1000^X, with 0 being empty, 1 being K (Kilo), 2 being M (Mega)...</summary>
    private static string GetMetricPrefix(int unit)
    {
        switch (unit)
        {
            case 0:
                return string.Empty;
            case 1:
                return "K";
            case 2:
                return "M";
            case 3:
                return "G";
            case 4:
                return "T";
            case 5:
                return "P";
            case 6:
                return "E";
            case 7:
                return "Z";
            case 8:
                return "Y";
            case 9:
                return "R";
            case 10:
                return "Q";
            default:
                return "ERR";
        }
    }
}