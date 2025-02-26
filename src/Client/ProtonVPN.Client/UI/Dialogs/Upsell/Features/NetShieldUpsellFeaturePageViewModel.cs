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
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.UI.Dialogs.Upsell.Bases;

namespace ProtonVPN.Client.UI.Dialogs.Upsell.Features;

public class NetShieldUpsellFeaturePageViewModel : UpsellFeaturePageViewModelBase
{
    public override string Title => Localizer.Get("Upsell_Carousel_NetShield");

    public override ImageSource SmallIllustrationSource { get; } = ResourceHelper.GetIllustration("NetShieldUpsellSmallIllustrationSource");

    public override ImageSource LargeIllustrationSource { get; } = ResourceHelper.GetIllustration("NetShieldUpsellLargeIllustrationSource");

    public NetShieldUpsellFeaturePageViewModel(
        IUpsellCarouselViewNavigator upsellCarouselViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(upsellCarouselViewNavigator,
               viewModelHelper,
               UpsellFeatureType.NetShield)
    { }
}