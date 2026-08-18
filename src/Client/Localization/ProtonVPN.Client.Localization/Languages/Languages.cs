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
        new("ar-SA", "Arabic - عربي", isRightToLeft: true),
        new("be-BY", "Belarusian - Беларуская"),
        new("ca-ES", "Catalan - Català"),
        new("zh-CN", "Chinese (Simplified) - 简体中文"),
        new("zh-TW", "Chinese (Traditional) - 繁體中文"),
        new("cs-CZ", "Czech - Čeština"),
        new("da-DK", "Danish - Dansk"),
        new("nl-NL", "Dutch - Nederlands"),
        new("en-US", "English"),
        new("fil-PH", "Filipino (Latin) - Filipino"),
        new("fil-Tglg", "Filipino (Baybayin) - ᜉᜒᜎᜒᜉᜒᜈᜓ"),
        new("fi-FI", "Finnish - Suomi"),
        new("fr-FR", "French - Français"),
        new("ka-GE", "Georgian - Ქართული"),
        new("de-DE", "German - Deutsch"),
        new("el-GR", "Greek - Ελληνικά"),
        new("hu-HU", "Hungarian - Magyar"),
        new("id-ID", "Indonesian - Bahasa Indonesia"),
        new("it-IT", "Italian - Italiano"),
        new("ja-JP", "Japanese - 日本語"),
        new("ko-KR", "Korean - 한국어"),
        new("nb-NO", "Norwegian (Bokmal) - Norsk (bokmål)"),
        new("fa-IR", "Persian - فارسی", isRightToLeft: true),
        new("pl-PL", "Polish - Polski"),
        new("pt-BR", "Portuguese (Brazil) - Português (Brasil)"),
        new("pt-PT", "Portuguese (Portugal) - Português (Portugal)"),
        new("ro-RO", "Romanian - Română"),
        new("ru-RU", "Russian - Русский"),
        new("sk-SK", "Slovak - Slovenčina"),
        new("sl-SI", "Slovenian - slovenščina"),
        new("es-ES", "Spanish (Spain) - Español (España)"),
        new("es-419", "Spanish (Latin America) - Español (Latinoamérica)"),
        new("sv-SE", "Swedish - Svenska"),
        new("th-TH", "Thai - ไทย"),
        new("tr-TR", "Turkish - Türkçe"),
        new("uk-UA", "Ukrainian - Українська"),
        new("vi-VN", "Vietnamese - Tiếng Việt"),
    ];
}