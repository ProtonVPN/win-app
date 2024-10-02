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
using Microsoft.UI.Xaml.Controls.Primitives;

namespace ProtonVPN.Client.Legacy.UI.Home.Recents;

public sealed partial class RecentsComponent
{
    public RecentsViewModel ViewModel { get; }

    public RecentsComponent()
    {
        ViewModel = App.GetService<RecentsViewModel>();

        InitializeComponent();
    }

    private void OnActionsMenuFlyoutClosing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
    {
        // Because the list of Pinned/Recent item is rebuilt after unpin or remove, we need to reset focus on the recent components
        Focus(FocusState.Programmatic);
    }
}