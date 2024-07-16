/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Helpers;
using WindowState = WinUIEx.WindowState;

namespace ProtonVPN.Client.UI;

public static class WindowConfigurator
{
    public static void Set(WindowEx window, uint width, uint height)
    {
        window.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/ProtonVPN.ico"));
        window.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        InvalidateWindowPosition(window, width, height);
    }

    private static void InvalidateWindowPosition(WindowEx window, uint width, uint height)
    {
        WindowPositionParameters parameters = new()
        {
            Width = width,
            Height = height
        };

        window.WindowState = WindowState.Normal;
        window.SetPosition(parameters);
    }
}
