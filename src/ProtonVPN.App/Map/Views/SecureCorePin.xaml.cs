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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ProtonVPN.Resource.Colors;
using ProtonVPN.Resources.Colors;

namespace ProtonVPN.Map.Views
{
    public partial class SecureCorePin
    {
        private const double TriangleWidth = 17;
        private readonly NumberFormatInfo _nfi = new() { NumberDecimalSeparator = "." };

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

        private static readonly IColorPalette _colorPalette = ColorPaletteFactory.Create();
        private readonly Lazy<SolidColorBrush> _fillColorBrush = new(() => _colorPalette.GetSolidColorBrushByResourceName("BackgroundNormBrushColor"));
        private readonly Lazy<SolidColorBrush> _interactionColorBrush = new(() => _colorPalette.GetSolidColorBrushByResourceName("InteractionNormBrushColor"));

        public SecureCorePin()
        {
            InitializeComponent();
            Loaded += Pin_Loaded;
        }

        private void SetPinColors()
        {
            PinPath.Fill = Connected ? _interactionColorBrush.Value : _fillColorBrush.Value;
            PinPath.Stroke = _interactionColorBrush.Value;
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
            double center = TriangleWidth / 2;
            PinPath.Data = Geometry.Parse($"M0,0 L{TriangleWidth.ToString(_nfi)},0 L{center.ToString(_nfi)},14 L0,0Z");
            PinPath.Width = PinPath.Data.Bounds.Width;
            PinPath.Height = PinPath.Data.Bounds.Height;
            PinPath.StrokeThickness = 2;
            SetPinColors();
        }

        private void ShowTooltipAction()
        {
            Tooltip.Visibility = Visibility.Visible;

            double pathWidth = SecureCoreText.ActualWidth;
            double triangleCenter = pathWidth / 2;
            double triangleLeft = triangleCenter - TriangleWidth / 2;
            double triangleRight = triangleCenter + TriangleWidth / 2;

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
            SecureCorePin pin = (SecureCorePin)d;
            pin.SetPinColors();
        }

        private static void ShowTooltipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SecureCorePin pin = (SecureCorePin)d;
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
