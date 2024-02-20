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

using Microsoft.UI;
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Common.Interop;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Common.UI.Windowing.System;
using ProtonVPN.Client.Common.UI.Windowing;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ProtonVPN.Client.Helpers;

public static class WindowExtensions
{
    public static void ApplyTheme(this Window window, ElementTheme theme)
    {
        window.AppWindow.TitleBar.BackgroundColor = Colors.Transparent;
        window.AppWindow.TitleBar.InactiveBackgroundColor = Colors.Transparent;

        window.AppWindow.TitleBar.ForegroundColor = ResourceHelper.GetColor(theme, "TextNormColor");
        window.AppWindow.TitleBar.InactiveForegroundColor = ResourceHelper.GetColor(theme, "TextWeakColor");

        window.AppWindow.TitleBar.ButtonBackgroundColor = ResourceHelper.GetColor(theme, "InteractionDefaultColor");
        window.AppWindow.TitleBar.ButtonHoverBackgroundColor = ResourceHelper.GetColor(theme, "InteractionDefaultHoverColor");
        window.AppWindow.TitleBar.ButtonPressedBackgroundColor = ResourceHelper.GetColor(theme, "InteractionDefaultActiveColor");
        window.AppWindow.TitleBar.ButtonInactiveBackgroundColor = ResourceHelper.GetColor(theme, "InteractionDefaultColor");

        window.AppWindow.TitleBar.ButtonForegroundColor = ResourceHelper.GetColor(theme, "TextNormColor");
        window.AppWindow.TitleBar.ButtonHoverForegroundColor = ResourceHelper.GetColor(theme, "TextNormColor");
        window.AppWindow.TitleBar.ButtonPressedForegroundColor = ResourceHelper.GetColor(theme, "TextNormColor");
        window.AppWindow.TitleBar.ButtonInactiveForegroundColor = ResourceHelper.GetColor(theme, "TextDisabledColor");

        if (window.Content is FrameworkElement element)
        {
            element.RequestedTheme = theme;
        }
    }

    public static void SetDragAreaFromTitleBar(this Window window, FrameworkElement titleBar)
    {
        double scaleAdjustment = window.GetDpiForWindow() / 96.0;
        Point relativeFromWindow = titleBar.TransformToVisual(window.Content).TransformPoint(new Point(0, 0));

        RectInt32 dragRect;
        dragRect.Height = (int)(titleBar.ActualHeight * scaleAdjustment);
        dragRect.Width = (int)(titleBar.ActualWidth * scaleAdjustment);
        dragRect.X = (int)(relativeFromWindow.X * scaleAdjustment);
        dragRect.Y = (int)(relativeFromWindow.Y * scaleAdjustment);

        RectInt32[] gripArray = new[] { dragRect };
        window.AppWindow.TitleBar.SetDragRectangles(gripArray);
    }

    public static void SetPosition(this Window window, WindowPositionParameters parameters)
    {      
        if (parameters.XPosition is null || parameters.YPosition is null)
        {
            W32Point? point = MonitorCalculator.CalculateWindowCenteredInCursorMonitor(parameters.Width, parameters.Height);
            if (point is not null)
            {
                parameters.XPosition = point.Value.X;
                parameters.YPosition = point.Value.Y;
            }
        }

        if (parameters.XPosition is not null && parameters.YPosition is not null)
        {
            W32Rect windowRectangle = new(
                new W32Point((int)parameters.XPosition, (int)parameters.YPosition),
                width: (int)parameters.Width,
                height: (int)parameters.Height);
            W32Rect? validWindowRectangle = MonitorCalculator.GetValidWindowSizeAndPosition(windowRectangle);
            if (validWindowRectangle is not null)
            {
                window.MoveAndResize(
                    x: validWindowRectangle.Value.Left,
                    y: validWindowRectangle.Value.Top,
                    width: validWindowRectangle.Value.GetWidth(),
                    height: validWindowRectangle.Value.GetHeight());
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
}