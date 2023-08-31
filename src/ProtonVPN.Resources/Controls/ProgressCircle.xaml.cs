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
using System.Windows.Media;

namespace ProtonVPN.Resource.Controls
{
    public partial class ProgressCircle
    {
        public ProgressCircle()
        {
            InitializeComponent();
            Loaded += OnViewLoaded;
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(double), typeof(ProgressCircle),
                new PropertyMetadata(0.0, OnProgressChanged));

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            Path.Data = CreateGeometry();
        }

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressCircle control)
            {
                control.Path.Data = control.CreateGeometry();
            }
        }

        private Geometry CreateGeometry()
        {
            double angle = 3.6 * Progress;
            double radius = 45;

            if (angle >= 360)
            {
                return new EllipseGeometry
                {
                    RadiusX = radius,
                    RadiusY = radius,
                    Center = new Point(0, 0),
                };
            }

            return CreateArcGeometry(angle, radius);
        }

        private Geometry CreateArcGeometry(double angle, double radius)
        {
            return new PathGeometry
            {
                Figures = new PathFigureCollection
                {
                    new PathFigure
                    {
                        StartPoint = new Point(0, -radius),
                        Segments = new PathSegmentCollection
                        {
                            new ArcSegment(
                                new Point(
                                    radius * Math.Sin(angle * Math.PI / 180),
                                    radius * -Math.Cos(angle * Math.PI / 180)),
                                new Size(radius, radius), 0,
                                angle >= 180, SweepDirection.Clockwise, true)
                        }
                    }
                }
            };
        }
    }
}