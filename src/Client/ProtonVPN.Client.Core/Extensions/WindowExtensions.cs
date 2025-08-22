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
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Interop;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Common.UI.Controls.Custom;
using ProtonVPN.Client.Common.UI.Windowing;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Common.Core.Helpers;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinUIEx;
using static Vanara.PInvoke.Shell32;
using Icon = System.Drawing.Icon;
using ProtonVPN.Client.Core.Bases;

namespace ProtonVPN.Client.Core.Extensions;

public static class WindowExtensions
{
    private static readonly Lazy<ITaskbarList3?> _taskbar = new(() =>
    {
        try
        {
            ITaskbarList3 taskbar = new();
            taskbar.HrInit();
            return taskbar;
        }
        catch
        {
            return null;
        }
    });

    public static void ApplyTheme(this Window window, ElementTheme theme)
    {
        window.AppWindow.TitleBar.BackgroundColor = Colors.Transparent;
        window.AppWindow.TitleBar.InactiveBackgroundColor = Colors.Transparent;

        window.AppWindow.TitleBar.ForegroundColor = ResourceHelper.GetColor("TextNormColor", theme);
        window.AppWindow.TitleBar.InactiveForegroundColor = ResourceHelper.GetColor("TextHintColor", theme);

        window.AppWindow.TitleBar.ButtonBackgroundColor = ResourceHelper.GetColor("InteractionDefaultEmptyColor", theme);
        window.AppWindow.TitleBar.ButtonHoverBackgroundColor = ResourceHelper.GetColor("InteractionDefaultHoverColor", theme);
        window.AppWindow.TitleBar.ButtonPressedBackgroundColor = ResourceHelper.GetColor("InteractionDefaultActiveColor", theme);
        window.AppWindow.TitleBar.ButtonInactiveBackgroundColor = ResourceHelper.GetColor("InteractionDefaultEmptyColor", theme);

        window.AppWindow.TitleBar.ButtonForegroundColor = ResourceHelper.GetColor("TextNormColor", theme);
        window.AppWindow.TitleBar.ButtonHoverForegroundColor = ResourceHelper.GetColor("TextNormColor", theme);
        window.AppWindow.TitleBar.ButtonPressedForegroundColor = ResourceHelper.GetColor("TextNormColor", theme);
        window.AppWindow.TitleBar.ButtonInactiveForegroundColor = ResourceHelper.GetColor("TextWeakColor", theme);

        if (window.Content is FrameworkElement element)
        {
            element.RequestedTheme = theme;
        }
    }

    public static void ApplyFlowDirection(this Window window, FlowDirection flowDirection)
    {
        ExtendedWindowStyle windowStyle = window.GetExtendedWindowStyle();

        if (flowDirection == FlowDirection.RightToLeft)
        {
            windowStyle |= ExtendedWindowStyle.LayoutRtl;
        }
        else
        {
            windowStyle &= ~ExtendedWindowStyle.LayoutRtl;
        }

        window.SetExtendedWindowStyle(windowStyle);

        if (window.Content is FrameworkElement element)
        {
            element.FlowDirection = flowDirection;
        }
    }

    public static void SetDragArea(this Window window, double width, double height)
    {
        double scaleAdjustment = window.GetDpiForWindow() / 96.0;

        // Scale the dimensions
        int scaledWidth = (int)(width * scaleAdjustment);
        int scaledHeight = (int)(height * scaleAdjustment);

        RectInt32 dragRect = new()
        {
            X = 0,
            Y = 0,
            Width = scaledWidth,
            Height = scaledHeight
        };

        window.AppWindow.TitleBar.SetDragRectangles([dragRect]);
    }

    public static Point GetRelativePosition(this Window window, UIElement element)
    {
        FrameworkElement? root = window.Content as FrameworkElement;

        if (root == null || element == null)
        {
            return new Point(0, 0);
        }

        GeneralTransform transform = element.TransformToVisual(root);

        return transform.TransformPoint(new Point(0, 0));
    }

    public static void SetDragArea(this Window window, double width, double height, RectInt32 gap)
    {
        double scaleAdjustment = window.GetDpiForWindow() / 96.0;

        // Scale the dimensions
        int scaledWidth = (int)(width * scaleAdjustment);
        int scaledHeight = (int)(height * scaleAdjustment);

        // Scale the gap
        RectInt32 scaledGap = new()
        {
            X = (int)(gap.X * scaleAdjustment),
            Y = (int)(gap.Y * scaleAdjustment),
            Width = (int)(gap.Width * scaleAdjustment),
            Height = (int)(gap.Height * scaleAdjustment)
        };

        List<RectInt32> dragRects = new();

        // Left area
        if (scaledGap.X > 0)
        {
            dragRects.Add(new RectInt32
            {
                X = 0,
                Y = 0,
                Width = scaledGap.X,
                Height = scaledHeight
            });
        }

        // Right area
        if (scaledGap.X + scaledGap.Width < scaledWidth)
        {
            dragRects.Add(new RectInt32
            {
                X = scaledGap.X + scaledGap.Width,
                Y = 0,
                Width = scaledWidth - (scaledGap.X + scaledGap.Width),
                Height = scaledHeight
            });
        }

        // Top area (if needed to leave a gap above the button)
        if (scaledGap.Y > 0)
        {
            dragRects.Add(new RectInt32
            {
                X = scaledGap.X,
                Y = 0,
                Width = scaledGap.Width,
                Height = scaledGap.Y
            });
        }

        // Bottom area (if needed to leave a gap below the button)
        if (scaledGap.Y + scaledGap.Height < scaledHeight)
        {
            dragRects.Add(new RectInt32
            {
                X = scaledGap.X,
                Y = scaledGap.Y + scaledGap.Height,
                Width = scaledGap.Width,
                Height = scaledHeight - (scaledGap.Y + scaledGap.Height)
            });
        }

        window.AppWindow.TitleBar.SetDragRectangles(dragRects.ToArray());
    }

    public static void MoveAndResize(this BaseWindow window, WindowPositionParameters parameters)
    {
        bool isPositionSpecified =
            parameters.XPosition is not null &&
            parameters.YPosition is not null;

        PointInt32 cursorPosition = MonitorCalculator.GetCursorPosition();

        DisplayArea displayArea = isPositionSpecified
            ? DisplayArea.GetFromRect(parameters.ToRect(), DisplayAreaFallback.Nearest)
            : DisplayArea.GetFromPoint(cursorPosition, DisplayAreaFallback.Primary);
        RectInt32 workArea = displayArea.WorkArea;

        // Get display area DPI
        uint dpi = displayArea.GetDpi();

        double maxWidth = workArea.Width.ToDips(dpi);
        double maxHeight = workArea.Height.ToDips(dpi);

        // Ensure the window size is within the work area limits
        double windowWidth = parameters.Width.Clamp(window.MinWidth, maxWidth);
        double windowHeight = parameters.Height.Clamp(window.MinHeight, maxHeight);

        double windowPositionX;
        double windowPositionY;
        if (isPositionSpecified)
        {
            // Ensure the window position is within the work area bounds
            windowPositionX = parameters.XPosition!.Value.Clamp(workArea.X, workArea.X + maxWidth - windowWidth);
            windowPositionY = parameters.YPosition!.Value.Clamp(workArea.Y, workArea.Y + maxHeight - windowHeight);
        }
        else
        {
            // No position specified, center the window on the current monitor.
            windowPositionX = workArea.X + ((maxWidth - windowWidth) / 2);
            windowPositionY = workArea.Y + ((maxHeight - windowHeight) / 2);
        }

        window.MoveAndResize(
            x: windowPositionX.ToPixels(dpi),
            y: windowPositionY.ToPixels(dpi),
            width: windowWidth,
            height: windowHeight);
    }

    public static void MoveNearTray(this BaseWindow window, double width, double height, double margin)
    {
        RectInt32? trayRect = MonitorCalculator.GetTrayRect();
        PointInt32 cursorPosition = MonitorCalculator.GetCursorPosition();

        // Determine the display area for the system tray.
        // 1. Ideally use the tray rectangle if available.
        // 2. If not available, for Windows 11 or higher, the system tray should always be on the primary display.
        // 3. for Windows 10, use the display area where the cursor is located.
        DisplayArea displayArea = trayRect.HasValue
            ? DisplayArea.GetFromRect(trayRect.Value, DisplayAreaFallback.Primary)
            : OSVersion.IsWindows11OrHigher()
                ? DisplayArea.Primary
                : DisplayArea.GetFromPoint(cursorPosition, DisplayAreaFallback.Primary);
        RectInt32 workArea = displayArea.WorkArea;

        // Get display area DPI and taskbard edge
        uint dpi = displayArea.GetDpi();
        TaskbarEdge taskbarEdge = MonitorCalculator.GetTaskbarEdge();

        double maxWidth = workArea.Width.ToDips(dpi);
        double maxHeight = workArea.Height.ToDips(dpi);

        // Ensure the window size is within the work area limits
        double windowWidth = width.Clamp(window.MinWidth, maxWidth - (2 * margin));
        double windowHeight = height.Clamp(window.MinHeight, maxHeight - (2 * margin));

        // Calculate the position based on the taskbar edge
        double windowPositionX = taskbarEdge switch
        {
            TaskbarEdge.Left => workArea.X + margin, // Dock to the left
            _ => workArea.X + maxWidth - windowWidth - margin // Dock to the right
        };
        double windowPositionY = taskbarEdge switch
        {
            TaskbarEdge.Top => workArea.Y + margin, // Dock to the top
            _ => workArea.Y + maxHeight - windowHeight - margin // Dock to the bottom
        };

        window.MoveAndResize(
            x: windowPositionX.ToPixels(dpi),
            y: windowPositionY.ToPixels(dpi),
            width: windowWidth,
            height: windowHeight);
    }

    public static async Task<string> PickSingleFileAsync(this Window window, string filterName, string[] filterFileExtensions)
    {
        try
        {
            FileOpenPicker picker = window.CreateOpenFilePicker();
            // Note: Filter name (eg. Image Files) is not supported by the FileOpenPicker
            foreach (string fileExtension in filterFileExtensions)
            {
                picker.FileTypeFilter.Add(fileExtension);
            }

            StorageFile file = await picker.PickSingleFileAsync();

            return file?.Path ?? string.Empty;
        }
        catch (Exception)
        {
            // The method above fails when the app run in elevated mode. Use Win32 API instead.
            return RuntimeHelper.PickSingleFile(window, filterName, filterFileExtensions);
        }
    }

    public static double GetTitleBarOpacity(this Window window)
    {
        return window is IFocusAware focusAware && focusAware.IsFocused()
            ? OpacityConstants.TITLE_BAR_FOCUSED
            : OpacityConstants.TITLE_BAR_UNFOCUSED;
    }

    public static XamlRoot GetXamlRoot(this Window window)
    {
        return window.Content?.XamlRoot
            ?? throw new InvalidOperationException("Cannot proceed, XamlRoot is undefined.");
    }

    public static void CenterOnMainWindowMonitor(this Window window, Window mainWindow)
    {
        // Get the monitor area where the main window is located
        RectInt32 monitorArea = DisplayArea.GetFromWindowId(mainWindow.AppWindow.Id, DisplayAreaFallback.Nearest).WorkArea;

        window.Move(monitorArea.X, monitorArea.Y);
        window.CenterOnScreen();
    }

    public static void SetBadge(this Window window, Icon? badgeIcon)
    {
        try
        {
            if (IsTaskbarBadgeSupported())
            {
                IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                _taskbar.Value?.SetOverlayIcon(hWnd, badgeIcon?.Handle ?? IntPtr.Zero, string.Empty);
            }
        }
        catch { }
    }

    public static void ClearBadge(this Window window)
    {
        window.SetBadge(null);
    }

    private static bool IsTaskbarBadgeSupported()
    {
        try
        {
            return OSVersion.IsOrHigherThan(OSVersion.TaskbarBadgeMinimumWindowsVersion)
                && _taskbar.Value != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static RectInt32 ToRect(this WindowPositionParameters parameters)
    {
        return new RectInt32(
            Convert.ToInt32(parameters.XPosition ?? 0),
            Convert.ToInt32(parameters.YPosition ?? 0),
            Convert.ToInt32(parameters.Width),
            Convert.ToInt32(parameters.Height));
    }

    private static double Clamp(this double value, double min, double max)
    {
        try
        {
            return min >= max ? max : Math.Clamp(value, min, max);
        }
        catch (Exception)
        {
            return Math.Min(min, max);
        }
    }

    public static class OpacityConstants
    {
        public const double TITLE_BAR_FOCUSED = 1.0;
        public const double TITLE_BAR_UNFOCUSED = 0.6;
    }
}