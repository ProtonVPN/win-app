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

using System.Collections.Generic;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Core.Servers
{
    public class ServerNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x.IsNullOrEmpty() && y.IsNullOrEmpty())
            {
                return 0;
            }

            if (x.IsNullOrEmpty())
            {
                return -1;
            }

            if (y.IsNullOrEmpty())
            {
                return 1;
            }

            if (x.IndexOf('#') == -1 || y.IndexOf('#') == -1)
            {
                return CompareNames(x, y);
            }

            return CompareNameWithHashSymbol(x, y);
        }

        private int CompareNames(string x, string y)
        {
            return x.CompareTo(y);
        }

        private int CompareNameWithHashSymbol(string x, string y)
        {
            int? number1 = GetNumber(x);
            int? number2 = GetNumber(y);
            if (!number1.HasValue || !number2.HasValue)
            {
                return CompareNames(x, y);
            }

            string name1 = GetName(x);
            string name2 = GetName(y);

            int nameCompare = name1.CompareTo(name2);
            if (nameCompare != 0)
            {
                return nameCompare;
            }

            return number1.Value.CompareTo(number2.Value);
        }

        private string GetName(string value)
        {
            return value.Substring(0, value.IndexOf('#'));
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
    }
}