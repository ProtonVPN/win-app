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

using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.UI.Dialogs.Upsell.Bases;

namespace ProtonVPN.Client.UI.Dialogs.Upsell.Features;

public class WorldwideCoverageUpsellFeaturePageViewModel : UpsellFeaturePageViewModelBase,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IServersLoader _serversLoader;

    public override string Title
        => Localizer.GetFormat("Upsell_Carousel_WorldwideCoverage",
                Localizer.GetPluralFormat("Upsell_Carousel_WorldwideCoverage_Servers", _serversLoader.GetServerCount()),
                Localizer.GetPluralFormat("Upsell_Carousel_WorldwideCoverage_Countries", _serversLoader.GetCountryCount()));

    public override ImageSource SmallIllustrationSource { get; } = ResourceHelper.GetIllustration("WorldwideCoverageUpsellSmallIllustrationSource");

    public override ImageSource LargeIllustrationSource { get; } = ResourceHelper.GetIllustration("WorldwideCoverageUpsellLargeIllustrationSource");

    public WorldwideCoverageUpsellFeaturePageViewModel(
        IUpsellCarouselViewNavigator upsellCarouselViewNavigator,
        IServersLoader serversLoader,
        IViewModelHelper viewModelHelper)
        : base(upsellCarouselViewNavigator,
               viewModelHelper,
               UpsellFeatureType.WorldwideCoverage)
    {
        _serversLoader = serversLoader;
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(Title)));
    }
}