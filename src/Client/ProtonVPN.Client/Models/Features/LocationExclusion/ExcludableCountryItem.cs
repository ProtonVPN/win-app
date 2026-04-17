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

using System.Collections.Specialized;
using System.ComponentModel;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;

namespace ProtonVPN.Client.Models.Features.LocationExclusion;

public partial class ExcludableCountryItem : ExcludableLocationItemBase<Country>
{
    public override bool IsLocationExcluded => base.IsLocationExcluded || (Children.Count > 0 && Children.All(c => c.IsExcluded));

    public bool HasMultipleAvailableChildren => Children.Count(c => !c.IsExcluded) > 1;

    public NotifyObservableCollection<IExcludableLocation> Children { get; } = [];

    public string CountryCode { get; }

    public ExcludableCountryItem(
        ILocalizationProvider localizer,
        Country country,
        bool isExcluded)
        : base(localizer, ExcludedLocationType.Country, country, isExcluded)
    {
        CountryCode = country.Code;
        DisplayName = localizer.GetCountryName(country.Code);

        Children.CollectionChanged += OnChildrenCollectionChanged;
        Children.ItemPropertyChanged += OnChildrenItemPropertyChanged;
    }

    public void AddChild(IExcludableChildItem state)
    {
        Children.Add(state);
    }

    public override ExcludedLocation ToExcludedLocation()
    {
        return new ExcludedLocation(Location.Code);
    }

    private void OnChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasMultipleAvailableChildren));
    }

    private void OnChildrenItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasMultipleAvailableChildren));
    }
}
