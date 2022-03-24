/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Text.RegularExpressions;

namespace ProtonVPN.Core.Servers
{
    internal class ServerNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x.IndexOf('#') == -1 || y.IndexOf('#') == -1)
            {
                return x.CompareTo(y);
            }

            var name1 = x.Substring(0, x.IndexOf('#'));
            var name2 = y.Substring(0, y.IndexOf('#'));

            var number1 = x.Substring(x.IndexOf('#') + 1);
            number1 = Regex.Match(number1, @"\d+").Value;
            var key1 = int.Parse(number1);

            var number2 = y.Substring(y.IndexOf('#') + 1);
            number2 = Regex.Match(number2, @"\d+").Value;
            var key2 = int.Parse(number2);

            var nameCompare = name1.CompareTo(name2);
            if (nameCompare != 0 && key1 < 100 && key2 < 100 || key1 > 100 && key2 > 100)
            {
                return nameCompare;
            }

            return key1.CompareTo(key2);
        }
    }
}
