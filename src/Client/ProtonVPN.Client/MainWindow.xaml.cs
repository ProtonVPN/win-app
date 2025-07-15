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

using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Services.Activation;
using ProtonVPN.Client.UI.Main.Components;
using Windows.Foundation;
using Windows.Graphics;
using WinRT.Interop;
using static ProtonVPN.Client.Common.Interop.WindowHelper;

namespace ProtonVPN.Client;

public sealed partial class MainWindow : IActivationStateAware
{
    private const double TITLE_BAR_HEIGHT = 38.0;

    public MainWindowActivator WindowActivator { get; }
    public MainWindowOverlayActivator OverlayActivator { get; }
    private IEventMessageSender EventMessageSender { get; }

    private IntPtr _hWnd;
    private WindowProc? _newWndProc;
    private IntPtr _oldWndProc;

    public MainWindow()
    {
        WindowActivator = App.GetService<MainWindowActivator>();
        OverlayActivator = App.GetService<MainWindowOverlayActivator>();
        EventMessageSender = App.GetService<IEventMessageSender>();

        InitializeComponent();

        WindowActivator.Initialize(this);
        OverlayActivator.Initialize(this);
    }

    protected override void OnActivated(object sender, WindowActivatedEventArgs e)
    {
        base.OnActivated(sender, e);

        if (_hWnd != IntPtr.Zero)
        {
            return;
        }

        _hWnd = WindowNative.GetWindowHandle(this);
        // It is important to have this reference as a class field, otherwise
        // garbage collector deletes the reference which causes the app to crash.
        _newWndProc = new(CustomWndProc);
        _oldWndProc = SetWindowLongPtr(_hWnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));
    }

    private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_ENDSESSION:
                if (wParam != IntPtr.Zero)
                {
                    OnSessionEnded();
                }
                break;
        }

        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    private void OnSessionEnded()
    {
        EventMessageSender.Send<WindowsSessionEndingMessage>();
    }

    public void InvalidateTitleBarOpacity(WindowActivationState activationState)
    {
        if (WindowContainer != null)
        {
            WindowContainer.TitleBarOpacity = activationState.GetTitleBarOpacity();
        }
    }

    public void InvalidateTitleBarVisibility(bool isTitleBarVisible)
    {
        if (WindowContainer != null)
        {
            WindowContainer.IsTitleBarVisible = isTitleBarVisible;
        }

        IsMinimizable = isTitleBarVisible;
        IsMaximizable = isTitleBarVisible;
        IsResizable = isTitleBarVisible;

        InvalidateTitleDragArea();
    }

    public void InvalidateTitleDragArea()
    {
        bool isTitleBarVisible = WindowContainer?.IsTitleBarVisible ?? false;

        if (isTitleBarVisible && TitleBarMenuComponent != null)
        {
            Point position = this.GetRelativePosition(TitleBarMenuComponent);
            Size size = TitleBarMenuComponent.RenderSize;

            RectInt32 interactiveArea = new(
                _X: (int)position.X,
                _Y: (int)position.Y,
                _Width: (int)size.Width,
                _Height: (int)size.Height);

            this.SetDragArea(Width, TITLE_BAR_HEIGHT, interactiveArea);
        }
        else
        {
            this.SetDragArea(Width, TITLE_BAR_HEIGHT);
        };
    }

    protected override bool OnSizeChanged(Size newSize)
    {
        InvalidateTitleDragArea();

        return base.OnSizeChanged(newSize);
    }

    private void OnTitleBarMenuComponentSizeChanged(object sender, SizeChangedEventArgs e)
    {
        InvalidateTitleDragArea();
    }
}