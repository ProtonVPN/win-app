/*
 * Copyright (c) 2025 Proton AG
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

using ProtonVPN.Api.Contracts.Announcements;
using ProtonVPN.Client.Files.Contracts.Images;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Announcements.EntityMapping;

public class AnnouncementMapper : IMapper<AnnouncementResponse, Announcement>
{
    private readonly ILogger _logger;
    private readonly IEntityMapper _entityMapper;
    private readonly IImageCache _imageCache;

    public AnnouncementMapper(ILogger logger,
        IEntityMapper entityMapper,
        IImageCache imageCache)
    {
        _logger = logger;
        _entityMapper = entityMapper;
        _imageCache = imageCache;
    }

    public Announcement Map(AnnouncementResponse leftEntity)
    {
        try
        {
            Announcement announcement = CreateAnnouncement(leftEntity);
            if (announcement is not null && !HasFailedFullscreenImage(leftEntity, announcement))
            {
                return announcement;
            }
            else if (announcement is not null)
            {
                _logger.Error<AppLog>($"Failed to fetch full screen image of announcement with ID '{leftEntity.Id}'.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("Failed to map an Announcement.", ex);
        }
        return null;
    }

    private bool HasFailedFullscreenImage(AnnouncementResponse leftEntity, Announcement announcement)
    {
        return leftEntity.Offer?.Panel?.FullScreenImage?.Source?.Count > 0 && 
               announcement?.Panel?.FullScreenImage?.Image is null;
    }

    private Announcement CreateAnnouncement(AnnouncementResponse announcementResponse)
    {
        return new()
        {
            Id = announcementResponse.Id,
            Type = (AnnouncementType)announcementResponse.Type,
            Reference = announcementResponse.Reference,
            StartDateTimeUtc = MapTimestampToDateTimeUtc(announcementResponse.StartTimestamp),
            EndDateTimeUtc = MapTimestampToDateTimeUtc(announcementResponse.EndTimestamp),
            Url = announcementResponse.Offer?.Url,
            Icon = !string.IsNullOrEmpty(announcementResponse.Offer?.Icon)
                ? _imageCache.Get(AnnouncementConstants.STORAGE_FOLDER, announcementResponse.Offer.Icon)
                : null,
            Label = announcementResponse.Offer?.Label,
            Panel = announcementResponse.Offer?.Panel is null
                ? _entityMapper.Map<ProminentBannerResponse, Panel>(announcementResponse.Offer?.ProminentBanner)
                : _entityMapper.Map<OfferPanelResponse, Panel>(announcementResponse.Offer?.Panel),
            Seen = false,
            ShowCountdown = announcementResponse.Offer?.Panel?.ShowCountdown ?? false,
            IsDismissible = announcementResponse.Offer?.Panel?.IsDismissible ?? true
        };
    }

    private DateTime MapTimestampToDateTimeUtc(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
    }

    public AnnouncementResponse Map(Announcement rightEntity)
    {
        throw new NotImplementedException("We don't need to map to API responses.");
    }
}