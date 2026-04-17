/*
 * Copyright (c) 2026 Proton AG
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

using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.Enums;

namespace ProtonVPN.Client.Models.Features.LocationExclusion;

public abstract class ExcludableChildItemBase<TLocation> : ExcludableLocationItemBase<TLocation>, IExcludableChildItem
    where TLocation : ILocation
{
    public ExcludableCountryItem ParentCountry { get; }

    public override bool IsLocationExcluded => base.IsLocationExcluded || ParentCountry.IsExcluded;

    /// <summary>
    /// For child items, the absolute sort order is determined by the parent country and the relative sort order of the child item.
    /// This provides a hierarchical structure to the location so child location are listed right after their parent country then sorted by their relative sort order.
    /// </summary>
    public override string AbsoluteSortOrder => $"{ParentCountry.AbsoluteSortOrder} > {RelativeSortOrder}";

    public ExcludableChildItemBase(
        ILocalizationProvider localizer,
        ExcludableCountryItem parent,
        ExcludedLocationType type,
        TLocation location,
        bool isExcluded)
        : base(localizer, type, location, isExcluded)
    {
        ParentCountry = parent;
    }
}
