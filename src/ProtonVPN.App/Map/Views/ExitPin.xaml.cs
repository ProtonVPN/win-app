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
using System.Windows.Input;
using System.Windows.Media;

namespace ProtonVPN.Map.Views
{
    public partial class ExitPin
    {
        private const double TriangleWidth = 17;
        private readonly NumberFormatInfo _nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };

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

        public ExitPin()
        {
            InitializeComponent();

            PinPath.IsMouseDirectlyOverChanged += PinPath_IsMouseDirectlyOverChanged;
            Loaded += Pin_Loaded;
            Tooltip.MouseEnter += OnTooltipMouseEnter;
            Tooltip.MouseLeave += OnTooltipMouseLeave;
            Tooltip.LayoutUpdated += Tooltip_LayoutUpdated;
        }

        private void Tooltip_LayoutUpdated(object sender, System.EventArgs e)
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
            if (Tooltip.IsMouseOver)
                return;

            ForceHideTooltip();
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
            var center = TriangleWidth / 2;
            PinPath.Data = Geometry.Parse($"M0,0 L{TriangleWidth.ToString(_nfi)},0 L{center.ToString(_nfi)},14 L0,0Z");
            PinPath.Width = PinPath.Data.Bounds.Width;
            PinPath.Height = PinPath.Data.Bounds.Height;
            SetPinColors();
        }

        private void ShowTooltipAction()
        {
            if (ShowTooltip)
                return;

            Tooltip.Visibility = Visibility.Visible;
            UpdateLayout();
            var pathWidth = Tooltip.ActualWidth;
            var height = Tooltip.ActualHeight - 45;
            if (height < 0) height = 0;
            var triangleCenter = pathWidth / 2;
            var triangleLeft = triangleCenter - TriangleWidth / 2;
            var triangleRight = triangleCenter + TriangleWidth / 2;

            PinPath.Data = Geometry.Parse($"M{pathWidth.ToString(_nfi)},30" +
                                          $"L{triangleRight.ToString(_nfi)},30" +
                                          $"L{triangleCenter.ToString(_nfi)},43" +
                                          $"L{triangleLeft.ToString(_nfi)},30" +
                                          "L0,30" +
                                          "A15,15,90,0,1,-15,15" +
                                          $"L-15,-{height.ToString(_nfi)}" +
                                          $"A15,15,90,0,1,0,-{(height + 15).ToString(_nfi)}" +
                                          $"L{pathWidth.ToString(_nfi)},-{(height + 15).ToString(_nfi)}" +
                                          $"A15,15,90,0,1,{(pathWidth + 15).ToString(_nfi)},-{height.ToString(_nfi)}" +
                                          $"L{(pathWidth + 15).ToString(_nfi)},15" +
                                          $"A15,15,90,0,1,{pathWidth.ToString(_nfi)},30Z");

            PinPath.Width = PinPath.Data.Bounds.Width;
            PinPath.Height = PinPath.Data.Bounds.Height;
            if (Highlighted)
            {
                PinPath.Stroke = Brushes.White;
            }
            else
            {
                PinPath.Stroke = (Brush)Application.Current.MainWindow?.FindResource("InactivePinStrokeColor");
            }

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
            if (Connected)
            {
                PinPath.Fill = (Brush) Application.Current.MainWindow?.FindResource("PrimaryColor");
            }
            else
            {
                PinPath.Fill = (Brush) Application.Current.MainWindow?.FindResource("PinFillColor");
            }

            if (Highlighted)
            {
                PinPath.Stroke = (Brush)Application.Current.MainWindow?.FindResource("PrimaryColor");
            }
            else
            {
                PinPath.Stroke = (Brush)Application.Current.MainWindow?.FindResource("InactivePinStrokeColor");
            }
        }

        private static void ConnectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pin = (ExitPin) d;
            pin.SetPinColors();
        }

        private static void HighlightedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pin = (ExitPin)d;
            pin.SetPinColors();
        }
    }
}
