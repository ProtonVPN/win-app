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

using System.Windows.Markup;

namespace ProtonVPN.MarkupValidator
{
    internal class DictionaryValue
    {
        private readonly string _value;

        private const string XamlFormat = "<Span xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">{0}</Span>";

        public string Error { get; private set; }

        public DictionaryValue(string value)
        {
            _value = value;
        }

        public bool Valid()
        {
            try
            {
                XamlReader.Parse(string.Format(XamlFormat, _value.Replace("&", "")));
                return true;
            }
            catch (XamlParseException e)
            {
                Error = e.Message;
                return false;
            }
        }
    }
}
