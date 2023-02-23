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

using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Map.ViewModels.Pins;
using ProtonVPN.Sidebar;
using ProtonVPN.SpeedGraph;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;

namespace ProtonVPN.Map.ViewModels
{
    internal class MapViewModel : Screen,
        IVpnStateAware,
        ISettingsAware,
        IPinChangeAware
    {
        private double _viewportHeight;
        private double _viewportWidth;

        private double _offsetY;
        private double _offsetX;

        private double _width;

        private const double InitialViewportWidth = 785.6;
        private const double InitialViewportHeight = 452.4;
        private const double InitialWidth = 2560;
        private const double VerticalScale = 0.882;
        private const double HorizontalScale = 0.493;

        private bool _connected;
        private bool _disconnected = true;
        private VpnStateChangedEventArgs _vpnState;

        private readonly IAppSettings _appSettings;
        private readonly PinFactory _pinFactory;
        private readonly MapLineManager _mapLineManager;

        public SpeedGraphViewModel SpeedGraphViewModel { get; }
        public SidebarViewModel SidebarViewModel { get; }

        public bool SecureCore => _appSettings.SecureCore;

        private bool _showAllProfilesButton;
        public bool ShowAllProfilesButton
        {
            get => _showAllProfilesButton;
            set
            {
                if (value == _showAllProfilesButton) return;
                _showAllProfilesButton = value;
                NotifyOfPropertyChange();
            }
        }

        private double _mapLineStroke;
        public double MapLineStroke
        {
            get => _mapLineStroke;
            set => Set(ref _mapLineStroke, value);
        }

        public double ViewportWidth
        {
            get => _viewportWidth;
            set => Set(ref _viewportWidth, value);
        }

        public double ViewportHeight
        {
            get => _viewportHeight;
            set => Set(ref _viewportHeight, value);
        }

        public double OffsetX
        {
            get => _offsetX;
            set => Set(ref _offsetX, value);
        }

        public double OffsetY
        {
            get => _offsetY;
            set => Set(ref _offsetY, value);
        }

        public double Width
        {
            get => _width;
            set
            {
                if (!(_width > value) && !(_width < value))
                    return;

                _width = value;
                NotifyOfPropertyChange();
            }
        }

        public bool Connected
        {
            get => _connected;
            set => Set(ref _connected, value);
        }

        public bool Disconnected
        {
            get => _disconnected;
            set => Set(ref _disconnected, value);
        }

        public ICommand ResizePins { get; }
        public ObservableCollection<ViewModel> Profiles { get; set; }

        private List<AbstractPinViewModel> _pinList;
        public List<AbstractPinViewModel> PinsList
        {
            get => _pinList;
            set => Set(ref _pinList, value);
        }

        private List<MapLine.MapLine> _lines = new List<MapLine.MapLine>();
        public List<MapLine.MapLine> Lines
        {
            get => _lines;
            set => Set(ref _lines, value);
        }

        public MapViewModel(
            IAppSettings appSettings,
            PinFactory pinFactory,
            MapLineManager pinLineManager,
            SpeedGraphViewModel speedGraphViewModel,
            SidebarViewModel sidebarViewModel)
        {
            _appSettings = appSettings;
            _pinFactory = pinFactory;
            _mapLineManager = pinLineManager;

            Width = InitialWidth;

            _viewportWidth = InitialViewportWidth;
            _viewportHeight = InitialViewportHeight;

            ResizePins = new RelayCommand<SizeChangedEventArgs>(ResizePinsCommand);

            SpeedGraphViewModel = speedGraphViewModel;
            SidebarViewModel = sidebarViewModel;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnState = e;

            switch (e.State.Status)
            {
                case VpnStatus.Connected:
                    _pinFactory.HideSecureCorePins();
                    _mapLineManager.SetConnectedLines(e.State.Server, true);
                    UpdateMapLines(Width);
                    Connected = true;
                    Disconnected = false;
                    break;
                case VpnStatus.Pinging:
                case VpnStatus.Connecting:
                case VpnStatus.Reconnecting:
                case VpnStatus.Disconnecting:
                case VpnStatus.Disconnected:
                    Connected = false;
                    Disconnected = true;

                    if (_appSettings.SecureCore)
                    {
                        ResetSecureCoreState();
                    }
                    else
                    {
                        _mapLineManager.ResetLineStates();
                        _pinFactory.HideTooltips();
                        _pinFactory.SetPinStates(PinStates.Disconnected);
                    }
                    break;
            }

            return Task.CompletedTask;
        }

        public void Load()
        {
            BuildMapElements();
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAppSettings.SecureCore))
            {
                BuildMapElements();
                if (_appSettings.SecureCore)
                {
                    _mapLineManager.ResetLineStates();
                    _pinFactory.HideTooltips();
                    _pinFactory.SetPinStates(PinStates.Disconnected);
                    _mapLineManager.SetSecureCoreLinesVisibility(true);
                }
                else
                {
                    ResetSecureCoreState();
                }

                NotifyOfPropertyChange(nameof(SecureCore));
            }
            else if (e.PropertyName == nameof(IAppSettings.Language))
            {
                GeneratePins();
            }
        }

        public void OnPinsChanged()
        {
            BuildMapElements();

            if (_vpnState != null)
            {
                OnVpnStateChanged(_vpnState);
                NotifyOfPropertyChange(nameof(Lines));
            }
        }

        private void BuildMapElements()
        {
            GeneratePins();
            GenerateLines();
            ResizeAllPins(Width);
            UpdateMapLines(Width);
        }

        private void GenerateLines()
        {
            Lines = _appSettings.SecureCore ? _mapLineManager.GetSecureCoreLines() : _mapLineManager.GetLines();
        }

        private void GeneratePins()
        {
            PinsList = _appSettings.SecureCore ?
                _pinFactory.GetSecureCorePins().ToList() :
                _pinFactory.GetPins().Values.ToList();
        }

        private void ResetSecureCoreState()
        {
            _mapLineManager.ResetSecureCoreLineStates();
            _mapLineManager.DisconnectSecureCoreHomeLine();
            _mapLineManager.DisconnectExitLines();
            _mapLineManager.HideHomeLines();
            _mapLineManager.HideExitLines();
            _pinFactory.SetSecureCorePinStates(PinStates.Disconnected);
            _pinFactory.HideSecureCorePins();
            _pinFactory.HideExitPins();
        }

        private void ResizePinsCommand(SizeChangedEventArgs e)
        {
            var newWidth = e.NewSize.Width;
            ResizeAllPins(newWidth);
            UpdateMapLines(newWidth);
            UpdateMapLineStroke(newWidth);
        }

        private double GetWScale(double newWidth)
        {
            return newWidth / Map.Views.Map.InitialWidth;
        }

        private void ResizeAllPins(double newWidth)
        {
            if (newWidth < Map.Views.Map.InitialWidth)
                newWidth = Map.Views.Map.InitialWidth;

            var wScale = GetWScale(newWidth);
            var hScale = wScale;

            foreach (var pin in PinsList)
            {
                var newPinWidth = pin.OrigWidth / wScale;
                var newPinHeight = pin.OrigHeight / hScale;

                var top = pin.VerticalOffset + (pin.Height - newPinHeight) * VerticalScale;
                var left = pin.HorizontalOffset + (pin.Width - newPinWidth) * HorizontalScale;

                pin.HorizontalOffset = left;
                pin.VerticalOffset = top;
                pin.Width = newPinWidth;
                pin.Height = newPinHeight;
            }
        }

        private void UpdateMapLineStroke(double newWidth)
        {
            MapLineStroke = 1 / GetWScale(newWidth);
        }

        private void UpdateMapLines(double newWidth)
        {
            var wScale = GetWScale(newWidth);
            foreach (var line in Lines)
            {
                line.ApplyMapScale(wScale);
            }
        }
    }
}
