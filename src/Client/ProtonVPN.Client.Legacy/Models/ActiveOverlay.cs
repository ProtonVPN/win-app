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

namespace ProtonVPN.Client.Legacy.Models;

public class ActiveOverlay
{
    public Window RootWindow { get; }

    public ContentDialog Dialog { get; }

    public ActiveOverlay(Window rootWindow, ContentDialog dialog)
    {
        RootWindow = rootWindow;
        Dialog = dialog;
    }

    public bool BelongsTo(Window rootWindow)
    {
        return RootWindow == rootWindow;
    }

    public bool IsOfType(Type overlayType)
    {
        return Dialog.GetType() == overlayType;
    }
}