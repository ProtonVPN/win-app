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
using System.Windows.Controls;

namespace ProtonVPN.Sidebar.CountryFeatures
{
    public partial class CountryFeature
    {
        public CountryFeature()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string),
                typeof(CountryFeature),
                new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PlanProperty =
            DependencyProperty.Register("Plan", typeof(string),
                typeof(CountryFeature),
                new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string),
                typeof(CountryFeature),
                new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(UserControl),
                typeof(CountryFeature),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.Register("Action", typeof(string),
                typeof(CountryFeature),
                new FrameworkPropertyMetadata(null));

        public UserControl Icon
        {
            get => (UserControl)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Plan
        {
            get => (string)GetValue(PlanProperty);
            set => SetValue(PlanProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public string Action
        {
            get => (string) GetValue(ActionProperty);
            set => SetValue(ActionProperty, value);
        }
    }
}