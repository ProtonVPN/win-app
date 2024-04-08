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

using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;

namespace ProtonVPN.Client.UI.Countries;

public static class CountryFeatureExtensions
{
    public static IFeatureIntent? GetFeatureIntent(this CountryFeature feature, string countryCode = "")
    {
        switch (feature)
        {
            case CountryFeature.SecureCore:
                return new SecureCoreFeatureIntent(countryCode);
            case CountryFeature.P2P:
                return new P2PFeatureIntent();
            case CountryFeature.Tor:
                return new TorFeatureIntent();
            default:
                return null;
        }
    }

    public static ModalSources GetUpsellModalSources(this CountryFeature feature)
    {
        return feature switch
        {
            CountryFeature.P2P => ModalSources.P2P,
            CountryFeature.SecureCore => ModalSources.SecureCore,
            CountryFeature.Tor => ModalSources.Countries, // Tor is not a modal source option yet, redirect to countries
            _ => ModalSources.Countries
        };
    }
}