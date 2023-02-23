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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace ProtonVPN.Core.Wpf.Behavior
{
    public class AnimateBehavior : Behavior<UIElement>
    {
        private DoubleAnimation _lastAnimation;

        public static readonly DependencyProperty ToAnimateProperty =
            DependencyProperty.Register("ToAnimate", typeof(DependencyProperty),
                typeof(AnimateBehavior), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double),
                typeof(AnimateBehavior),
                new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.None, ValuePropertyChanged));

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(double),
                typeof(AnimateBehavior),
                new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty ResetOnProperty =
            DependencyProperty.Register("ResetOn", typeof(double),
                typeof(AnimateBehavior),
                new FrameworkPropertyMetadata(100.0d, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty AutoResetProperty =
            DependencyProperty.Register("AutoReset", typeof(bool),
                typeof(AnimateBehavior),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty ResetToProperty =
            DependencyProperty.Register("ResetTo", typeof(double),
                typeof(AnimateBehavior),
                new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty ResetDelayProperty =
            DependencyProperty.Register("ResetDelay", typeof(int),
                typeof(AnimateBehavior),
                new FrameworkPropertyMetadata(1000, FrameworkPropertyMetadataOptions.None));

        public DependencyProperty ToAnimate
        {
            get => (DependencyProperty)GetValue(ToAnimateProperty);
            set => SetValue(ToAnimateProperty, value);
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public bool AutoReset
        {
            get => (bool)GetValue(AutoResetProperty);
            set => SetValue(AutoResetProperty, value);
        }

        public double ResetOn
        {
            get => (double)GetValue(ResetOnProperty);
            set => SetValue(ResetOnProperty, value);
        }

        public int ResetDelay
        {
            get => (int)GetValue(ResetDelayProperty);
            set => SetValue(ResetDelayProperty, value);
        }

        public double ResetTo
        {
            get => (double)GetValue(ResetToProperty);
            set => SetValue(ResetToProperty, value);
        }

        /// <summary>
        /// Time in seconds
        /// </summary>
        public double Duration
        {
            get => (double)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = d as AnimateBehavior;
            item?.SetAnimatedValue(e.NewValue != null ? (double)e.NewValue : 0.0);
        }

        private void SetAnimatedValue(double newValue)
        {
            if (AssociatedObject == null)
            {
                return;
            }

            if (_lastAnimation != null)
                _lastAnimation.Completed -= AnimationCompleted;

            _lastAnimation = new DoubleAnimation(newValue,
                new Duration(TimeSpan.FromSeconds(Duration)))
            {
                DecelerationRatio = 0.7,
                AccelerationRatio = 0.3
            };

            _lastAnimation.Completed += AnimationCompleted;

            AssociatedObject.BeginAnimation(ToAnimate, _lastAnimation);
        }

        private async void AnimationCompleted(object sender, EventArgs e)
        {
            AssociatedObject.SetValue(ToAnimate, (double)AssociatedObject.GetValue(ToAnimate));
            AssociatedObject.BeginAnimation(ToAnimate, null);

            if (AutoReset)
            {
                if (Math.Abs((double)AssociatedObject.GetValue(ToAnimate) - ResetOn) < 0.001)
                {
                    await Task.Delay(ResetDelay);
                    AssociatedObject.SetValue(ToAnimate, ResetTo);
                }
            }
        }
    }
}
