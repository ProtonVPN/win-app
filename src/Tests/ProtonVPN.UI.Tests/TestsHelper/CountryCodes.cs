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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtonVPN.UI.Tests.TestsHelper;
public static class CountryCodes
{
    public static readonly Dictionary<string, string> NameToCode = new()
    {
        { "Afghanistan", "AF" },
        { "Albania", "AL" },
        { "Algeria", "DZ" },
        { "Angola", "AO" },
        { "Argentina", "AR" },
        { "Armenia", "AM" },
        { "Australia", "AU" },
        { "Austria", "AT" },
        { "Azerbaijan", "AZ" },
        { "Bahrain", "BH" },
        { "Bangladesh", "BD" },
        { "Belarus", "BY" },
        { "Belgium", "BE" },
        { "Bhutan", "BT" },
        { "Bosnia and Herzegovina", "BA" },
        { "Brazil", "BR" },
        { "Brunei Darussalam", "BN" },
        { "Bulgaria", "BG" },
        { "Cambodia", "KH" },
        { "Canada", "CA" },
        { "Chad", "TD" },
        { "Chile", "CL" },
        { "Colombia", "CO" },
        { "Comoros", "KM" },
        { "Costa Rica", "CR" },
        { "Côte d'Ivoire", "CI" },
        { "Croatia", "HR" },
        { "Cyprus", "CY" },
        { "Czechia", "CZ" },
        { "Denmark", "DK" },
        { "Ecuador", "EC" },
        { "Egypt", "EG" },
        { "El Salvador", "SV" },
        { "Eritrea", "ER" },
        { "Estonia", "EE" },
        { "Ethiopia", "ET" },
        { "Finland", "FI" },
        { "France", "FR" },
        { "Georgia", "GE" },
        { "Germany", "DE" },
        { "Ghana", "GH" },
        { "Greece", "GR" },
        { "Hong Kong", "HK" },
        { "Hungary", "HU" },
        { "Iceland", "IS" },
        { "India", "IN" },
        { "Indonesia", "ID" },
        { "Iraq", "IQ" },
        { "Ireland", "IE" },
        { "Israel", "IL" },
        { "Italy", "IT" },
        { "Japan", "JP" },
        { "Jordan", "JO" },
        { "Kazakhstan", "KZ" },
        { "Kenya", "KE" },
        { "Kuwait", "KW" },
        { "Laos", "LA" },
        { "Latvia", "LV" },
        { "Libya", "LY" },
        { "Lithuania", "LT" },
        { "Luxembourg", "LU" },
        { "Macedonia", "MK" },
        { "Malaysia", "MY" },
        { "Malta", "MT" },
        { "Mauritania", "MR" },
        { "Mauritius", "MU" },
        { "Mexico", "MX" },
        { "Moldova", "MD" },
        { "Mongolia", "MN" },
        { "Montenegro", "ME" },
        { "Morocco", "MA" },
        { "Mozambique", "MZ" },
        { "Myanmar", "MM" },
        { "Nepal", "NP" },
        { "Netherlands", "NL" },
        { "New Zealand", "NZ" },
        { "Nigeria", "NG" },
        { "Norway", "NO" },
        { "Oman", "OM" },
        { "Pakistan", "PK" },
        { "Panama", "PA" },
        { "Peru", "PE" },
        { "Philippines", "PH" },
        { "Poland", "PL" },
        { "Portugal", "PT" },
        { "Puerto Rico", "PR" },
        { "Qatar", "QA" },
        { "Romania", "RO" },
        { "Russia", "RU" },
        { "Rwanda", "RW" },
        { "Saudi Arabia", "SA" },
        { "Senegal", "SN" },
        { "Serbia", "RS" },
        { "Singapore", "SG" },
        { "Slovakia", "SK" },
        { "Slovenia", "SI" },
        { "Somalia", "SO" },
        { "South Africa", "ZA" },
        { "South Korea", "KR" },
        { "South Sudan", "SS" },
        { "Spain", "ES" },
        { "Sri Lanka", "LK" },
        { "Sudan", "SD" },
        { "Sweden", "SE" },
        { "Switzerland", "CH" },
        { "Syrian Arab Republic", "SY" },
        { "Taiwan", "TW" },
        { "Tajikistan", "TJ" },
        { "Tanzania", "TZ" },
        { "Thailand", "TH" },
        { "Togo", "TG" },
        { "Tunisia", "TN" },
        { "Türkiye", "TR" },
        { "Turkmenistan", "TM" },
        { "Ukraine", "UA" },
        { "United Arab Emirates", "AE" },
        { "United Kingdom", "UK" },
        { "United States", "US" },
        { "Uzbekistan", "UZ" },
        { "Venezuela", "VE" },
        { "Vietnam", "VN" },
        { "Yemen", "YE" }
    };

    public static string GetCode(string countryName)
    {
        return NameToCode.TryGetValue(countryName, out string code)
            ? code
            : throw new ArgumentException($"Unknown country: {countryName}");
    }
}
