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

namespace ProtonVPN.Common.Helpers
{
    public struct HashCode
    {
        private readonly int _value;

        public HashCode(int value) => _value = value;

        public static HashCode Start { get; } = new HashCode(17);

        public static implicit operator int(HashCode hash) => hash._value;

        public HashCode Hash<T>(T obj)
        {
            var h = EqualityComparer<T>.Default.GetHashCode(obj);
            return unchecked(new HashCode((_value * 31) + h));
        }

        public override int GetHashCode() => _value;
    }
}
