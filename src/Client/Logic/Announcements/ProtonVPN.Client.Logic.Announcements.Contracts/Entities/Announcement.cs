/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Client.Files.Contracts.Images;

namespace ProtonVPN.Client.Logic.Announcements.Contracts.Entities;

public class Announcement
{
    public required string Id { get; set; }
    public AnnouncementType Type { get; set; }
    public string? Reference { get; set; }
    public DateTime StartDateTimeUtc { get; set; }
    public DateTime EndDateTimeUtc { get; set; }
    public string? Url { get; set; }
    public CachedImage? Icon { get; set; }
    public string? Label { get; set; }
    public Panel? Panel { get; set; }
    public bool Seen { get; set; }
    public bool ShowCountdown { get; set; }
    public bool IsDismissible { get; set; }

    public bool IsActive()
    {
        DateTime utcNow = DateTime.UtcNow;
        return StartDateTimeUtc <= utcNow && EndDateTimeUtc > utcNow;
    }

    public bool IsActiveAndUnseen()
    {
        return !Seen && IsActive();
    }
}
