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

using ProtonVPN.Core.Wpf;
using ProtonVPN.Map.ViewModels.MapLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ProtonVPN.Map.Views
{
    public partial class Map
    {
        private Point? _lastPoint;
        public const double InitialWidth = 850;
        public const double InitialHeight = 400;
        private const int MaxZoomLevel = 7;
        private const double Step = 1000;
        private const double MaxMapWidth = MaxZoomLevel * Step;

        private int _zoomLevel;

        public bool Connected
        {
            get => (bool)GetValue(ConnectedProperty);
            set => SetValue(ConnectedProperty, value);
        }

        public static readonly DependencyProperty ConnectedProperty = DependencyProperty.Register(
            "Connected",
            typeof(bool),
            typeof(Map),
            new PropertyMetadata(false, ConnectedChanged));

        public bool SecureCore
        {
            get => (bool)GetValue(SecureCoreProperty);
            set => SetValue(SecureCoreProperty, value);
        }

        public static readonly DependencyProperty SecureCoreProperty = DependencyProperty.Register(
            "SecureCore",
            typeof(bool),
            typeof(Map),
            new PropertyMetadata(false, SecureCoreChanged));

        public Map()
        {
            InitializeComponent();
            RegisterListeners();
            Loaded += OnMapLoaded;
        }

        private void RegisterListeners()
        {
            ScrollviewerWrapper.PreviewMouseLeftButtonDown += OnDragStart;
            ScrollviewerWrapper.PreviewMouseLeftButtonUp += OnDragStop;
            ScrollviewerWrapper.PreviewMouseMove += Drag;
            ScrollViewer.ScrollChanged += OnScrollChanged;
            ScrollViewer.SizeChanged += OnMapSizeChanged;
            ScrollViewer.MouseLeave += ScrollViewer_MouseLeave;
            ScrollviewerWrapper.PreviewMouseWheel += OnMouseWheel;
            ConnectionStatus.MouseLeftButtonUp += ResetMapButton_Click;
            ConnectionStatus.IsVisibleChanged += OnConnectionStatusVisibilityChanged;
            MapCanvas.RequestBringIntoView += OnRequestBringIntoView;
            Lines.TargetUpdated += LinesUpdated;
        }

        private void LinesUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            SetConnectionLinePosition();
        }

        private void OnScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            SetConnectionLinePosition();
        }

        private void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
        {
            e.Mode = ManipulationModes.Scale | ManipulationModes.Translate;
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            var newWidth = NewWidth(e.DeltaManipulation.Expansion.X);
            if (e.DeltaManipulation.Translation.Length > 0 && e.DeltaManipulation.Expansion.Length <= 0)
            {
                Scroll(
                    ScrollViewer.HorizontalOffset - e.DeltaManipulation.Translation.X,
                    ScrollViewer.VerticalOffset - e.DeltaManipulation.Translation.Y);
                return;
            }

            if (newWidth - InitialWidth >= MaxMapWidth)
            {
                e.Handled = true;
                SetZoomBars();
                return;
            }

            if (newWidth <= ScrollViewer.ViewportWidth)
                newWidth = ScrollViewer.ViewportWidth;

            var viewBoxX = ScrollViewer.ContentHorizontalOffset + e.ManipulationOrigin.X;
            var viewBoxY = ScrollViewer.ContentVerticalOffset + e.ManipulationOrigin.Y;

            MapViewBox.Width = newWidth;
            var offsetX = new MapOffset(e.ManipulationOrigin.X, viewBoxX, Scale(e.DeltaManipulation.Expansion.X)).Value();
            var offsetY = new MapOffset(e.ManipulationOrigin.Y, viewBoxY, Scale(e.DeltaManipulation.Expansion.X)).Value();

            Scroll(offsetX, offsetY);
            SetZoomBars();
        }

        private double NewWidth(double delta)
        {
            var width = MapViewBox.DesiredSize.Width + delta * 20;
            return width > InitialWidth ? width : InitialWidth;
        }

        private void OnConnectionStatusVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetHomeLineVisibility();
        }

        private void ScrollViewer_MouseLeave(object sender, MouseEventArgs e)
        {
            OnDragStop(null, null);
            HideMapCoordinates();
        }

        private void OnMapLoaded(object sender, RoutedEventArgs e)
        {
            ResetMap();
            SetConnectionLinePosition();
        }

        private void SetSpeedGraphWidth()
        {
            var speedGraphWidth = MainGrid.ActualWidth - Sidebar.ActualWidth - 80;
            if (speedGraphWidth < 0)
                speedGraphWidth = 0;
            SpeedGraph.Width = speedGraphWidth;
        }

        private void ToggleConnectionStatusVisibility()
        {
            ConnectionStatus.Visibility = ScrollviewerWrapper.ActualWidth < ConnectionStatus.ActualWidth ?
                Visibility.Hidden : Visibility.Visible;
        }

        private void ToggleLogoVisibility()
        {
            Logo.Visibility = Logo.InterceptWith(ConnectionStatus) ? Visibility.Hidden : Visibility.Visible;
        }

        private void ToggleZoombarVisibility()
        {
            if (Logo.Visibility.Equals(Visibility.Hidden))
            {
                HorizontalZoomPanel.Visibility = Visibility.Hidden;
                VerticalZoomPanel.Visibility = VerticalZoomPanel.InterceptWith(ConnectionStatus) ? Visibility.Hidden : Visibility.Visible;
            }
            else
            {
                HorizontalZoomPanel.Visibility = Visibility.Visible;
                VerticalZoomPanel.Visibility = Visibility.Hidden;
            }
        }

        private void ResetMapButton_Click(object sender, RoutedEventArgs e)
        {
            ResetMap();
        }

        private void ResetMap()
        {
            MapViewBox.Width = ScrollViewer.ActualWidth;
            var horizontalOffset = Math.Abs((ScrollViewer.ActualWidth - InitialWidth) / 2) - 50;
            if (horizontalOffset < 0)
                horizontalOffset = 0;
            Scroll(horizontalOffset, 0);
            SetZoomBars();
        }

        private void OnDragStart(object sender, MouseButtonEventArgs e)
        {
            _lastPoint = e.GetPosition(ScrollViewer);
        }

        private void OnDragStop(object sender, MouseButtonEventArgs e)
        {
            _lastPoint = null;
        }

        private double WidthRatio()
        {
            return MapViewBox.ActualWidth / InitialWidth;
        }

        private double HeightRatio()
        {
            return MapViewBox.ActualHeight / InitialHeight;
        }

        private void Drag(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(ScrollViewer);
            DisplayMapCoordinates(position);

            if (!_lastPoint.HasValue)
                return;

            var x = position.X - _lastPoint.Value.X;
            var y = position.Y - _lastPoint.Value.Y;

            if (x.Equals(0) && y.Equals(0))
                return;

            _lastPoint = position;

            Scroll(ScrollViewer.HorizontalOffset - x, ScrollViewer.VerticalOffset - y);
            ScrollViewer.UpdateLayout();
        }

        private void SetConnectionLinePosition()
        {
            var wRatio = WidthRatio();
            var hRatio = HeightRatio();
            var relativeLocation = HomeIcon.TranslatePoint(new Point(0, 0), ScrollviewerWrapper);
            var relativeLocation2 = HomeIcon.TranslatePoint(new Point(0, 0), MapCanvas);

            foreach (var line in Lines.Items)
            {
                var mapLine = line as MapLine;
                if (mapLine is HomeLine)
                {
                    if (relativeLocation2.Y < 0)
                    {
                        mapLine.Y1 = relativeLocation2.Y / hRatio + HomeIcon.ActualHeight / 2 / hRatio;
                    }
                    else
                    {
                        mapLine.Y1 = relativeLocation.Y / hRatio
                            + ScrollViewer.VerticalOffset / hRatio
                            + HomeIcon.ActualHeight / 2 / hRatio;
                    }

                    mapLine.StrokeThickness = 1 / wRatio;
                    mapLine.X1 = relativeLocation.X / hRatio
                        + ScrollViewer.HorizontalOffset / wRatio
                        + HomeIcon.ActualWidth / wRatio / 2;
                }
            }
        }

        private void ZoomIn(object sender, MouseButtonEventArgs e)
        {
            Zoom(1);
        }

        private void ZoomOut(object sender, MouseButtonEventArgs e)
        {
            Zoom(-1);
        }

        private void Zoom(int delta)
        {
            var width = NewWidth(delta);

            if (delta > 0 && _zoomLevel >= MaxZoomLevel)
                return;

            MapViewBox.Width = width;
            var viewBoxX = ScrollViewer.ContentHorizontalOffset + ScrollViewer.ViewportWidth / 2;
            var viewBoxY = ScrollViewer.ContentVerticalOffset + ScrollViewer.ViewportHeight / 2;
            var offsetX = new MapOffset(ScrollViewer.ViewportWidth / 2, viewBoxX, Scale(delta)).Value();
            var offsetY = new MapOffset(ScrollViewer.ViewportHeight / 2, viewBoxY, Scale(delta)).Value();

            Scroll(offsetX, offsetY);
            SetZoomBars();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var width = NewWidth(e.Delta);

            if (e.Delta > 0 && _zoomLevel >= MaxZoomLevel)
            {
                e.Handled = true;
                return;
            }

            var scale = Scale(e.Delta);
            var offsetX = new MapOffset(e.GetPosition(ScrollViewer).X, e.GetPosition(MapViewBox).X, scale).Value();
            var offsetY = new MapOffset(e.GetPosition(ScrollViewer).Y, e.GetPosition(MapViewBox).Y, scale).Value();

            MapViewBox.Width = width;
            Scroll(offsetX, offsetY);
            SetZoomBars();

            if (!e.Handled)
                e.Handled = true;
        }

        private double NewWidth(int delta)
        {
            var width = MapViewBox.DesiredSize.Width;
            if (delta > 0)
                return width + Step;

            var diff = width - Step;
            width = diff > InitialWidth ? diff : InitialWidth;

            if (width - ScrollViewer.ViewportWidth < 0.01)
                width = ScrollViewer.ViewportWidth;

            return width;
        }

        private double Scale(int delta)
        {
            var factor = Math.Abs(100 - NewWidth(delta) * 100 / MapViewBox.DesiredSize.Width) / 100;
            if (delta < 0)
                factor *= -1;

            return factor;
        }

        private double Scale(double delta)
        {
            var scale = Math.Abs(100 - NewWidth(delta) * 100 / MapViewBox.DesiredSize.Width) / 100;
            if (delta < 1)
                scale *= -1;

            return scale;
        }

        private void Scroll(double x, double y)
        {
            ScrollViewer.ScrollToHorizontalOffset(x);
            ScrollViewer.ScrollToVerticalOffset(y);
        }

        private void ResizeMap(SizeChangedEventArgs e)
        {
            if (MapViewBox.Width <= e.NewSize.Width)
                MapViewBox.Width = e.NewSize.Width;
            else
            {
                var width = (e.PreviousSize.Width - e.NewSize.Width) / 2;
                var height = (e.PreviousSize.Height - e.NewSize.Height) / 2;
                Scroll(
                    ScrollViewer.ContentHorizontalOffset + width,
                    ScrollViewer.ContentVerticalOffset + height);
            }
        }

        private void OnMapSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ToggleConnectionStatusVisibility();
            ToggleLogoVisibility();
            ToggleZoombarVisibility();
            SetSpeedGraphWidth();
            SetHomeLineVisibility();
            ResizeMap(e);
        }

        private void SetHomeLineVisibility()
        {
            foreach (var line in Lines.Items)
            {
                if (line is HomeLine homeLine && homeLine.Connected)
                    homeLine.Visible = ConnectionStatus.IsVisible;
            }
        }

        private void SetZoomBars()
        {
            var number = GetZoomLevel();
            if (number < 0 || number > MaxZoomLevel)
                return;

            int i;
            var horizontalBars = new List<bool>();
            var verticalBars = new List<bool>();

            for (i = 0; i < MaxZoomLevel + 1; i++)
            {
                horizontalBars.Add(i == number);
                verticalBars.Add(MaxZoomLevel - i == number);
            }

            VerticalZoomBars.ItemsSource = verticalBars;
            HorizontalZoomBars.ItemsSource = horizontalBars;
            _zoomLevel = number;
        }

        private int GetZoomLevel()
        {
            if (MapViewBox.Width <= InitialWidth)
            {
                return 0;
            }

            return (int)Math.Round((MapViewBox.Width - InitialWidth) / MaxMapWidth * MaxZoomLevel);
        }

        private static void ConnectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var map = (Map)d;
            if (map.Connected)
            {
                map.SetHomeLineVisibility();
                map.SetConnectionLinePosition();
            }
        }

        private static void SecureCoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var map = (Map)d;
            if (map.SecureCore)
                map.SetConnectionLinePosition();
        }

        [Conditional("DEBUG")]
        private void DisplayMapCoordinates(Point position)
        {
            if (!Coordinates.IsVisible)
                Coordinates.Visibility = Visibility.Visible;

            var x = (ScrollViewer.ContentHorizontalOffset + position.X) / WidthRatio() - 16.7;
            var y = (ScrollViewer.ContentVerticalOffset + position.Y) / HeightRatio() - 48.4;

            XCoordinate.Content = x.ToString("n1");
            YCoordinate.Content = y.ToString("n1");
        }

        [Conditional("DEBUG")]
        private void HideMapCoordinates()
        {
            Coordinates.Visibility = Visibility.Hidden;
        }
    }
}
