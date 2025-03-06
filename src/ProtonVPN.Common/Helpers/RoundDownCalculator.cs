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

namespace ProtonVPN.Common.Helpers
{
    public static class RoundDownCalculator
    {
        private const int NUM_OF_SIGNIFICANT_DIGITS = 2;

        public static int RoundDown(int originalValue)
        {
            if (originalValue == 0)
            {
                return 0;
            }

            bool isNegative = originalValue < 0;
            int value = Math.Abs(originalValue);

            int numberOfDigits = (int)Math.Floor(Math.Log10(value)) + 1;
            if (numberOfDigits <= 1)
            {
                return originalValue;
            }

            int numOfSignificantDigits = Math.Min(numberOfDigits - 1, NUM_OF_SIGNIFICANT_DIGITS);
            int placeValue = (int)Math.Pow(10, numberOfDigits - numOfSignificantDigits);
            int roundedDownResult = value - (value % placeValue);
            return isNegative ? -roundedDownResult : roundedDownResult;
        }
    }
}
