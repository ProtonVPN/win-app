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

using System.Windows;

namespace ProtonVPN.Views.Controls
{
    public partial class AltImage
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(object),
            typeof(AltImage),
            new PropertyMetadata(null));

        public static readonly DependencyProperty AltProperty = DependencyProperty.Register(
            nameof(Alt),
            typeof(string),
            typeof(AltImage),
            new PropertyMetadata(string.Empty));

        public AltImage()
        {
            InitializeComponent();
        }

        public object Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public string Alt
        {
            get => (string)GetValue(AltProperty);
            set => SetValue(AltProperty, value);
        }

        private void Logo_OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var element = (FrameworkElement)e.Source;
            element.Visibility = Visibility.Collapsed;
        }
    }
}