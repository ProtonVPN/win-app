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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProtonVPN.Core.Wpf.Behavior
{
    public class TextBlockAutoToolTipBehavior : Behavior<TextBlock>
    {
        private ToolTip _toolTip;

        protected override void OnAttached()
        {
            base.OnAttached();
            _toolTip = new ToolTip
            {
                Placement = PlacementMode.Relative,
                VerticalOffset = 0,
                HorizontalOffset = 0
            };

            ToolTipService.SetShowDuration(_toolTip, int.MaxValue);

            _toolTip.SetBinding(ContentControl.ContentProperty, new Binding
            {
                Path = new PropertyPath("Text"),
                Source = AssociatedObject
            });

            AssociatedObject.TextTrimming = TextTrimming.CharacterEllipsis;
            AssociatedObject.AddValueChanged(TextBlock.TextProperty, TextBlockOnTextChanged);
            AssociatedObject.SizeChanged += AssociatedObjectOnSizeChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.RemoveValueChanged(TextBlock.TextProperty, TextBlockOnTextChanged);
            AssociatedObject.SizeChanged -= AssociatedObjectOnSizeChanged;
        }

        private void AssociatedObjectOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            CheckToolTipVisibility();
        }

        private void TextBlockOnTextChanged(object sender, EventArgs eventArgs)
        {
            CheckToolTipVisibility();
        }

        private void CheckToolTipVisibility()
        {
            if (AssociatedObject.ActualWidth == 0.0)
            {
                Dispatcher.BeginInvoke(
                    new Action(
                        () => AssociatedObject.ToolTip = IsTextTrimmed(AssociatedObject) ? _toolTip : null),
                    DispatcherPriority.Loaded);
            }
            else
            {
                AssociatedObject.ToolTip = IsTextTrimmed(AssociatedObject) ? _toolTip : null;
            }
        }

        private static bool IsTextTrimmed(TextBlock textBlock)
        {
            var typeface = new Typeface(
                textBlock.FontFamily,
                textBlock.FontStyle,
                textBlock.FontWeight,
                textBlock.FontStretch);

            // FormattedText is used to measure the size required to display the text
            // contained in the TextBlock control.
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Threading.Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection,
                typeface,
                textBlock.FontSize,
                textBlock.Foreground,
                VisualTreeHelper.GetDpi(textBlock).PixelsPerDip)
            {
                MaxTextWidth = textBlock.ActualWidth
            };

            // If the textBlock is being trimmed to fit then the formatted text will report
            // a larger height than the textBlock. The width of the formattedText might grow
            // if a single line is too long to fit within the text area, this can only happen
            // if there is a long text with no spaces.
            return (formattedText.Height > textBlock.ActualHeight || formattedText.MinWidth > formattedText.MaxTextWidth);
        }
    }
}
