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
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.UI.Dialogs.Upsell.Bases;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Dialogs.Upsell.Features;

public class ProfilesUpsellFeaturePageViewModel : UpsellFeaturePageViewModelBase
{
    public override string Title => Localizer.Get("Upsell_Carousel_Profiles");

    public override ImageSource SmallIllustrationSource { get; } = ResourceHelper.GetIllustration("ProfilesUpsellSmallIllustrationSource");

    public override ImageSource LargeIllustrationSource { get; } = ResourceHelper.GetIllustration("ProfilesUpsellLargeIllustrationSource");

    public ProfilesUpsellFeaturePageViewModel(
        IUpsellCarouselViewNavigator upsellCarouselViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(upsellCarouselViewNavigator,
               localizer,
               logger,
               issueReporter,
               UpsellFeatureType.Profiles)
    { }
}