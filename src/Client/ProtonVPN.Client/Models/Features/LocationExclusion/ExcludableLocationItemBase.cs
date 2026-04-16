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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Core.Bases.Models;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;

namespace ProtonVPN.Client.Models.Features.LocationExclusion;

public abstract partial class ExcludableLocationItemBase<TLocation> : ModelBase, IExcludableLocation
    where TLocation : ILocation
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLocationExcluded))]
    private bool _isExcluded;

    public virtual bool IsLocationExcluded => IsExcluded;

    public ExcludedLocationType Type { get; }

    public TLocation Location { get; }

    public string DisplayName { get; protected set; } = string.Empty;

    public string RemoveTooltip { get; }

    public virtual string AbsoluteSortOrder => $"{(int)Type} > {RelativeSortOrder}";

    public virtual string RelativeSortOrder => DisplayName;

    public ExcludableLocationItemBase(
        ILocalizationProvider localizer,
        ExcludedLocationType type,
        TLocation location,
        bool isExcluded)
        : base(localizer)
    {
        Type = type;
        Location = location;
        IsExcluded = isExcluded;

        RemoveTooltip = localizer.Get("Common_Actions_Remove");
    }

    public abstract ExcludedLocation ToExcludedLocation();

    [RelayCommand]
    private void ExcludeLocation()
    {
        if (IsExcluded)
        {
            return;
        }

        IsExcluded = true;
    }

    [RelayCommand]
    private void IncludeLocation()
    {
        if (!IsExcluded)
        {
            return;
        }

        IsExcluded = false;
    }
}
