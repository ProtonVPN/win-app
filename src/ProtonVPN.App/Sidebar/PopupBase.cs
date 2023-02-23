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
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using ProtonVPN.Core.Native;
using ProtonVPN.Core.Native.Structures;

namespace ProtonVPN.Sidebar
{
    public class PopupBase : Popup
    {
        private bool? _isCurrentlyForeground;
        private bool _isLoaded;
        private Window _parentWindow;

        public static readonly DependencyProperty IsRequestedToBeOpenProperty = DependencyProperty.Register("IsRequestedToBeOpen",
            typeof(bool), typeof(PopupBase), new FrameworkPropertyMetadata(false, OnIsRequestedToBeOpenChanged));
        
        public bool IsRequestedToBeOpen
        {
            get => (bool)GetValue(IsRequestedToBeOpenProperty);
            set
            {
                SetValue(IsRequestedToBeOpenProperty, value);
                if (_parentWindow != null)
                {
                    bool isOpen = value &&
                        _parentWindow.IsVisible &&
                        _parentWindow.WindowState != WindowState.Minimized &&
                        _parentWindow.IsActive;
                    SetIsOpen(isOpen);
                }
            }
        }

        public PopupBase()
        {
            Loaded += OnPopupLoaded;
            Unloaded += OnPopupUnloaded;
        }

        private void OnPopupLoaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                return;
            }

            _isLoaded = true;
            Child?.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnChildPreviewMouseLeftButtonDown), true);
            _parentWindow = Window.GetWindow(this);

            if (_parentWindow != null)
            {
                _parentWindow.LocationChanged += OnParentWindowLocationChanged;
                _parentWindow.StateChanged += OnParentWindowStateChanged;
                _parentWindow.IsVisibleChanged += OnParentWindowIsVisibleChanged;
                _parentWindow.Activated += OnParentWindowActivated;
                _parentWindow.Deactivated += OnParentWindowDeactivated;
            }
        }

        private void OnPopupUnloaded(object sender, RoutedEventArgs e)
        {
            if (_parentWindow != null)
            {
                _parentWindow.LocationChanged -= OnParentWindowLocationChanged;
                _parentWindow.StateChanged -= OnParentWindowStateChanged;
                _parentWindow.IsVisibleChanged -= OnParentWindowIsVisibleChanged;
                _parentWindow.Activated -= OnParentWindowActivated;
                _parentWindow.Deactivated -= OnParentWindowDeactivated;
            }
        }

        private void OnParentWindowLocationChanged(object sender, EventArgs e)
        {
            RepositionPopup();
        }

        private void RepositionPopup()
        {
            HorizontalOffset += 1;
            HorizontalOffset -= 1;
        }

        private void OnParentWindowStateChanged(object sender, EventArgs e)
        {
            SetIsOpen(_parentWindow.WindowState != WindowState.Minimized);
        }

        private void SetIsOpen(bool isOpen)
        {
            bool isForeground = isOpen && IsRequestedToBeOpen;
            IsOpen = isForeground;
            SetZIndexIfPossible(isForeground);
            RepositionPopup();
        }

        private void OnParentWindowIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool value)
            {
                SetIsOpen(value);
            }
        }

        private void OnParentWindowActivated(object sender, EventArgs e)
        {
            if (!IsOpen)
            {
                SetIsOpen(true);
            }
            SetZIndexIfPossible(true);
        }

        private void OnParentWindowDeactivated(object sender, EventArgs e)
        {
            SetZIndexIfPossible(false);
        }

        private void OnChildPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetZIndexIfPossible(true);

            if (!_parentWindow.IsActive)
            {
                _parentWindow.Activate();
            }
        }

        private static void OnIsRequestedToBeOpenChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            PopupBase popupBase = (PopupBase)dependencyObject;
            popupBase.IsRequestedToBeOpen = (bool)e.NewValue;
        }

        protected override void OnOpened(EventArgs e)
        {
            SetZIndexIfPossible(true);
            base.OnOpened(e);
        }

        private void SetZIndexIfPossible(bool isForeground)
        {
            if (_isCurrentlyForeground == isForeground || Child == null || !(PresentationSource.FromVisual(Child) is HwndSource hwndSource))
            {
                return;
            }

            IntPtr windowHandle = hwndSource.Handle;
            Rectangle? rectangle = WindowPositionExtensions.GetWindowRectangle(windowHandle);

            if (rectangle != null)
            {
                SetZIndex(windowHandle, isForeground, rectangle.Value);
            }
        }

        private void SetZIndex(IntPtr windowHandle, bool isForeground, Rectangle rectangle)
        {
            WindowPosition windowPosition = new()
            {
                IsForegroundWindow = isForeground,
                X = rectangle.Left,
                Y = rectangle.Top,
                Width = (int)Width,
                Height = (int)Height,
            };

            WindowPositionExtensions.SetWindowPosition(windowHandle, windowPosition);

            _isCurrentlyForeground = isForeground;
        }
    }
}