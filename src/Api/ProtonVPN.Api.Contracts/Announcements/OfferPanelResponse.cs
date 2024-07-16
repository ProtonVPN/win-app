﻿/*
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
using Newtonsoft.Json;

namespace ProtonVPN.Api.Contracts.Announcements;

public class OfferPanelResponse
{
    public string Incentive { get; set; }

    public string IncentivePrice { get; set; }

    public string Pill { get; set; }

    [JsonProperty(PropertyName = "PictureURL")]
    public string PictureUrl { get; set; }

    public string Title { get; set; }

    public IList<OfferPanelFeatureResponse> Features { get; set; }

    public string FeaturesFooter { get; set; }

    public OfferPanelButtonResponse Button { get; set; }

    public string PageFooter { get; set; }

    public FullScreenImageResponse FullScreenImage { get; set; }

    public bool ShowCountdown { get; set; }

    public bool IsDismissible { get; set; } = true;
}