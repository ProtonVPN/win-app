/*
 * Copyright (c) 2021 Proton Technologies AG
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

namespace ProtonVPN.Core.Api.Contracts
{
    public class AnnouncementsResponse : BaseResponse
    {
        [JsonProperty(PropertyName = "Notifications")]
        public IList<Announcement> Announcements { get; set; }
    }

    public class Announcement
    {
        [JsonProperty(PropertyName = "NotificationID")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "StartTime")]
        public long StartTimestamp { get; set; }

        [JsonProperty(PropertyName = "EndTime")]
        public long EndTimestamp { get; set; }

        public Offer Offer { get; set; }
    }

    public class Offer
    {
        [JsonProperty(PropertyName = "URL")]
        public string Url { get; set; }

        public string Icon { get; set; }

        public string Label { get; set; }

        public OfferPanel Panel { get; set; }
    }

    public class OfferPanel
    {
        public string Incentive { get; set; }

        public string IncentivePrice { get; set; }

        public string Pill { get; set; }

        [JsonProperty(PropertyName = "PictureURL")]
        public string PictureUrl { get; set; }

        public string Title { get; set; }

        public IList<OfferPanelFeature> Features { get; set; }

        public string FeaturesFooter { get; set; }

        public OfferPanelButton Button { get; set; }

        public string PageFooter { get; set; }
    }

    public class OfferPanelFeature
    {
        [JsonProperty(PropertyName = "IconURL")]
        public string IconUrl { get; set; }

        public string Text { get; set; }
    }

    public class OfferPanelButton
    {
        [JsonProperty(PropertyName = "URL")]
        public string Url { get; set; }

        public string Text { get; set; }
    }
}
