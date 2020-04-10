/*
 * Copyright (c) 2020 Proton Technologies AG
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
    public partial class Pin
    {
        private const double TriangleWidth = 17;
        private bool _tooltipVisible;
        private readonly NumberFormatInfo _nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };

        public bool Highlighted
        {
            get => (bool)GetValue(HighlightedProperty);
            set => SetValue(HighlightedProperty, value);
        }

        public static readonly DependencyProperty HighlightedProperty = DependencyProperty.Register(
            "Highlighted",
            typeof(bool),
            typeof(Pin),
            new PropertyMetadata(false, HighlightedChanged));

        public bool Connected
        {
            get => (bool)GetValue(ConnectedProperty);
            set => SetValue(ConnectedProperty, value);
        }

        public static readonly DependencyProperty ConnectedProperty = DependencyProperty.Register(
            "Connected",
            typeof(bool),
            typeof(Pin),
            new PropertyMetadata(false, ConnectedChanged));

        public Pin()
        {
            InitializeComponent();

            PinPath.IsMouseDirectlyOverChanged += PinPath_IsMouseDirectlyOverChanged;
            CountryButton.MouseEnter += CountryButton_MouseEnter;
            CountryButton.MouseLeave += CountryButton_MouseLeave;
            Loaded += Pin_Loaded;
            CountryButton.LayoutUpdated += CountryButton_LayoutUpdated;
        }

        private void CountryButton_LayoutUpdated(object sender, System.EventArgs e)
        {
            CenterPin();
        }

        private void CountryButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CountryButton.FontWeight = FontWeights.Bold;
            CenterPin();
        }

        private void CountryButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CountryButton.FontWeight = FontWeights.Normal;
            CenterPin();
            if (!PinPath.IsMouseDirectlyOver)
            {
                HideTooltip();
            }
        }

        private void Pin_Loaded(object sender, RoutedEventArgs e)
        {
            CountryButton.Visibility = Visibility.Collapsed;
            SetInitialStyle();
            CenterPin();
        }

        private void PinPath_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (PinPath.IsMouseDirectlyOver)
            {
                ShowTooltip();
            }
            else
            {
                HideTooltip();
            }
        }

        private void HideTooltip()
        {
            if (!_tooltipVisible)
                return;

            if (CountryButton.IsMouseOver)
            {
                return;
            }

            SetInitialStyle();
            CenterPin();
            CountryButton.Visibility = Visibility.Collapsed;
            _tooltipVisible = false;
        }

        private void SetInitialStyle()
        {
            var center = TriangleWidth / 2;
            PinPath.Data = Geometry.Parse($"M0,0 L{TriangleWidth.ToString(_nfi)},0 L{center.ToString(_nfi)},14 L0,0Z");
            PinPath.Width = PinPath.Data.Bounds.Width;
            PinPath.Height = PinPath.Data.Bounds.Height;
            CountryButton.FontSize = 12;
            SetPinColors();
        }

        private void ShowTooltip()
        {
            if (_tooltipVisible)
                return;

            CountryButton.Visibility = Visibility.Visible;
            UpdateLayout();

            var pathWidth = CountryButton.ActualWidth + 60;
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
            if (Highlighted)
            {
                PinPath.Stroke = Brushes.White;
            }
            else
            {
                PinPath.Stroke = (Brush)Application.Current.MainWindow?.FindResource("InactivePinStrokeColor");
            }

            SetHoverStyle();
            CenterPin();
            _tooltipVisible = true;
        }

        private void CenterPin()
        {
            UpdateLayout();
            Canvas.SetLeft(PinPath, (PinCanvas.ActualWidth - PinPath.ActualWidth) / 2);
            Canvas.SetLeft(CountryButton, (PinCanvas.ActualWidth - CountryButton.ActualWidth) / 2);
            Canvas.SetBottom(CountryButton, 18);
            Canvas.SetBottom(PinPath, 0);
        }

        private static void HighlightedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pin = (Pin) d;
            pin.SetPinColors();
        }

        private void SetPinColors()
        {
            if (Connected)
            {
                PinPath.Fill = (Brush)Application.Current.MainWindow?.FindResource("PrimaryColor");
            }
            else
            {
                PinPath.Fill = (Brush)Application.Current.MainWindow?.FindResource("PinFillColor");
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

        private void SetHoverStyle()
        {
            if (Highlighted && !Connected)
            {
                PinPath.Stroke = Brushes.White;
                CountryButton.FontWeight = FontWeights.Bold;
            }
        }

        private static void ConnectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pin = (Pin)d;
            pin.SetPinColors();
        }
    }
}
