/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Client.Common.UI.Extensions;
using ProtonVPN.Client.Core.Bases;

namespace ProtonVPN.Client.Extensions;

public static class WindowExtensions
{
    public static Window? GetHostWindow(this DependencyObject control)
    {
        App app = (App)Application.Current;
        if (IsControlInWindow(control, app.MainWindow))
        {
            return app.MainWindow;
        }

        return IsControlInWindow(control, app.TrayWindow) ? app.TrayWindow : (Window?)null;
    }

    public static bool IsParentWindowFocused(this DependencyObject control)
    {
        Window? window = control.GetHostWindow();

        if (window is IFocusAware focusAware)
        {
            return focusAware.IsFocused();
        }

        return false;
    }

    private static bool IsControlInWindow(DependencyObject control, Window? window)
    {
        return window?.Content is FrameworkElement windowContent && control.IsDescendantOf(windowContent);
    }
}