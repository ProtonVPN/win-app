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
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Announcements.Files;

namespace ProtonVPN.Client.Logic.Announcements.Images;

public class AnnouncementImagesDeleter : IAnnouncementImagesDeleter
{
    private readonly IImageCache _imageCache;
    private readonly IAnnouncementsFileReaderWriter _announcementsFileReaderWriter;

    public AnnouncementImagesDeleter(IImageCache imageCache,
        IAnnouncementsFileReaderWriter announcementsFileReaderWriter)
    {
        _imageCache = imageCache;
        _announcementsFileReaderWriter = announcementsFileReaderWriter;
    }

    public void DeleteUnused()
    {
        IDictionary<string, List<Announcement>> announcementsByUser = _announcementsFileReaderWriter.ReadAllUsers();
        IList<CachedImage> existingCachedImages = _imageCache.GetAllFromFolder(AnnouncementConstants.STORAGE_FOLDER);

        List<CachedImage> usedCachedImages = GetUsedCachedImagesFromAnnouncements(announcementsByUser);
        foreach (CachedImage usedCachedImage in usedCachedImages)
        {
            existingCachedImages.Remove(usedCachedImage);
        }

        foreach (CachedImage imageFilePath in existingCachedImages)
        {
            _imageCache.Delete(imageFilePath);
        }
    }

    private List<CachedImage> GetUsedCachedImagesFromAnnouncements(IDictionary<string, List<Announcement>> announcementsByUser)
    {
        List<CachedImage> usedCachedImages = [];
        foreach (KeyValuePair<string, List<Announcement>> userAnnouncements in announcementsByUser)
        {
            if (userAnnouncements.Value is not null)
            {
                foreach (Announcement announcement in userAnnouncements.Value)
                {
                    usedCachedImages.AddRange(GetUsedCachedImagesFromAnnouncement(announcement));
                }
            }
        }
        return usedCachedImages;
    }

    private IEnumerable<CachedImage> GetUsedCachedImagesFromAnnouncement(Announcement announcement)
    {
        CachedImage? icon = announcement?.Icon;
        if (icon.HasValue)
        {
            yield return icon.Value;
        }

        CachedImage? panelFullScreenImage = announcement?.Panel?.FullScreenImage?.Image;
        if (panelFullScreenImage.HasValue)
        {
            yield return panelFullScreenImage.Value;
        }

        CachedImage? panelFullScreenImageLight = announcement?.Panel?.FullScreenImage?.ImageLight;
        if (panelFullScreenImageLight.HasValue)
        {
            yield return panelFullScreenImageLight.Value;
        }

        CachedImage? panelPicture = announcement?.Panel?.Picture;
        if (panelPicture.HasValue)
        {
            yield return panelPicture.Value;
        }

        List<PanelFeature>? features = announcement?.Panel?.Features;
        if (features is not null)
        {
            foreach (PanelFeature feature in features)
            {
                CachedImage? featureIcon = feature?.Icon;
                if (featureIcon.HasValue)
                {
                    yield return featureIcon.Value;
                }
            }
        }
    }
}
