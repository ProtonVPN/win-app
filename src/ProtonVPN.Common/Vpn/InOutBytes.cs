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

namespace ProtonVPN.Common.Vpn
{
    public struct InOutBytes
    {
        public InOutBytes(double bytesIn, double bytesOut)
        {
            BytesIn = bytesIn;
            BytesOut = bytesOut;
        }

        public double BytesIn { get; }

        public double BytesOut { get; }

        public static InOutBytes Zero { get; } = new InOutBytes(0, 0);

        public static InOutBytes operator -(InOutBytes op1, InOutBytes op2)
        {
            return new InOutBytes(op1.BytesIn - op2.BytesIn, op1.BytesOut - op2.BytesOut);
        }
    }
}