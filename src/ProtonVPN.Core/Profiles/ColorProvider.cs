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

using System;

namespace ProtonVPN.Core.Profiles
{
    public class ColorProvider
    {
        private static readonly Random Random = new Random();

        private static readonly string[] ColorCodes = {
            "#F44236", "#E91D62", "#9C27B0", "#6739B6", "#3E50B4", "#2195F2", "#01BBD4",
            "#029587", "#8BC24A", "#CCDB38", "#FFE93B", "#FF7044", "#FF9700", "#607C8A",
        };

        public string RandomColor() => ColorCodes[Random.Next(TotalColors)];

        public int TotalColors => ColorCodes.Length;

        public string[] GetColors() => ColorCodes;
    }
}
