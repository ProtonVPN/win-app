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
using System.Windows.Input;
using System.Windows.Media;
using ProtonVPN.Resource.Colors;
using ProtonVPN.Resources.Colors;

namespace ProtonVPN.Map.Views
{
    public partial class ExitPin
    {
        private const double TRIANGLE_HALF_WIDTH = 1.7383;
        private const double ANGLE_HALF_WIDTH = 0.9354;
        private const double JOINT_DISTANCE = 5.5;
        private const double CORNER_RADIUS = 8;
        private const double EXTRA_TOOLTIP_WIDTH = 50;
        private const double EXTRA_TOOLTIP_HEIGHT = 8;
        private const double TRIANGLE_POINT_HEIGHT = 9;
        private const double TRIANGLE_ANGLE_HEIGHT = 10.2845;

        private readonly NumberFormatInfo _nfi = new() { NumberDecimalSeparator = "." };

        public bool Connected
        {
            get => (bool) GetValue(ConnectedProperty);
            set => SetValue(ConnectedProperty, value);
        }

        public static readonly DependencyProperty ConnectedProperty = DependencyProperty.Register(
            "Connected",
            typeof(bool),
            typeof(ExitPin),
            new PropertyMetadata(false, ConnectedChanged));

        public bool Highlighted
        {
            get => (bool)GetValue(HighlightedProperty);
            set => SetValue(HighlightedProperty, value);
        }

        public static readonly DependencyProperty HighlightedProperty = DependencyProperty.Register(
            "Highlighted",
            typeof(bool),
            typeof(ExitPin),
            new PropertyMetadata(false, HighlightedChanged));

        public bool ShowTooltip
        {
            get => (bool)GetValue(ShowTooltipProperty);
            set => SetValue(ShowTooltipProperty, value);
        }

        public static readonly DependencyProperty ShowTooltipProperty = DependencyProperty.Register(
            "ShowTooltip",
            typeof(bool),
            typeof(ExitPin),
            new PropertyMetadata(false));

        private static readonly IColorPalette _colorPalette = ColorPaletteFactory.Create();
        private readonly Lazy<SolidColorBrush> _fillColorBrush = new(() => _colorPalette.GetSolidColorBrushByResourceName("BackgroundNormBrushColor"));
        private readonly Lazy<SolidColorBrush> _interactionColorBrush = new(() => _colorPalette.GetSolidColorBrushByResourceName("InteractionNormBrushColor"));

        public ExitPin()
        {
            InitializeComponent();

            PinPath.IsMouseDirectlyOverChanged += PinPath_IsMouseDirectlyOverChanged;
            Loaded += Pin_Loaded;
            Tooltip.MouseEnter += OnTooltipMouseEnter;
            Tooltip.MouseLeave += OnTooltipMouseLeave;
            Tooltip.LayoutUpdated += Tooltip_LayoutUpdated;
        }

        private void Tooltip_LayoutUpdated(object sender, EventArgs e)
        {
            CenterPin();
        }

        private void OnTooltipMouseLeave(object sender, MouseEventArgs e)
        {
            CenterPin();
            if (!PinPath.IsMouseDirectlyOver)
            {
                HideTooltip();
            }
        }

        private void OnTooltipMouseEnter(object sender, MouseEventArgs e)
        {
            CenterPin();
        }

        private void Pin_Loaded(object sender, RoutedEventArgs e)
        {
            Tooltip.Visibility = Visibility.Collapsed;
            SetInitialStyle();
            CenterPin();
        }

        private void PinPath_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (PinPath.IsMouseDirectlyOver)
            {
                ShowTooltipAction();
            }
            else
            {
                HideTooltip();
            }
        }

        private void HideTooltip()
        {
            if (!Tooltip.IsMouseOver)
            {
                ForceHideTooltip();
            }
        }

        private void ForceHideTooltip()
        {
            SetInitialStyle();
            Tooltip.Visibility = Visibility.Collapsed;
            CenterPin();
            ShowTooltip = false;
        }

        private void SetInitialStyle()
        {
            PinPath.Data = Geometry.Parse("M 3.4758 1.3586 H 15.8761 C 17.4862 1.3586 18.4677 3.1296 17.6144 4.4949 L 11.4143 14.4152 C 10.6114 15.6997 8.7406 15.6997 7.9377 14.4152 L 1.7376 4.4949 C 0.8842 3.1296 1.8659 1.3586 3.4758 1.3586 Z");
            PinPath.Width = PinPath.Data.Bounds.Width;
            PinPath.Height = PinPath.Data.Bounds.Height;
            PinPath.StrokeThickness = 2;
            SetPinColors();
        }

        private void ShowTooltipAction()
        {
            if (ShowTooltip)
            {
                return;
            }

            Tooltip.Visibility = Visibility.Visible;
            UpdateLayout();
            
            double pathWidth = Tooltip.ActualWidth + EXTRA_TOOLTIP_WIDTH;
            double height = Tooltip.ActualHeight + EXTRA_TOOLTIP_HEIGHT;
            double center = pathWidth / 2;
            double triangleRight = center + TRIANGLE_HALF_WIDTH;
            double triangleAngleRight = center + ANGLE_HALF_WIDTH;
            double triangleAngleLeft = center - ANGLE_HALF_WIDTH;
            double triangleLeft = center - TRIANGLE_HALF_WIDTH;
            double jointRight = triangleRight + JOINT_DISTANCE;
            double jointLeft = triangleLeft - JOINT_DISTANCE;
            double rightBorderX = pathWidth + CORNER_RADIUS;
            double lowerCornersY = height - CORNER_RADIUS;
            double triangleY = height + TRIANGLE_POINT_HEIGHT;
            double triangleAngleY = height + TRIANGLE_ANGLE_HEIGHT;
            
            string data = $"M{pathWidth.ToString(_nfi)},{height.ToString(_nfi)}" +
                          $"L{jointRight.ToString(_nfi)},{height.ToString(_nfi)}" +
                          $"L{triangleRight.ToString(_nfi)},{triangleY.ToString(_nfi)}" +
                          $"C{triangleAngleRight.ToString(_nfi)},{triangleAngleY.ToString(_nfi)} {triangleAngleLeft.ToString(_nfi)},{triangleAngleY.ToString(_nfi)} {triangleLeft.ToString(_nfi)},{triangleY.ToString(_nfi)}" +
                          $"L{jointLeft.ToString(_nfi)},{height.ToString(_nfi)}" +
                          $"L0,{height.ToString(_nfi)}" +
                          $"C0,{height.ToString(_nfi)} -{CORNER_RADIUS.ToString(_nfi)},{height.ToString(_nfi)} -{CORNER_RADIUS.ToString(_nfi)},{lowerCornersY.ToString(_nfi)}" +
                          $"L-{CORNER_RADIUS.ToString(_nfi)},{CORNER_RADIUS.ToString(_nfi)}" +
                          $"C-{CORNER_RADIUS.ToString(_nfi)},{CORNER_RADIUS.ToString(_nfi)} -{CORNER_RADIUS.ToString(_nfi)},0 0,0" +
                          $"L{pathWidth.ToString(_nfi)},0" +
                          $"C{pathWidth.ToString(_nfi)},0 {rightBorderX.ToString(_nfi)},0 {rightBorderX.ToString(_nfi)},{CORNER_RADIUS.ToString(_nfi)}" +
                          $"L{rightBorderX.ToString(_nfi)},{lowerCornersY.ToString(_nfi)}" +
                          $"C{rightBorderX.ToString(_nfi)},{lowerCornersY.ToString(_nfi)} {rightBorderX.ToString(_nfi)},{height.ToString(_nfi)} {pathWidth.ToString(_nfi)},{height.ToString(_nfi)}" +
                          $"Z";
            PinPath.Data = Geometry.Parse(data);

            PinPath.Width = PinPath.Data.Bounds.Width;
            PinPath.Height = PinPath.Data.Bounds.Height;
            
            PinPath.Fill = _interactionColorBrush.Value;

            CenterPin();
            ShowTooltip = true;
        }

        private void CenterPin()
        {
            UpdateLayout();
            Canvas.SetLeft(PinPath, (PinCanvas.ActualWidth - PinPath.ActualWidth) / 2);
            Canvas.SetLeft(Tooltip, (PinCanvas.ActualWidth - Tooltip.ActualWidth) / 2);
            Canvas.SetBottom(PinPath, 0);
            Canvas.SetBottom(Tooltip, 15);
        }

        private void SetPinColors()
        {
            PinPath.Fill = Connected ? _interactionColorBrush.Value : _fillColorBrush.Value;
            PinPath.Stroke = _interactionColorBrush.Value;
        }

        private static void ConnectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExitPin pin = (ExitPin) d;
            pin.SetPinColors();
        }

        private static void HighlightedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExitPin pin = (ExitPin)d;
            pin.SetPinColors();
        }
    }
}
