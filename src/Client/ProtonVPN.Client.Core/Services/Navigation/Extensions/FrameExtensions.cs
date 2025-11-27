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

using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Core.Services.Navigation.Extensions;

public static class FrameExtensions
{
    public static object? TryGetContent(this Frame? frame, ILogger? logger = null)
    {
        try
        {
            return frame?.Content;
        }
        catch (Exception e)
        {
            logger?.Debug<AppLog>($"Cannot access Frame.Content. {e}. ");
            return null;
        }
    }

    public static Type? TryGetContentType(this Frame? frame, ILogger? logger = null)
    {
        return frame.TryGetContent(logger)?.GetType();
    }

    public static bool IsContentNull(this Frame? frame)
    {
        return frame.TryGetContent() == null;
    }

    public static bool TryClearContent(this Frame? frame, ILogger? logger = null)
    {
        try
        {
            if (frame?.Content != null)
            {
                frame.Content = null;
            }
            return frame != null;
        }
        catch (Exception e)
        {
            logger?.Debug<AppLog>($"Cannot clear Frame.Content. {e}.");
            return false;
        }
    }
}
