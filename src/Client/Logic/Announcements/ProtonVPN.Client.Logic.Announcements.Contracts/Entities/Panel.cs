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

using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Files.Contracts.Images;

namespace ProtonVPN.Client.Logic.Announcements.Contracts.Entities;

public class Panel
{
    public string? Incentive { get; set; }
    public string? IncentivePrice { get; set; }
    public string? IncentiveSuffix { get; set; }
    public string? Description { get; set; }
    public CachedImage? Picture { get; set; }
    public string? Title { get; set; }
    public List<PanelFeature> Features { get; set; } = [];
    public string? FeaturesFooter { get; set; }
    public PanelButton? Button { get; set; }
    public string? PageFooter { get; set; }
    public FullScreenImage? FullScreenImage { get; set; }
    public ProminentBannerStyle Style { get; set; }
}
