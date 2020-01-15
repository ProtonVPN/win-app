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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProtonVPN.Core.Wpf
{
    public class ClippingBorder : Border
    {
        protected override void OnRender(DrawingContext dc)
        {
            OnApplyChildClip();
            base.OnRender(dc);
        }

        public override UIElement Child
        {
            get => base.Child;
            set
            {
                if (Equals(Child, value)) return;

                Child?.SetValue(ClipProperty, _oldClip);
                _oldClip = value?.ReadLocalValue(ClipProperty);

                base.Child = value;
            }
        }

        protected virtual void OnApplyChildClip()
        {
            if (Child != null)
            {
                _clipRect.RadiusX = _clipRect.RadiusY = Math.Max(0.0, CornerRadius.TopLeft - BorderThickness.Left * 0.5);
                _clipRect.Rect = new Rect(Child.RenderSize);
                Child.Clip = _clipRect;
            }
        }

        private readonly RectangleGeometry _clipRect = new RectangleGeometry();
        private object _oldClip;
    }
}
