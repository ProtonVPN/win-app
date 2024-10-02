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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Legacy.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Legacy.UI.Upsell.Carousel.Features.Base;
using ProtonVPN.Client.Legacy.UI.Upsell.Carousel.Models;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.UI.Upsell.Carousel.Features;

public class WorldwideCoverageUpsellFeatureViewModel : UpsellFeatureViewModelBase,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IServerCountCache _serverCountCache;

    public override string Title
        => Localizer.GetFormat("Upsell_Carousel_WorldwideCoverage",
                Localizer.GetPluralFormat("Upsell_Carousel_WorldwideCoverage_Servers", _serverCountCache.GetServerCount()),
                Localizer.GetPluralFormat("Upsell_Carousel_WorldwideCoverage_Countries", _serverCountCache.GetCountryCount()));

    public override ImageSource SmallIllustrationSource { get; } = ResourceHelper.GetIllustration("WorldwideCoverageUpsellSmallIllustrationSource");

    public override ImageSource LargeIllustrationSource { get; } = ResourceHelper.GetIllustration("WorldwideCoverageUpsellLargeIllustrationSource");

    public WorldwideCoverageUpsellFeatureViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IServerCountCache serverCountCache)
        : base(UpsellFeature.WorldwideCoverage,
               localizationProvider,
               logger,
               issueReporter)
    {
        _serverCountCache = serverCountCache;
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(Title)));
    }
}