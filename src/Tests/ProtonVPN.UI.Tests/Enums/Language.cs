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

using System.ComponentModel.DataAnnotations;

namespace ProtonVPN.UI.Tests.Enums;

public enum Language
{
    [Display(Name = "English", ShortName = "en-US")] English,
    [Display(Name = "Deutsch - German", ShortName = "de-DE")] German,
    [Display(Name = "Français - French", ShortName = "fr-FR")] French,
    [Display(Name = "Español (España) - Spanish (Spain)", ShortName = "es-ES")] Spanish_Spain,
    [Display(Name = "Español (Latinoamérica) - Spanish (Latin America)", ShortName = "es-419")] Spanish_LatinAmerica,
    [Display(Name = "Italiano - Italian", ShortName = "it-IT")] Italian,
    [Display(Name = "Nederlands - Dutch", ShortName = "nl-NL")] Dutch,
    [Display(Name = "Polski - Polish", ShortName = "pl-PL")] Polish,
    [Display(Name = "Português (Brasil) - Portuguese (Brazil)", ShortName = "pt-BR")] Portuguese_Brazil,
    [Display(Name = "Русский - Russian", ShortName = "ru-RU")] Russian,
    [Display(Name = "한국어 - Korean", ShortName = "ko-KR")] Korean,
    [Display(Name = "日本語 - Japanese", ShortName = "ja-JP")] Japanese,
    [Display(Name = "Català - Catalan", ShortName = "ca-ES")] Catalan,
    [Display(Name = "Čeština - Czech", ShortName = "cs-CZ")] Czech,
    [Display(Name = "Dansk - Danish", ShortName = "da-DK")] Danish,
    [Display(Name = "Suomi - Finnish", ShortName = "fi-FI")] Finnish,
    [Display(Name = "Bahasa (Indonesia) - Indonesian", ShortName = "id-ID")] Indonesian,
    [Display(Name = "Português (Portugal) - Portuguese", ShortName = "pt-PT")] Portuguese_Portugal,
    [Display(Name = "Română - Romanian", ShortName = "ro-RO")] Romanian,
    [Display(Name = "Svenska - Swedish", ShortName = "sv-SE")] Swedish,
    [Display(Name = "Türkçe - Turkish", ShortName = "tr-TR")] Turkish,
    [Display(Name = "Tiếng Việt - Vietnamese", ShortName = "vi-VN")] Vietnamese,
    [Display(Name = "简体中文 - Chinese (Simplified)", ShortName = "zh-CN")] Chinese_Simplified,
    [Display(Name = "繁體中文 - Chinese (Traditional)", ShortName = "zh-TW")] Chinese_Traditional,
    [Display(Name = "ไทย - Thai", ShortName = "th-TH")] Thai,
    [Display(Name = "Filipino - Filipino", ShortName = "fil-PH")] Filipino,
    [Display(Name = "Magyar - Hungarian", ShortName = "hu-HU")] Hungarian,
    [Display(Name = "Norsk (bokmål) - Norwegian (Bokmal)", ShortName = "nb-NO")] Norwegian,
    [Display(Name = "Slovenčina - Slovak", ShortName = "sk-SK")] Slovak,
    [Display(Name = "Slovenščina - Slovenian", ShortName = "sl-SI")] Slovenian,
    [Display(Name = "Ελληνικά - Greek", ShortName = "el-GR")] Greek,
    [Display(Name = "Беларуская - Belarusian", ShortName = "be-BY")] Belarusian,
    [Display(Name = "Українська - Ukrainian", ShortName = "uk-UA")] Ukrainian,
    [Display(Name = "Ქართული - Georgian", ShortName = "ka-GE")] Georgian,
    [Display(Name = "عربي - Arabic", ShortName = "ar-SA")] Arabic,
    [Display(Name = "فارسی - Persian", ShortName = "fa-IR")] Persian,
}