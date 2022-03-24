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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProtonVPN.Views.Controls
{
    public partial class PieChart
    {
        private Image _pieChartImage;

        public static readonly DependencyProperty PercentageProperty =
           DependencyProperty.Register("Percentage", typeof(double), typeof(PieChart), new FrameworkPropertyMetadata(0.5, OnPiePropertyChanged, CoercePercentageCallback));

        public static readonly DependencyProperty SizeProperty =
          DependencyProperty.Register("Size", typeof(double), typeof(PieChart), new PropertyMetadata(100.0, OnPiePropertyChanged));

        public static readonly DependencyProperty InnerPieSliceFillProperty =
           DependencyProperty.Register("InnerPieSliceFill", typeof(Brush), typeof(PieChart), new PropertyMetadata(CreateBrush("#939496"), OnPiePropertyChanged));

        public static readonly DependencyProperty OuterPieSliceFillProperty =
           DependencyProperty.Register("OuterPieSliceFill", typeof(Brush), typeof(PieChart), new PropertyMetadata(CreateBrush("#D0D1D3"), OnPiePropertyChanged));

        public double Percentage
        {
            get => (double)GetValue(PercentageProperty);
            set => SetValue(PercentageProperty, value);
        }

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public Brush InnerPieSliceFill
        {
            get => (Brush)GetValue(InnerPieSliceFillProperty);
            set => SetValue(InnerPieSliceFillProperty, value);
        }

        public Brush OuterPieSliceFill
        {
            get => (Brush)GetValue(OuterPieSliceFillProperty);
            set => SetValue(OuterPieSliceFillProperty, value);
        }

        public PieChart()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            _pieChartImage = (Image)Template.FindName("PART_PieChart", this);

            CreatePieChart();
        }

        private static object CoercePercentageCallback(DependencyObject dep, object baseValue)
        {
            var value = (double)baseValue;

            if (value < 0.0)
            {
                value = 0.0;
            }
            else if (value > 1.0)
            {
                value = 1.0;
            }

            return value;
        }

        private static void OnPiePropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs ev)
        {
            var chart = (PieChart)dep;

            if (chart.IsInitialized)
            {
                chart.CreatePieChart();
            }
        }

        private void CreatePieChart()
        {
            if (_pieChartImage != null)
            {
                if (!double.IsNaN(Size) && !double.IsNaN(Percentage))
                {
                    _pieChartImage.Width = _pieChartImage.Height = Width = Height = Size;

                    var di = new DrawingImage();
                    _pieChartImage.Source = di;

                    var dg = new DrawingGroup();
                    di.Drawing = dg;
                    dg.Children.Add(CreateEllipseGeometry(BorderBrush, Size / 2 + BorderThickness.Left));

                    if (Percentage > 0.0 && Percentage < 1.0)
                    {
                        var angle = 360 * Percentage;
                        var radians = Math.PI / 180 * angle;
                        var endPointX = Math.Sin(radians) * Height / 2 + Height / 2;
                        var endPointY = Width / 2 - Math.Cos(radians) * Width / 2;
                        var endPoint = new Point(endPointX, endPointY);

                        dg.Children.Add(CreatePathGeometry(InnerPieSliceFill, new Point(Width / 2, 0), endPoint, Percentage > 0.5));
                        dg.Children.Add(CreatePathGeometry(OuterPieSliceFill, endPoint, new Point(Width / 2, 0), Percentage <= 0.5));
                    }
                    else
                    {
                        dg.Children.Add(CreateEllipseGeometry(Math.Abs(Percentage - 0.0) < 0.0001 ? OuterPieSliceFill : InnerPieSliceFill, Size / 2));
                    }
                }
                else
                {
                    _pieChartImage.Source = null;
                }
            }
        }

        private GeometryDrawing CreatePathGeometry(Brush brush, Point startPoint, Point arcPoint, bool isLargeArc)
        {
            var midPoint = new Point(Width / 2, Height / 2);

            var drawing = new GeometryDrawing { Brush = brush };
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure { StartPoint = midPoint };

            var ls1 = new LineSegment(startPoint, false);
            var arc = new ArcSegment
            {
                SweepDirection = SweepDirection.Clockwise,
                Size = new Size(Width / 2, Height / 2),
                Point = arcPoint,
                IsLargeArc = isLargeArc
            };
            var ls2 = new LineSegment(midPoint, false);

            drawing.Geometry = pathGeometry;
            pathGeometry.Figures.Add(pathFigure);

            pathFigure.Segments.Add(ls1);
            pathFigure.Segments.Add(arc);
            pathFigure.Segments.Add(ls2);

            return drawing;
        }

        private GeometryDrawing CreateEllipseGeometry(Brush brush, double size)
        {
            var midPoint = new Point(Width / 2, Height / 2);

            var drawing = new GeometryDrawing { Brush = brush };
            var ellipse = new EllipseGeometry(midPoint, size, size);

            drawing.Geometry = ellipse;

            return drawing;
        }

        private static SolidColorBrush CreateBrush(string brush)
        {
            var color = ColorConverter.ConvertFromString(brush);
            if (color != null)
            {
                return new SolidColorBrush((Color)color);
            }

            return null;
        }
    }
}
