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

using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;

namespace ProtonVPN.Client.Models.Connections.Countries;

public class GenericCountryLocationItem : LocationItemBase
{
    public CountriesConnectionType ConnectionType { get; }
    public ConnectionIntentKind IntentKind { get; }

    public bool ExcludeMyCountry { get; }

    public FlagType FlagType => IntentKind switch
    {
        ConnectionIntentKind.Random => FlagType.Random,
        _ => FlagType.Fastest,
    };

    public override string Header => Localizer.GetCountryName(string.Empty, IntentKind, ExcludeMyCountry);

    public override bool IsCounted => false;

    public override object FirstSortProperty => string.Empty;

    public override object SecondSortProperty => string.Empty;

    public override ILocationIntent LocationIntent { get; }

    public override IFeatureIntent? FeatureIntent { get; }

    public override ConnectionGroupType GroupType => ConnectionType switch
    {
        CountriesConnectionType.SecureCore => ConnectionGroupType.SecureCoreCountries,
        CountriesConnectionType.P2P => ConnectionGroupType.P2PCountries,
        CountriesConnectionType.Tor => ConnectionGroupType.TorCountries,
        _ => ConnectionGroupType.Countries,
    };

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_Country_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_Country_UnderMaintenance")
                : null;

    protected override string AutomationName => IntentKind switch
    {
        ConnectionIntentKind.Fastest when ExcludeMyCountry => "FastestExcludingMyCountry",
        ConnectionIntentKind.Random when ExcludeMyCountry => "RandomExcludingMyCountry",
        ConnectionIntentKind.Fastest => "Fastest",
        ConnectionIntentKind.Random => "Random",
        _ => throw new NotImplementedException($"Intent kind '{IntentKind}' is not supported."),
    };

    public bool IsSecureCore => FeatureIntent is SecureCoreFeatureIntent;

    public GenericCountryLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        CountriesConnectionType connectionType,
        ConnectionIntentKind intentKind,
        bool excludeMyCountry)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator)
    {
        ConnectionType = connectionType;
        IntentKind = intentKind;
        ExcludeMyCountry = excludeMyCountry;

        LocationIntent = new CountryLocationIntent(IntentKind, ExcludeMyCountry);
        FeatureIntent = ConnectionType switch
        {
            CountriesConnectionType.SecureCore => new SecureCoreFeatureIntent(),
            CountriesConnectionType.P2P => new P2PFeatureIntent(),
            CountriesConnectionType.Tor => new TorFeatureIntent(),
            _ => null
        };
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && currentConnectionDetails.OriginalConnectionIntent.Location.IsSameAs(LocationIntent)
            && ((currentConnectionDetails.OriginalConnectionIntent.Feature == null && FeatureIntent == null) 
               || (currentConnectionDetails.OriginalConnectionIntent.Feature?.IsSameAs(FeatureIntent) ?? false));
    }
}