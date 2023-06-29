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
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Core.Servers
{
    public class ServerNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (string.IsNullOrEmpty(x) && string.IsNullOrEmpty(y))
            {
                return 0;
            }
            if (string.IsNullOrEmpty(x))
            {
                return -1;
            }
            if (string.IsNullOrEmpty(y))
            {
                return 1;
            }
            if (!x.Contains('#') || !y.Contains('#'))
            {
                return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
            }

            return CompareNameWithHashSymbol(x, y);
        }

        private int CompareNameWithHashSymbol(string x, string y)
        {
            int preHashtagNameComparisonResult = ComparePreHashtagNames(x, y);
            if (preHashtagNameComparisonResult != 0)
            {
                return preHashtagNameComparisonResult;
            }

            int numberComparisonResult = CompareNumbers(x, y);
            if (numberComparisonResult != 0)
            {
                return numberComparisonResult;
            }

            return ComparePostHashtagNames(x, y);
        }

        private int ComparePreHashtagNames(string x, string y)
        {
            string preHashtagName1 = GetPreHashtagName(x);
            string preHashtagName2 = GetPreHashtagName(y);

            return preHashtagName1.CompareTo(preHashtagName2);
        }

        private string GetPreHashtagName(string value)
        {
            return value.Substring(0, value.IndexOf('#'));
        }

        private int CompareNumbers(string x, string y)
        {
            int number1 = GetNumber(x) ?? int.MaxValue;
            int number2 = GetNumber(y) ?? int.MaxValue;

            return number1.CompareTo(number2);
        }

        private int? GetNumber(string value)
        {
            string stringAfterHash = value.Substring(value.IndexOf('#') + 1);
            int indexOfDash = stringAfterHash.IndexOf('-');
            string substring = indexOfDash > -1 ? stringAfterHash.Substring(0, indexOfDash) : stringAfterHash;
            return ParseNumber(substring);
        }

        private int? ParseNumber(string value)
        {
            return int.TryParse(value, out int number) ? number : null;
        }

        private int ComparePostHashtagNames(string x, string y)
        {
            string postHashtagName1 = GetPostHashtagName(x);
            string postHashtagName2 = GetPostHashtagName(y);

            return postHashtagName1.CompareTo(postHashtagName2);
        }

        private string GetPostHashtagName(string value)
        {
            return value.Substring(value.IndexOf('#') + 1);
        }
    }
}