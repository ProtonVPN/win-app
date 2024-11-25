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

using ProtonVPN.Client.Core.Bases;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;

namespace ProtonVPN.Client.UI.Main.Features.NetShield;

public sealed partial class NetShieldWidgetView : IContextAware
{
    public NetShieldWidgetViewModel ViewModel { get; }

    public NetShieldWidgetView()
    {
        ViewModel = App.GetService<NetShieldWidgetViewModel>();

        InitializeComponent();
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnWidgetButtonPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.IsFeatureFlyoutOpened)
        {
            FlyoutShowOptions options = new()
            {
                ShowMode = FlyoutShowMode.Transient,
                Placement = FlyoutPlacementMode.LeftEdgeAlignedTop,
                Position = new Point(-16, -1)
            };
            WidgetFlyout.ShowAt(WidgetButton, options);
        }
        else
        {
            // When pointer is over the button, switch to transient mode so the flyout cannot be dismissed
            WidgetFlyout.ShowMode = FlyoutShowMode.Transient;
        }
    }

    private void OnWidgetButtonPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (ViewModel.IsFeatureFlyoutOpened)
        {
            // When pointer is not over the button, flyout can be dismissed when the pointer moves away
            WidgetFlyout.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
        }
    }

    private void OnWidgetFlyoutOpened(object sender, object e)
    {
        ViewModel.IsFeatureFlyoutOpened = true;
    }

    private void OnWidgetFlyoutClosed(object sender, object e)
    {
        ViewModel.IsFeatureFlyoutOpened = false;
    }
}