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

namespace ProtonVPN.Resource
{
    public class BasePopupWindow : WindowBase
    {
        private bool _isStartupPositionSet;
        private Window _owner;

        public BasePopupWindow()
        {
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetStartupPosition();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            SetStartupPosition();
        }

        protected void ForceSetStartupPosition()
        {
            _isStartupPositionSet = false;
            SetStartupPosition();
        }

        private void SetStartupPosition()
        {
            object dataContext = DataContext;
            if (!_isStartupPositionSet && dataContext != null && ActualWidth > 0 && ActualHeight > 0)
            {
                SetOwner(dataContext);
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = _owner.Left + ((_owner.ActualWidth - ActualWidth) / 2);
                Top = _owner.Top + ((_owner.ActualHeight - ActualHeight) / 2);
                _isStartupPositionSet = true;
            }
        }

        private void SetOwner(dynamic dataContext)
        {
            _owner = dataContext.Owner;
            SetInitialWindowState();
            _owner.StateChanged += OnParentWindowStateChanged;
            _owner.IsVisibleChanged += OnParentWindowIsVisibleChanged;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            SetInitialWindowState();
        }

        private void SetInitialWindowState()
        {
            if (_owner != null)
            {
                if (_owner.IsVisible &&
                    _owner.WindowState != WindowState.Minimized)
                {
                    OpenAndBringToFront();
                }
                else
                {
                    WindowState = WindowState.Minimized;
                    Topmost = false;
                }
            }
        }

        private void OpenAndBringToFront()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            ResetZOrderAndSetAsTopmost();
        }

        private void ResetZOrderAndSetAsTopmost()
        {
            Topmost = false;
            Topmost = true;
        }

        private void OnParentWindowStateChanged(object sender, EventArgs e)
        {
            SetWindowState();
        }

        private void OnParentWindowIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetWindowState();
        }

        private void SetWindowState()
        {
            if (_owner.IsVisible &&
                _owner.WindowState != WindowState.Minimized)
            {
                OpenAndBringToFront();
            }
        }
    }
}
