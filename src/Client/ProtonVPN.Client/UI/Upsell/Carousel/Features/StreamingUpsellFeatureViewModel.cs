/*
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

using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.UI.Upsell.Carousel.Features.Base;
using ProtonVPN.Client.UI.Upsell.Carousel.Models;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Upsell.Carousel.Features;

public class StreamingUpsellFeatureViewModel : UpsellFeatureViewModelBase
{
    public override string? Title => Localizer.Get("Upsell_Carousel_Streaming");

    public override ImageSource SmallIllustrationSource { get; } = ResourceHelper.GetIllustration("StreamingUpsellSmallIllustrationSource");

    public override ImageSource LargeIllustrationSource { get; } = ResourceHelper.GetIllustration("StreamingUpsellLargeIllustrationSource");

    public StreamingUpsellFeatureViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(UpsellFeature.Streaming,
               localizationProvider,
               logger,
               issueReporter)
    { }
}
