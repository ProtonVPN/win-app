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

using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers;

public class ModalTriggerDimensionMapper : DimensionMapperBase, IModalTriggerDimensionMapper
{
    private const string COUNTRIES_BANNER = "countries_banner";
    private const string COUNTRY_SELECTION = "country_selection";
    private const string ERROR_DIALOG = "error_dialog";
    private const string HOME = "home";
    private const string HOME_BANNER = "home_banner";
    private const string HOME_CAROUSEL = "home_carousel";
    private const string NETWORK_RESTRICTION = "network_restriction";
    private const string MAP = "map";
    private const string ONBOARDING = "onboarding";
    private const string PROFILES = "profiles";
    private const string PROMO_OFFER_BANNER = "promo_offer_banner";
    private const string PROMO_OFFER_POPUP = "promo_offer_popup";
    private const string SEARCH = "search";
    private const string SEARCH_SELECTION = "search_selection";
    private const string SETTINGS = "settings";
    private const string TRAY = "tray";

    public string Map(ModalTrigger? modalTrigger)
    {
        return modalTrigger switch
        {
            ModalTrigger.CountriesBanner => COUNTRIES_BANNER,
            ModalTrigger.CountrySelection => COUNTRY_SELECTION,
            ModalTrigger.ErrorDialog => ERROR_DIALOG,
            ModalTrigger.Home => HOME,
            ModalTrigger.HomeBanner => HOME_BANNER,
            ModalTrigger.Carousel => HOME_CAROUSEL,
            ModalTrigger.NetworkRestriction => NETWORK_RESTRICTION,
            ModalTrigger.Map => MAP,
            ModalTrigger.Onboarding => ONBOARDING,
            ModalTrigger.Profiles => PROFILES,
            ModalTrigger.PromoOfferBanner => PROMO_OFFER_BANNER,
            ModalTrigger.PromoOfferPopup => PROMO_OFFER_POPUP,
            ModalTrigger.Search => SEARCH,
            ModalTrigger.SearchSelection => SEARCH_SELECTION,
            ModalTrigger.Settings => SETTINGS,
            ModalTrigger.Tray => TRAY,
            _ => NOT_AVAILABLE
        };
    }
}
