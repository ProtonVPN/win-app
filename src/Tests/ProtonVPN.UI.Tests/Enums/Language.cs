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
    [Display(Name = "Arabic - عربي", ShortName = "ar-SA")] Arabic,
    [Display(Name = "Belarusian - Беларуская", ShortName = "be-BY")] Belarusian,
    [Display(Name = "Catalan - Català", ShortName = "ca-ES")] Catalan,
    [Display(Name = "Chinese (Simplified) - 简体中文", ShortName = "zh-CN")] Chinese_Simplified,
    [Display(Name = "Chinese (Traditional) - 繁體中文", ShortName = "zh-TW")] Chinese_Traditional,
    [Display(Name = "Czech - Čeština", ShortName = "cs-CZ")] Czech,
    [Display(Name = "Danish - Dansk", ShortName = "da-DK")] Danish,
    [Display(Name = "Dutch - Nederlands", ShortName = "nl-NL")] Dutch,
    [Display(Name = "English", ShortName = "en-US")] English,
    [Display(Name = "Filipino (Latin) - Filipino", ShortName = "fil-PH")] Filipino,
    [Display(Name = "Filipino (Baybayin) - ᜉᜒᜎᜒᜉᜒᜈᜓ", ShortName = "fil-Tglg")] Filipino_Baybayin,
    [Display(Name = "Finnish - Suomi", ShortName = "fi-FI")] Finnish,
    [Display(Name = "French - Français", ShortName = "fr-FR")] French,
    [Display(Name = "Georgian - Ქართული", ShortName = "ka-GE")] Georgian,
    [Display(Name = "German - Deutsch", ShortName = "de-DE")] German,
    [Display(Name = "Greek - Ελληνικά", ShortName = "el-GR")] Greek,
    [Display(Name = "Hungarian - Magyar", ShortName = "hu-HU")] Hungarian,
    [Display(Name = "Indonesian - Bahasa Indonesia", ShortName = "id-ID")] Indonesian,
    [Display(Name = "Italian - Italiano", ShortName = "it-IT")] Italian,
    [Display(Name = "Japanese - 日本語", ShortName = "ja-JP")] Japanese,
    [Display(Name = "Korean - 한국어", ShortName = "ko-KR")] Korean,
    [Display(Name = "Norwegian (Bokmal) - Norsk (bokmål)", ShortName = "nb-NO")] Norwegian,
    [Display(Name = "Persian - فارسی", ShortName = "fa-IR")] Persian,
    [Display(Name = "Polish - Polski", ShortName = "pl-PL")] Polish,
    [Display(Name = "Portuguese (Brazil) - Português (Brasil)", ShortName = "pt-BR")] Portuguese_Brazil,
    [Display(Name = "Portuguese (Portugal) - Português (Portugal)", ShortName = "pt-PT")] Portuguese_Portugal,
    [Display(Name = "Romanian - Română", ShortName = "ro-RO")] Romanian,
    [Display(Name = "Russian - Русский", ShortName = "ru-RU")] Russian,
    [Display(Name = "Slovak - Slovenčina", ShortName = "sk-SK")] Slovak,
    [Display(Name = "Slovenian - slovenščina", ShortName = "sl-SI")] Slovenian,
    [Display(Name = "Spanish (Spain) - Español (España)", ShortName = "es-ES")] Spanish_Spain,
    [Display(Name = "Spanish (Latin America) - Español (Latinoamérica)", ShortName = "es-419")] Spanish_LatinAmerica,
    [Display(Name = "Swedish - Svenska", ShortName = "sv-SE")] Swedish,
    [Display(Name = "Thai - ไทย", ShortName = "th-TH")] Thai,
    [Display(Name = "Turkish - Türkçe", ShortName = "tr-TR")] Turkish,
    [Display(Name = "Ukrainian - Українська", ShortName = "uk-UA")] Ukrainian,
    [Display(Name = "Vietnamese - Tiếng Việt", ShortName = "vi-VN")] Vietnamese,
}