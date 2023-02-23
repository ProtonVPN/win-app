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
using System.Windows.Forms;
using System.Windows.Interop;

namespace ProtonVPN.Windows.Popups.Offers
{
    public partial class OfferPopupView
    {
        private const int TITLE_BAR_HEIGHT = 36;
        public OfferPopupView()
        {
            InitializeComponent();
            Loaded += OnViewLoaded;
            FullscreenImage.SizeChanged += OnFullscreenImageSizeChanged;
        }

        private void OnFullscreenImageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ForceSetStartupPosition();
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            int availableHeight = (int)((GetWorkingAreaHeight() - GetButtonHeight() - TITLE_BAR_HEIGHT) / GetScaleFactor());
            // Use 60% of the available height so that we have some space above and below the popup
            availableHeight = (int)(availableHeight * 0.6);
            FullscreenImage.Height = availableHeight;
        }

        private double GetScaleFactor()
        {
            return PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M22;
        }

        private int GetButtonHeight()
        {
            return (int)(FullscreenImageButton.ActualHeight +
                         FullscreenImageButton.Margin.Top +
                         FullscreenImageButton.Margin.Bottom);
        }

        private int GetWorkingAreaHeight()
        {
            WindowInteropHelper window = new WindowInteropHelper(this);
            return Screen.FromHandle(window.Handle).WorkingArea.Height;
        }
    }
}