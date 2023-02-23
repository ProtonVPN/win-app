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

namespace ProtonVPN.Translations
{
    public class Translation
    {
        public static string Get(string key)
        {
            return TranslationSource.Instance["ProtonVPN.Translations.Properties.Resources." + key];
        }

        public static string GetPlural(string key, decimal number)
        {
            return TranslationSource.Instance.GetPlural(key, number);
        }

        public static string GetPluralFormat(string key, decimal number)
        {
            return string.Format(GetPlural(key, number), number);
        }

        public static string Format(string key, object args0)
        {
            return Safe(Get(key), (value) => string.Format(value, args0));
        }

        public static string Format(string key, object arg0, object arg1)
        {
            return Safe(Get(key), (value) => string.Format(value, arg0, arg1));
        }

        public static string Format(string key, params object[] args)
        {
            return Safe(Get(key), (value) => string.Format(value, args));
        }

        private static string Safe(string value, Func<string, string> func)
        {
            try
            {
                return func(value);
            }
            catch (FormatException)
            {
                return value;
            }
        }
    }
}
