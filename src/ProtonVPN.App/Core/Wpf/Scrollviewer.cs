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

namespace ProtonVPN.Core.Wpf
{
    public class Scrollviewer : ScrollViewer
    {
        public new double HorizontalOffset
        {
            get => (double)GetValue(HorizontalOffsetProperty);
            set => SetValue(HorizontalOffsetProperty, value);
        }

        public new double VerticalOffset
        {
            get => (double)GetValue(VerticalOffsetProperty);
            set => SetValue(VerticalOffsetProperty, value);
        }

        public new double ViewportWidth
        {
            get => (double)GetValue(ViewportWidthProperty);
            set => SetValue(ViewportWidthProperty, value);
        }

        public new double ViewportHeight
        {
            get => (double)GetValue(ViewportHeightProperty);
            set => SetValue(ViewportHeightProperty, value);
        }

        public new static DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "HorizontalOffset",
                typeof(double),
                typeof(Scrollviewer),
                new FrameworkPropertyMetadata(
                    default(double),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    HorizontalOffsetChanged
                    )
                );

        public new static DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "VerticalOffset",
                typeof(double),
                typeof(Scrollviewer),
                new FrameworkPropertyMetadata(
                    default(double),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    VerticalOffsetChanged
                    )
                );

        public new static DependencyProperty ViewportWidthProperty =
            DependencyProperty.RegisterAttached(
                "ViewportWidth",
                typeof(double),
                typeof(Scrollviewer),
                new FrameworkPropertyMetadata(
                    default(double)
                    )
                );

        public new static DependencyProperty ViewportHeightProperty =
            DependencyProperty.RegisterAttached(
                "ViewportHeight",
                typeof(double),
                typeof(Scrollviewer),
                new FrameworkPropertyMetadata(
                    default(double)
                    )
                );

        public static void SetViewportWidth(UIElement element, double value)
        {
            element.SetValue(ViewportWidthProperty, value);
        }

        public static void SetViewportHeight(UIElement element, double value)
        {
            element.SetValue(ViewportHeightProperty, value);
        }

        public static double GetViewportWidth(UIElement element)
        {
            return (double)element.GetValue(ViewportWidthProperty);
        }

        public static double GetViewportHeight(UIElement element)
        {
            return (double)element.GetValue(ViewportHeightProperty);
        }

        public static void SetHorizontalOffset(UIElement element, double value)
        {
            element.SetValue(HorizontalOffsetProperty, value);
        }

        public static double GetHorizontalOffset(UIElement element)
        {
            return (double)element.GetValue(HorizontalOffsetProperty);
        }

        public static void SetVerticalOffset(UIElement element, double value)
        {
            element.SetValue(VerticalOffsetProperty, value);
        }

        public static double GetVerticalOffset(UIElement element)
        {
            return (double)element.GetValue(VerticalOffsetProperty);
        }

        private static void HorizontalOffsetChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            (dependencyObject as ScrollViewer)?.ScrollToHorizontalOffset((double)e.NewValue);
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            SetHorizontalOffset(this, e.HorizontalOffset);
            SetVerticalOffset(this, e.VerticalOffset);
            SetViewportHeight(this, e.ViewportHeight);
            SetViewportWidth(this, e.ViewportWidth);

            HorizontalOffsetChanged(this,
                new DependencyPropertyChangedEventArgs(
                    HorizontalOffsetProperty,
                    e.HorizontalOffset - e.HorizontalChange,
                    e.HorizontalOffset)
                );
            VerticalOffsetChanged(this,
                new DependencyPropertyChangedEventArgs(
                    VerticalOffsetProperty,
                    e.VerticalOffset - e.VerticalChange,
                    e.VerticalOffset)
                );

            RaiseEvent(e);
        }

        private static void VerticalOffsetChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            (dependencyObject as ScrollViewer)?.ScrollToVerticalOffset((double)e.NewValue);
        }
    }
}
