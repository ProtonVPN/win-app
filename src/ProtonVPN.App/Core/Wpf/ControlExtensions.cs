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

namespace ProtonVPN.Core.Wpf
{
    public static class ControlExtensions
    {
        public static bool InterceptWith(this FrameworkElement element, FrameworkElement secondElement)
        {
            var point1 = element.PointToScreen(new Point(0, 0));
            var point2 = secondElement.PointToScreen(new Point(0, 0));

            var dpiX = SystemParams.GetDpiX();
            var dpiY = SystemParams.GetDpiY();

            point1.X = point1.X * 96.0 / dpiX;
            point1.Y = point1.Y * 96.0 / dpiY;

            point2.X = point2.X * 96.0 / dpiX;
            point2.Y = point2.Y * 96.0 / dpiY;

            var rect = new Rect
            {
                Location = new Point(point1.X, point1.Y),
                Size = new Size(element.ActualWidth, element.ActualHeight)
            };

            var rect2 = new Rect
            {
                Location = new Point(point2.X, point2.Y),
                Size = new Size(secondElement.ActualWidth, secondElement.ActualHeight)
            };

            var a = rect.IntersectsWith(rect2);


            return a;
        }
    }
}
