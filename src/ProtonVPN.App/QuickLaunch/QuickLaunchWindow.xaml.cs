/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Windows.Input;
using ProtonVPN.Windows;

namespace ProtonVPN.QuickLaunch
{
    public partial class QuickLaunchWindow
    {
        private readonly IWindowPositionSetter _windowPositionSetter;

        public QuickLaunchWindow(IWindowPositionSetter windowPositionSetter)
        {
            _windowPositionSetter = windowPositionSetter;

            InitializeComponent();

            Deactivated += QuickLaunch_Deactivated;
            Activated += QuickLaunch_Activated;
        }

        private void QuickLaunch_Activated(object sender, EventArgs e)
        {
            _windowPositionSetter.SetPositionToMouse(this);
        }

        private void QuickLaunch_Deactivated(object sender, EventArgs e)
        {
            Hide();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Hide();
        }
    }
}
