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
using ProtonVPN.Client.Common.UI.Windowing;
using ProtonVPN.Client.Core.Helpers;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinUIEx;
using ProtonVPN.Common.Core.Helpers;
using Icon = System.Drawing.Icon;
using static Vanara.PInvoke.Shell32;

namespace ProtonVPN.Client.Core.Extensions;

public static class WindowExtensions
{
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

    public static void SetPosition(this Window window, WindowPositionParameters parameters)
    {
        if (parameters.XPosition is null || parameters.YPosition is null)
        {
            Point? point = MonitorCalculator.CalculateWindowCenteredInCursorMonitor(parameters.Width, parameters.Height);
            if (point is not null)
            {
                parameters.XPosition = point.Value.X;
                parameters.YPosition = point.Value.Y;
            }
        }

        if (parameters.XPosition is not null && parameters.YPosition is not null)
        {
            Rect windowRectangle = new(
                parameters.XPosition.Value,
                parameters.YPosition.Value,
                parameters.Width,
                parameters.Height);
            Rect? validWindowRectangle = MonitorCalculator.GetValidWindowSizeAndPosition(windowRectangle);
            if (validWindowRectangle is not null)
            {
                window.MoveAndResize(
                    x: validWindowRectangle.Value.Left,
                    y: validWindowRectangle.Value.Top,
                    width: validWindowRectangle.Value.Width,
                    height: validWindowRectangle.Value.Height);
            }
        }
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

    public static double GetTitleBarOpacity(this WindowActivationState activationState)
    {
        return activationState != WindowActivationState.Deactivated ? 1.0 : 0.6;
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

    public static void MoveNearTrayOnPrimaryMonitor(this Window window, double width, double height, double margin)
    {
        // Get the primary monitor area and taskbar edge
        RectInt32 monitorArea = DisplayArea.Primary.WorkArea;
        TaskbarEdge taskbarEdge = MonitorCalculator.GetTaskbarEdge();

        double scaleAdjustment = window.GetDpiForWindow() / 96.0;

        // Scale the dimensions
        int scaledWidth = (int)(width * scaleAdjustment);
        int scaledHeight = (int)(height * scaleAdjustment);
        int scaledMargin = (int)(margin * scaleAdjustment);

        int coercedWidth = Math.Min(monitorArea.Width - (2 * scaledMargin), scaledWidth);
        int coercedHeight = Math.Min(monitorArea.Height - (2 * scaledMargin), scaledHeight);

        int x = taskbarEdge switch
        {
            TaskbarEdge.Left => monitorArea.X + scaledMargin,
            _ => monitorArea.X + monitorArea.Width - coercedWidth - scaledMargin
        };
        int y = taskbarEdge switch
        {
            TaskbarEdge.Top => monitorArea.Y + scaledMargin,
            _ => monitorArea.Y + monitorArea.Height - coercedHeight - scaledMargin
        };

        window.MoveAndResize(x, y, coercedWidth / scaleAdjustment, coercedHeight / scaleAdjustment);
    }

    private static readonly Lazy<ITaskbarList3?> _taskbar = new(() =>
    {
        try
        {
            var taskbar = new ITaskbarList3();
            taskbar.HrInit();
            return taskbar;
        }
        catch
        {
            return null;
        }
    });

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
}