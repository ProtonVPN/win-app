/*
 * Copyright (c) 2020 Proton Technologies AG
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

using Caliburn.Micro;
using ProtonVPN.Core;
using ProtonVPN.Core.Events;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Native;
using ProtonVPN.Core.Settings;
using ProtonVPN.Onboarding;
using ProtonVPN.QuickLaunch;
using ProtonVPN.Sidebar;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using ProtonVPN.Core.Window;

namespace ProtonVPN.Windows
{
    internal partial class AppWindow :
        IHandle<ToggleOverlay>,
        IOnboardingStepAware
    {
        private const int SidebarWidth = 336;
        private const int DefaultWidth = 800;

        private readonly IAppSettings _appSettings;
        private readonly IEventAggregator _eventAggregator;
        private readonly QuickLaunchWindow _quickLaunchWindow;
        private readonly TrayContextMenu _trayContextMenu;

        private static FieldInfo _menuDropAlignmentField;
        private bool _sidebarModeBeforeMaximize;
        private bool _blurInProgress;
        private bool _blurOutInProgress;

        private readonly DoubleAnimation _blurInAnimation = new DoubleAnimation(10, TimeSpan.FromMilliseconds(200));
        private readonly DoubleAnimation _blurOutAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));

        public AppWindow(
            IEventAggregator eventAggregator,
            IAppSettings appSettings,
            QuickLaunchWindow quickLaunchWindow,
            TrayContextMenu trayContextMenu,
            TrayIcon trayIcon)
        {
            _trayContextMenu = trayContextMenu;
            _quickLaunchWindow = quickLaunchWindow;
            _appSettings = appSettings;
            _eventAggregator = eventAggregator;

            trayIcon.OnMouseEvent += TrayIconClick;

            InitializeComponent();

            MinimizeButton.Click += Minimize_Click;
            CloseButton.Click += CloseButton_Click;
            MaximizeButton.Click += MaximizeButton_Click;

            _menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);

            EnsureStandardPopupAlignment();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;

            _blurOutAnimation.Completed += OnBlurOutAnimationCompleted;
            _blurInAnimation.Completed += OnBlurInAnimationCompleted;
            Loaded += OnWindowLoaded;

            Deactivated += PublishWindowState;
            Activated += PublishWindowState;
        }

        public void Handle(ToggleOverlay message)
        {
            if (message.Show)
            {
                if (_blurOutInProgress)
                {
                    BlurBorder.Effect.BeginAnimation(BlurEffect.RadiusProperty, null);
                    BlurBorder.Effect = null;
                    _blurOutInProgress = false;
                }

                BlurBorder.Effect = new BlurEffect { Radius = 0 };
                BlurBorder.Effect.BeginAnimation(BlurEffect.RadiusProperty, _blurInAnimation);
                _blurInProgress = true;
            }
            else
            {
                if (_blurInProgress)
                {
                    BlurBorder.Effect.BeginAnimation(BlurEffect.RadiusProperty, null);
                    BlurBorder.Effect = null;
                    _blurInProgress = false;
                }

                BlurBorder.Effect = new BlurEffect { Radius = 10 };
                BlurBorder.Effect.BeginAnimation(BlurEffect.RadiusProperty, _blurOutAnimation);
                _blurOutInProgress = true;
            }
        }

        public void OnManualSidebarModeChangeRequested(object sender, SidebarModeEventArgs e)
        {
            _sidebarModeBeforeMaximize = e.Mode;

            if (e.Mode)
            {
                TurnOnSidebarMode();
            }
            else
            {
                TurnOffSidebarMode();
            }
        }

        public void OnStepChanged(int step)
        {
            var min = DefaultWidth;
            if (Width < min)
                Width = min;

            ResizeMode = step > 0 ? ResizeMode.NoResize : ResizeMode.CanResize;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState.Equals(WindowState.Maximized))
            {
                _appSettings.SidebarMode = false;
            }
            else
            {
                if (WindowState.Equals(WindowState.Minimized))
                    return;

                _appSettings.SidebarMode = _sidebarModeBeforeMaximize;
                if (_appSettings.SidebarMode)
                {
                    TurnOnSidebarMode();
                }
                else
                {
                    TurnOffSidebarMode();
                }

                SetWindowPlacement(true, false);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);

            if (_appSettings.StartMinimized == StartMinimizedMode.ToSystray)
            {
                Visibility = Visibility.Hidden;
                SetWindowPlacement(false, true);
                return;
            }

            if (_appSettings.StartMinimized == StartMinimizedMode.ToTaskbar)
            {
                WindowState = WindowState.Minimized;
                SetWindowPlacement(false, false);
                return;
            }

            if (!_appSettings.SidebarMode || _appSettings.SidebarWindowPlacement != null)
            {
                SetWindowPlacement(true, false);
            }

            SetToggleButton();
            SaveWindowsPlacement();
        }

        private void TrayIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Clicks == 1)
                {
                    _quickLaunchWindow.Show();
                    _quickLaunchWindow.Activate();
                }
                else
                {
                    Show();
                    Activate();
                }
            }
            else if(e.Button == MouseButtons.Right)
            {
                _trayContextMenu.Show();
                _trayContextMenu.Activate();
            }
        }

        private void SetToggleButton()
        {
            _appSettings.SidebarMode = ActualWidth <= SidebarWidth;
            _sidebarModeBeforeMaximize = _appSettings.SidebarMode;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 562) // WM_EXITSIZEMOVE after window move / size changed
            {
                SetToggleButton();

                if (!WindowState.Equals(WindowState.Maximized))
                    SaveWindowsPlacement();
            }

            return IntPtr.Zero;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _eventAggregator.Subscribe(this);
        }

        private void OnBlurOutAnimationCompleted(object sender, EventArgs e)
        {
            if (_blurInProgress)
                return;

            BlurBorder.Effect = null;
            _blurOutInProgress = false;
        }

        private void OnBlurInAnimationCompleted(object sender, EventArgs e)
        {
            _blurInProgress = false;
        }

        private static void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EnsureStandardPopupAlignment();
        }

        private static void EnsureStandardPopupAlignment()
        {
            if (SystemParameters.MenuDropAlignment && _menuDropAlignmentField != null)
            {
                _menuDropAlignmentField.SetValue(null, false);
            }
        }

        private void TurnOnSidebarMode()
        {
            WindowState = WindowState.Normal;
            Width = SidebarWidth;
            SaveWindowsPlacement();
        }

        private void TurnOffSidebarMode()
        {
            if (_appSettings.WindowPlacement != null)
            {
                var width = _appSettings.WindowPlacement.NormalPosition.Right -
                      _appSettings.WindowPlacement.NormalPosition.Left;
                Width = width * 96 / SystemParams.GetDpiX();
            }
            else
            {
                Width = DefaultWidth;
            }

            SaveWindowsPlacement();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Equals(WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SaveWindowsPlacement();
            Hide();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void SetWindowPlacement(bool restoreFromMinimizedState, bool hide)
        {
            var placement = _appSettings.SidebarMode ? _appSettings.SidebarWindowPlacement : _appSettings.WindowPlacement;
            if (placement == null)
                return;

            this.SetWindowPlacement(placement, restoreFromMinimizedState, hide);
        }

        private void SaveWindowsPlacement()
        {
            if (_appSettings.SidebarMode)
            {
                _appSettings.SidebarWindowPlacement = this.GetWindowPlacement();
            }
            else
            {
                var placement = this.GetWindowPlacement();
                var width = placement.NormalPosition.Right - placement.NormalPosition.Left;
                //Prevent saving window placement when snapping window in sidebar mode
                //to left side of the screen, because it provides wrong size.
                if (width == SidebarWidth && !_appSettings.SidebarMode)
                    return;

                _appSettings.WindowPlacement = this.GetWindowPlacement();
            }
        }

        private void PublishWindowState(object sender, EventArgs e)
        {
            _eventAggregator.PublishOnUIThread(new WindowStateMessage(IsActive));
        }
    }
}
