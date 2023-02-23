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
using System.Windows.Media;

namespace ProtonVPN.Core.Wpf
{
    public class BorderGrid : Grid
    {
        public double BorderThickness
        {
            get => (double)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
            "BorderThickness",
            typeof(double),
            typeof(BorderGrid),
            new PropertyMetadata(0.0));

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
            "BorderBrush",
            typeof(Brush),
            typeof(BorderGrid),
            new PropertyMetadata(Brushes.Red));

        protected override void OnRender(DrawingContext dc)
        {
            double leftOffset = 0;
            double topOffset = 0;
            var pen = new Pen(BorderBrush, BorderThickness);
            pen.Freeze();

            foreach (RowDefinition row in RowDefinitions)
            {
                dc.DrawLine(pen, new Point(0, topOffset), new Point(ActualWidth, topOffset));
                topOffset += row.ActualHeight;
            }

            // draw last line at the bottom
            dc.DrawLine(pen, new Point(0, topOffset), new Point(ActualWidth, topOffset));

            foreach (ColumnDefinition column in ColumnDefinitions)
            {
               dc.DrawLine(pen, new Point(leftOffset, 0), new Point(leftOffset, ActualHeight));
               leftOffset += column.ActualWidth;
            }

            //draw last line on the right
            dc.DrawLine(pen, new Point(leftOffset, 0), new Point(leftOffset, ActualHeight));

            base.OnRender(dc);
        }
    }
}
