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
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ProtonVPN.Account;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core;
using ProtonVPN.Core.Events;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Native;
using ProtonVPN.Core.Native.Structures;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Core.Windows;
using ProtonVPN.Onboarding;
using ProtonVPN.QuickLaunch;
using ProtonVPN.Sidebar;

namespace ProtonVPN.Windows
{
    public partial class AppWindow :
        IHandle<ToggleOverlay>,
        IOnboardingStepAware,
        IVpnStateAware
    {
        private const int BLUR_AMOUNT = 20;
        private const int SIDEBAR_WIDTH = 336;
        private const int DEFAULT_WIDTH = 800;
        private const string ICON_PATH = "protonvpn.ico";
        private const string CONNECTED_ICON_PATH = "Resources/Assets/Images/Icons/systray-connected.ico";

        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly IAppSettings _appSettings;
        private readonly QuickLaunchWindow _quickLaunchWindow;
        private readonly TrayContextMenu _trayContextMenu;
        private readonly DoubleAnimation _blurInAnimation = new(BLUR_AMOUNT, TimeSpan.FromMilliseconds(200));
        private readonly DoubleAnimation _blurOutAnimation = new(0, TimeSpan.FromMilliseconds(200));
        private readonly ResourceIcon _icon;
        private readonly ResourceIcon _connectedIcon;

        private static FieldInfo _menuDropAlignmentField;
        private bool _sidebarModeBeforeMaximize;
        private bool _blurInProgress;
        private bool _blurOutInProgress;
        private bool _isConnected;
        private bool _isConnecting;

        public bool AllowWindowHiding;

        public AppWindow(
            ILogger logger,
            IEventAggregator eventAggregator,
            IAppSettings appSettings,
            QuickLaunchWindow quickLaunchWindow,
            TrayContextMenu trayContextMenu,
            TrayIcon trayIcon)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _appSettings = appSettings;
            _quickLaunchWindow = quickLaunchWindow;
            _trayContextMenu = trayContextMenu;

            trayIcon.OnMouseEvent += TrayIconClick;

            InitializeComponent();

            MinimizeButton.Click += Minimize_Click;
            CloseButton.Click += CloseButton_Click;
            MaximizeButton.Click += MaximizeButton_Click;

            _menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);

            SetGenericTooltipBehaviour();
            EnsureStandardPopupAlignment();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;

            _blurOutAnimation.Completed += OnBlurOutAnimationCompleted;
            _blurInAnimation.Completed += OnBlurInAnimationCompleted;
            Loaded += OnWindowLoaded;

            Deactivated += PublishWindowState;
            Activated += PublishWindowState;

            _icon = new(ICON_PATH);
            _connectedIcon = new(CONNECTED_ICON_PATH);
            _sidebarModeBeforeMaximize = _appSettings.SidebarMode;
        }

        private void SetGenericTooltipBehaviour()
        {
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(0));
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

                BlurBorder.Effect = new BlurEffect { Radius = BLUR_AMOUNT };
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
            int min = DEFAULT_WIDTH;
            if (Width < min)
            {
                Width = min;
            }

            if (step <= 0)
            {
                MinWidth = SIDEBAR_WIDTH;
            }
            else if (step > 0 && MinWidth < DEFAULT_WIDTH)
            {
                MinWidth = DEFAULT_WIDTH;
            }
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            UpdateIcon(e.State.Status);
            _isConnected = e.State.Status == VpnStatus.Connected;
            _isConnecting = e.State.Status != VpnStatus.Disconnecting &&
                            e.State.Status != VpnStatus.Disconnected &&
                            e.State.Status != VpnStatus.Connected;
            return Task.CompletedTask;
        }

        public void TriggerAccountInfoUpdate()
        {
            _eventAggregator.PublishOnUIThread(new UpdateVpnInfoMessage());
        }

        private void UpdateIcon(VpnStatus vpnStatus)
        {
            try
            {
                if (_isConnected && vpnStatus != VpnStatus.Connected)
                {
                    Icon = BitmapFrame.Create(_icon.GetIconStream());
                }
                else if (!_isConnected && vpnStatus == VpnStatus.Connected)
                {
                    Icon = BitmapFrame.Create(_connectedIcon.GetIconStream());
                }
            }
            catch (Exception exception)
            {
                _logger.Error<AppLog>("Failed to set window icon.", exception);
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState.Equals(WindowState.Maximized))
            {
                _appSettings.SidebarMode = false;
            }
            else if (!WindowState.Equals(WindowState.Minimized))
            {
                _appSettings.SidebarMode = _sidebarModeBeforeMaximize;
                if (_appSettings.SidebarMode)
                {
                    TurnOnSidebarMode();
                }
                else
                {
                    TurnOffSidebarMode();
                }

                SetWindowPlacement(WindowStates.Normal);
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

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);

            if (AllowWindowHiding)
            {
                if (_appSettings.StartMinimized == StartMinimizedMode.ToSystray)
                {
                    SetWindowPlacement(WindowStates.Hidden);
                    return;
                }

                if (_appSettings.StartMinimized == StartMinimizedMode.ToTaskbar)
                {
                    WindowState = WindowState.Minimized;
                    SetWindowPlacement(WindowStates.Minimized);
                    return;
                }
            }

            if (!_appSettings.SidebarMode || _appSettings.SidebarWindowPlacement != null)
            {
                SetWindowPlacement(WindowStates.Normal);
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
                    if (_isConnecting)
                    {
                        Handle(new ToggleOverlay(true));
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                _trayContextMenu.Show();
                _trayContextMenu.Activate();
            }
        }

        private void SetToggleButton()
        {
            _appSettings.SidebarMode = ActualWidth <= SIDEBAR_WIDTH;
            _sidebarModeBeforeMaximize = _appSettings.SidebarMode;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 562) // WM_EXITSIZEMOVE after window move / size changed
            {
                SetToggleButton();

                if (!WindowState.Equals(WindowState.Maximized))
                {
                    SaveWindowsPlacement();
                }
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
            {
                return;
            }

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
            Width = SIDEBAR_WIDTH;
            SaveWindowsPlacement();
        }

        private void TurnOffSidebarMode()
        {
            if (_appSettings.WindowPlacement != null)
            {
                int width = _appSettings.WindowPlacement.NormalPosition.Right -
                            _appSettings.WindowPlacement.NormalPosition.Left;
                Width = width * 96 / SystemParams.GetDpiX();
            }
            else
            {
                Width = DEFAULT_WIDTH;
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

        private void SetWindowPlacement(WindowStates windowState)
        {
            WindowPlacement placement = _appSettings.SidebarMode ? _appSettings.SidebarWindowPlacement : _appSettings.WindowPlacement;

            if (placement != null)
            {
                this.SetWindowPlacement(placement, windowState);
            }
        }

        private void SaveWindowsPlacement()
        {
            if (_appSettings.SidebarMode)
            {
                _appSettings.SidebarWindowPlacement = this.GetWindowPlacement();
            }
            else
            {
                WindowPlacement placement = this.GetWindowPlacement();
                int width = placement.NormalPosition.Right - placement.NormalPosition.Left;
                //Prevent saving window placement when snapping window in sidebar mode
                //to left side of the screen, because it provides wrong size.
                if (width == SIDEBAR_WIDTH && !_appSettings.SidebarMode)
                {
                    return;
                }

                _appSettings.WindowPlacement = this.GetWindowPlacement();
            }
        }

        private void PublishWindowState(object sender, EventArgs e)
        {
            _eventAggregator.PublishOnUIThread(new WindowStateMessage(IsActive));
        }
    }
}
