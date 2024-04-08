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
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.UI.Upsell.Carousel.Features.Base;
using ProtonVPN.Client.UI.Upsell.Carousel.Models;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Upsell.Carousel.Features;

public class WorldwideCoverageUpsellFeatureViewModel : UpsellFeatureViewModelBase,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private const int SERVERS_ROUND_DOWN_THRESHOLD = 100;
    private const int SERVERS_FALLBACK_COUNT = 4000;
    private const int COUNTRIES_FALLBACK_COUNT = 85;

    private readonly IServersLoader _serversLoader;

    public override string? Title
        => Localizer.GetFormat("Upsell_Carousel_WorldwideCoverage",
                Localizer.GetPluralFormat("Upsell_Carousel_WorldwideCoverage_Servers", GetServersCount()),
                Localizer.GetPluralFormat("Upsell_Carousel_WorldwideCoverage_Countries", GetCountriesCount()));

    public override ImageSource SmallIllustrationSource { get; } = ResourceHelper.GetIllustration("WorldwideCoverageUpsellSmallIllustrationSource");

    public override ImageSource LargeIllustrationSource { get; } = ResourceHelper.GetIllustration("WorldwideCoverageUpsellLargeIllustrationSource");

    public WorldwideCoverageUpsellFeatureViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IServersLoader serversLoader)
        : base(UpsellFeature.WorldwideCoverage,
               localizationProvider,
               logger,
               issueReporter)
    {
        _serversLoader = serversLoader;
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(Title)));
    }

    private int GetServersCount()
    {
        int serversCount = _serversLoader.GetServers().Count();
        int roundedServersCount = serversCount - (serversCount % SERVERS_ROUND_DOWN_THRESHOLD);

        return Math.Max(SERVERS_FALLBACK_COUNT, roundedServersCount);
    }

    private int GetCountriesCount()
    {
        int countriesCount = _serversLoader.GetCountryCodes().Count();

        return Math.Max(COUNTRIES_FALLBACK_COUNT, countriesCount);
    }
}