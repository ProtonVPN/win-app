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

using System.Collections.Generic;
using ProtonVPN.Client.Localization.Contracts;

namespace ProtonVPN.Client.Localization.Languages;

/// <summary>
/// Defines all supported languages in the application.
/// The order of languages in this list determines the order they appear in the UI.
/// </summary>
public static class Languages
{
    public static IReadOnlyList<Language> All { get; } =
    [
        new("en-US", "English"),
        new("de-DE", "Deutsch - German"),
        new("fr-FR", "Français - French"),
        new("nl-NL", "Nederlands - Dutch"),
        new("es-ES", "Español (España) - Spanish (Spain)"),
        new("es-419", "Español (Latinoamérica) - Spanish (Latin America)"),
        new("it-IT", "Italiano - Italian"),
        new("pl-PL", "Polski - Polish"),
        new("pt-BR", "Português (Brasil) - Portuguese (Brazil)"),
        new("ru-RU", "Русский - Russian"),
        new("tr-TR", "Türkçe - Turkish"),
        new("ca-ES", "Català - Catalan"),
        new("cs-CZ", "Čeština - Czech"),
        new("da-DK", "Dansk - Danish"),
        new("fi-FI", "Suomi - Finnish"),
        new("hu-HU", "Magyar - Hungarian"),
        new("id-ID", "Bahasa (Indonesia) - Indonesian"),
        new("nb-NO", "Norsk (bokmål) - Norwegian (Bokmal)"),
        new("pt-PT", "Português (Portugal) - Portuguese"),
        new("ro-RO", "Română - Romanian"),
        new("sk-SK", "Slovenčina - Slovak"),
        new("sl-SI", "Slovenščina - Slovenian"),
        new("sv-SE", "Svenska - Swedish"),
        new("el-GR", "Ελληνικά - Greek"),
        new("be-BY", "Беларуская - Belarusian"),
        new("uk-UA", "Українська - Ukrainian"),
        new("ka-GE", "Ქართული - Georgian"),
        new("ko-KR", "한국어 - Korean"),
        new("ja-JP", "日本語 - Japanese"),
        new("zh-CN", "简体中文 - Chinese (Simplified)"),
        new("zh-TW", "繁體中文 - Chinese (Traditional)"),
        new("ar-SA", "عربي - Arabic", isRightToLeft: true),
        new("fa-IR", "فارسی - Persian", isRightToLeft: true),
    ];
}