/*
 * Copyright (c) 2022 Proton Technologies AG
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

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProtonVPN.Map.Views
{
    public partial class SecureCorePin
    {
        private const double TriangleWidth = 17;
        private readonly NumberFormatInfo _nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };

        public bool ShowTooltip
        {
            get => (bool)GetValue(ShowTooltipProperty);
            set => SetValue(ShowTooltipProperty, value);
        }

        public static readonly DependencyProperty ShowTooltipProperty = DependencyProperty.Register(
            "ShowTooltip",
            typeof(bool),
            typeof(SecureCorePin),
            new PropertyMetadata(false, ShowTooltipChanged));

        public bool Connected
        {
            get => (bool)GetValue(ConnectedProperty);
            set => SetValue(ConnectedProperty, value);
        }

        public static readonly DependencyProperty ConnectedProperty = DependencyProperty.Register(
            "Connected",
            typeof(bool),
            typeof(SecureCorePin),
            new PropertyMetadata(false, ConnectedChanged));

        public SecureCorePin()
        {
            InitializeComponent();
            Loaded += Pin_Loaded;
        }

        private void SetPinColors()
        {
            if (Connected)
            {
                PinPath.Fill = (Brush)Application.Current.MainWindow?.FindResource("PrimaryColor");
                PinPath.Stroke = (Brush)Application.Current.MainWindow?.FindResource("PrimaryColor");
            }
            else
            {
                PinPath.Fill = (Brush)Application.Current.MainWindow?.FindResource("PinFillColor");
                PinPath.Stroke = (Brush)Application.Current.MainWindow?.FindResource("PrimaryColor");
            }
        }

        private void Pin_Loaded(object sender, RoutedEventArgs e)
        {
            Tooltip.Visibility = Visibility.Collapsed;
            SetInitialStyle();
            CenterPin();
        }

        private void HideTooltip()
        {
            SetInitialStyle();
            Tooltip.Visibility = Visibility.Collapsed;
            CenterPin();
        }

        private void SetInitialStyle()
        {
            var center = TriangleWidth / 2;
            PinPath.Data = Geometry.Parse($"M0,0 L{TriangleWidth.ToString(_nfi)},0 L{center.ToString(_nfi)},14 L0,0Z");
            PinPath.Width = PinPath.Data.Bounds.Width;
            PinPath.Height = PinPath.Data.Bounds.Height;
            SetPinColors();
        }

        private void ShowTooltipAction()
        {
            Tooltip.Visibility = Visibility.Visible;

            var pathWidth = SecureCoreText.ActualWidth;
            var triangleCenter = pathWidth / 2;
            var triangleLeft = triangleCenter - TriangleWidth / 2;
            var triangleRight = triangleCenter + TriangleWidth / 2;

            PinPath.Data = Geometry.Parse($"M{pathWidth.ToString(_nfi)},30" +
                                          $"L{triangleRight.ToString(_nfi)},30" +
                                          $"L{triangleCenter.ToString(_nfi)},43" +
                                          $"L{triangleLeft.ToString(_nfi)},30" +
                                          "L0,30" +
                                          "A 15,15 180 1 1 0,0" +
                                          $"L{pathWidth.ToString(_nfi)},0" +
                                          $"A 15,15 180 1 1 {pathWidth.ToString(_nfi)},30 Z");

            PinPath.Width = PinPath.Data.Bounds.Width;
            PinPath.Height = PinPath.Data.Bounds.Height;

            CenterPin();
        }

        private void CenterPin()
        {
            UpdateLayout();
            Canvas.SetLeft(Tooltip, (PinCanvas.ActualWidth - Tooltip.ActualWidth) / 2);
            Canvas.SetBottom(Tooltip, 15);
        }

        private static void ConnectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pin = (SecureCorePin)d;
            pin.SetPinColors();
        }

        private static void ShowTooltipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pin = (SecureCorePin)d;
            if ((bool) e.NewValue)
            {
                pin.ShowTooltipAction();
            }
            else
            {
                pin.HideTooltip();
            }
        }
    }
}
