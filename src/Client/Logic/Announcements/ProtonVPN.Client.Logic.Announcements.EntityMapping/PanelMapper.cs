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

using ProtonVPN.Api.Contracts.Announcements;
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Files.Contracts.Images;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Announcements.EntityMapping;

public class PanelMapper : IMapper<OfferPanelResponse, Panel>
{
    private const string INCENTIVE_PRICE_TAG = "%IncentivePrice%";

    private readonly IEntityMapper _entityMapper;
    private readonly IImageCache _imageCache;

    public PanelMapper(IEntityMapper entityMapper, IImageCache imageCache)
    {
        _entityMapper = entityMapper;
        _imageCache = imageCache;
    }

    public Panel Map(OfferPanelResponse leftEntity)
    {
        if (leftEntity is null)
        {
            return null;
        }

        string incentive = leftEntity?.Incentive;
        string incentiveSuffix = null;
        int? indexOfIncentivePriceTag = leftEntity?.Incentive?.IndexOf(INCENTIVE_PRICE_TAG, StringComparison.InvariantCulture);

        if (indexOfIncentivePriceTag is >= 0)
        {
            incentive = leftEntity.Incentive.Substring(0, indexOfIncentivePriceTag.Value).TrimEnd();
            incentiveSuffix = leftEntity.Incentive.Substring(indexOfIncentivePriceTag.Value + INCENTIVE_PRICE_TAG.Length).TrimStart();
        }

        return new()
        {
            Incentive = incentive,
            IncentivePrice = leftEntity?.IncentivePrice,
            IncentiveSuffix = incentiveSuffix,
            Description = leftEntity?.Pill,
            Picture = _imageCache.Get(AnnouncementConstants.STORAGE_FOLDER, leftEntity?.PictureUrl),
            Title = leftEntity?.Title,
            Features = _entityMapper.Map<OfferPanelFeatureResponse, PanelFeature>(leftEntity?.Features),
            FeaturesFooter = leftEntity?.FeaturesFooter,
            Button = _entityMapper.Map<OfferPanelButtonResponse, PanelButton>(leftEntity?.Button),
            PageFooter = leftEntity?.PageFooter,
            FullScreenImage = _entityMapper.Map<FullScreenImageResponse, FullScreenImage>(leftEntity?.FullScreenImage),
            Style = ProminentBannerStyle.Regular,
        };
    }

    public OfferPanelResponse Map(Panel rightEntity)
    {
        throw new NotImplementedException("We don't need to map to API responses.");
    }
}