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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ProtonVPN.Client.Common.UI.Controls.Bases;

public abstract class DualConnectionRowButtonBase : ContentControl
{
    private const string POINTER_OVER_VISUAL_STATE = "PointerOver";
    private const string POINTER_NOT_OVER_VISUAL_STATE = "Normal";

    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        base.OnPointerEntered(e);

        VisualStateManager.GoToState(this, POINTER_OVER_VISUAL_STATE, true);
    }

    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        base.OnPointerExited(e);

        VisualStateManager.GoToState(this, POINTER_NOT_OVER_VISUAL_STATE, true);
    }
}
