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

using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Navigation;

namespace ProtonVPN.Client.UI.Dialogs.Upsell.Bases;

public abstract class UpsellFeaturePageViewModelBase : PageViewModelBase<IUpsellCarouselViewNavigator>, IUpsellFeaturePage
{
    public int SortIndex => (int)Feature;

    public UpsellFeatureType Feature { get; }

    public abstract ImageSource SmallIllustrationSource { get; }

    public abstract ImageSource LargeIllustrationSource { get; }

    protected UpsellFeaturePageViewModelBase(
        IUpsellCarouselViewNavigator parentViewNavigator,
        IViewModelHelper viewModelHelper,
        UpsellFeatureType feature)
        : base(parentViewNavigator, viewModelHelper)
    {
        Feature = feature;
    }
}