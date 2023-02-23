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

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace ProtonVPN.Core.Wpf.Behavior
{
    public static class TextStyleBehavior
    {
        public static string GetFormattedText(DependencyObject obj)
        {
            return (string)obj.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(DependencyObject obj, string value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached("FormattedText",
            typeof(string),
            typeof(TextStyleBehavior),
            new UIPropertyMetadata("", FormattedTextChanged));

        private static void FormattedTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var value = e.NewValue as string;
            if (sender is TextBlock textBlock)
            {
                textBlock.Inlines.Clear();
                textBlock.Inlines.Add(SafeToInline(value));
            }
            else if (sender is Span span)
            {
                span.Inlines.Clear();
                span.Inlines.Add(SafeToInline(value));
            }
        }

        private static Inline SafeToInline(string value)
        {
            try
            {
                return ToInline(value);
            }
            catch (XamlParseException)
            {
                return new Run(value);
            }
        }

        private static Inline ToInline(string value)
        {
            const string xamlFormat =
                "<Span xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">{0}</Span>";

            var xaml = string.Format(xamlFormat, value);
            using (var stringReader = new StringReader(xaml))
            using (var xmlReader = XmlReader.Create(stringReader))
            {
                var result = (Inline)XamlReader.Load(xmlReader);
                return result;
            }
        }
    }
}
