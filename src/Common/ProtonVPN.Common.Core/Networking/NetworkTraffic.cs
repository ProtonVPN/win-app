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

namespace ProtonVPN.Common.Core.Networking;

public readonly struct NetworkTraffic
{
    public ulong BytesDownloaded { get; }
    public ulong BytesUploaded { get; }
    public DateTime UtcDate { get; }

    public static NetworkTraffic Zero { get; } = new(0, 0, DateTime.UtcNow);

    public NetworkTraffic(ulong bytesDownloaded, ulong bytesUploaded)
        : this(bytesDownloaded, bytesUploaded, DateTime.UtcNow)
    {
    }

    public NetworkTraffic(ulong bytesDownloaded, ulong bytesUploaded, DateTime utcDate)
    {
        BytesDownloaded = bytesDownloaded;
        BytesUploaded = bytesUploaded;
        UtcDate = utcDate;
    }

    public NetworkTraffic Copy(DateTime utcDate)
    {
        return new(BytesDownloaded, BytesUploaded, utcDate);
    }

    public ulong Sum()
    {
        return BytesDownloaded + BytesUploaded;
    }

    public static NetworkTraffic operator -(NetworkTraffic current, NetworkTraffic previous)
    {
        return new NetworkTraffic(
            bytesDownloaded: GetDifference(current.BytesDownloaded, previous.BytesDownloaded),
            bytesUploaded: GetDifference(current.BytesUploaded, previous.BytesUploaded),
            current.UtcDate
        );
    }

    private static ulong GetDifference(ulong currentBytes, ulong lastBytes)
    {
        return currentBytes > lastBytes
            ? currentBytes - lastBytes
            : 0;
    }
}