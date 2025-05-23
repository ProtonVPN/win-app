﻿/*
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
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Announcements.EntityMapping;

public class ProminentBannerMapper : IMapper<ProminentBannerResponse, Panel>
{
    private readonly IEntityMapper _entityMapper;

    public ProminentBannerMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public Panel Map(ProminentBannerResponse leftEntity)
    {
        if (leftEntity is null)
        {
            return null;
        }

        return new()
        {
            Title = leftEntity?.Title,
            Description = leftEntity?.Description,
            Button = _entityMapper.Map<OfferPanelButtonResponse, PanelButton>(leftEntity?.ActionButton),
            Style = Enum.TryParse(leftEntity.Style, out ProminentBannerStyle style) ? style : ProminentBannerStyle.Regular,
        };
    }

    public ProminentBannerResponse Map(Panel rightEntity)
    {
        throw new NotImplementedException("We don't need to map to API responses.");
    }
}