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

namespace ProtonVPN.Client.Localization.Contracts;

public interface ILocalizationProvider
{
    string Get(string resourceKey);

    string GetFormat(string resourceKey, object arg0);

    string GetFormat(string resourceKey, object arg0, object arg1);

    string GetFormat(string resourceKey, object arg0, object arg1, object arg2);

    string GetFormat(string resourceKey, params object[] args);

    string GetPlural(string resourceKey, long number);

    string GetPluralFormat(string resourceKey, long number);

    void ForceCurrentLanguageForPluralProvider();
}