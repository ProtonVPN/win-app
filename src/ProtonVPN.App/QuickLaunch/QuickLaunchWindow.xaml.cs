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
using System.Windows.Input;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Windows;

namespace ProtonVPN.QuickLaunch
{
    public partial class QuickLaunchWindow
    {
        private readonly ILogger _logger;
        private readonly IWindowPositionSetter _windowPositionSetter;

        public QuickLaunchWindow(IWindowPositionSetter windowPositionSetter, ILogger logger)
        {
            _windowPositionSetter = windowPositionSetter;
            _logger = logger;

            InitializeComponent();

            Deactivated += QuickLaunch_Deactivated;
            Activated += QuickLaunch_Activated;
        }

        private void QuickLaunch_Activated(object sender, EventArgs e)
        {
            _logger.Info<AppLog>("The QuickLaunchWindow is now focused, setting its position.");
            _windowPositionSetter.SetPositionToMouse(this);
        }

        private void QuickLaunch_Deactivated(object sender, EventArgs e)
        {
            _logger.Info<AppLog>("The QuickLaunchWindow is no longer focused and is going to be hidden.");
            Hide();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _logger.Info<AppLog>("The mouse left button was pressed, the QuickLaunchWindow is going to be hidden.");
            Hide();
        }
    }
}
