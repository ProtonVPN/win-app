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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;

namespace ProtonVPN.Modals.Upsell
{
    public partial class CountryUpsellModalView
    {
        private const int FLAG_WIDTH = 48;

        public CountryUpsellModalView()
        {
            InitializeComponent();
            Loaded += OnViewLoaded;
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            string countryCode = (DataContext as CountryUpsellModalViewModel)?.CountryCode;
            if (string.IsNullOrEmpty(countryCode))
            {
                return;
            }

            try
            {
                Type type = Type.GetType($"ProtonVPN.Resource.Graphics.Flags.{countryCode}, ProtonVPN.Resource");
                if (type == null)
                {
                    return;
                }

                UserControl bottomFlag = CreateFlagControl(type);
                if (bottomFlag != null)
                {
                    bottomFlag.Effect = new BlurEffect
                    {
                        KernelType = KernelType.Gaussian,
                        Radius = 50
                    };
                }

                UserControl topFlag = CreateFlagControl(type);

                FlagContainer.Children.Clear();
                if (bottomFlag != null && topFlag != null)
                {
                    FlagContainer.Children.Add(bottomFlag);
                    FlagContainer.Children.Add(topFlag);
                }
            }
            catch
            {
            }
        }

        private UserControl CreateFlagControl(Type type)
        {
            if (Activator.CreateInstance(type) is UserControl control)
            {
                control.Width = FLAG_WIDTH;
                return control;
            }

            return null;
        }
    }
}