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
using System.Linq;

namespace ProtonVPN.Settings.Migrations.v1_7_2
{
    internal class MigratedColorCode
    {
        public static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            { "#E31C1C", "#E91D62" },
            { "#C06565", "#F44236" },
            { "#D41BB8", "#9C27B0" },
            { "#B34FA4", "#9C27B0" },
            { "#9F48DB", "#6739B6" },
            { "#9468B2", "#6739B6" },
            { "#5E70DB", "#6739B6" },
            { "#6770A7", "#3E50B4" },
            { "#31C5CA", "#01BBD4" },
            { "#559193", "#029587" },
            { "#37BA62", "#8BC24A" },
            { "#59946D", "#029587" },
            { "#97BA2E", "#8BC24A" },
            { "#909F63", "#CCDB38" },
            { "#E0BE48", "#FFE93B" },
            { "#A3945F", "#607C8A" },
            { "#D97216", "#FF9700" },
            { "#AA7D55", "#607C8A" }
        };

        private static readonly Random Random = new Random();

        private readonly string _color;

        public MigratedColorCode(string color)
        {
            _color = color ?? "";
        }

        public static implicit operator string(MigratedColorCode item) => item.ToString();

        public override string ToString()
        {
            var key = _color.ToUpperInvariant();
            return Map.ContainsKey(key) 
                ? Map[key] 
                : Map.Values.ElementAt(Random.Next(Map.Count));
        }
    }
}
