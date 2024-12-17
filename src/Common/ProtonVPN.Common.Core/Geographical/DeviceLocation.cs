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

namespace ProtonVPN.Common.Core.Geographical;

public struct DeviceLocation
{
    public static DeviceLocation Unknown => new();

    public string IpAddress { get; init; }
    public string CountryCode { get; init; }
    public string Isp { get; init; }

    public static bool operator ==(DeviceLocation? dl1, DeviceLocation? dl2)
    {
        return (dl1 is null && dl2 is null) || 
               (dl1 is not null && dl2 is not null && dl1.Equals(dl2));
    }

    public override bool Equals(object o)
    {
        if (o == null)
        {
            return false;
        }

        DeviceLocation deviceLocation = (DeviceLocation)o;
        return IpAddress == deviceLocation.IpAddress &&
               (CountryCode == deviceLocation.CountryCode);
    }

    public override int GetHashCode()
    {
        return Tuple.Create(IpAddress, CountryCode).GetHashCode();
    }

    public static bool operator !=(DeviceLocation? dl1, DeviceLocation? dl2)
    {
        return (dl1 is null && dl2 is not null) || 
               (dl1 is not null && dl2 is null) || 
               (dl1 is not null && dl2 is not null && !dl1.Equals(dl2));
    }
}