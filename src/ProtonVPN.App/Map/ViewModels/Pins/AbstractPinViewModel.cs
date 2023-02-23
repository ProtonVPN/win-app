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

using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Servers.Name;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Map.ViewModels.Pins
{
    public abstract class AbstractPinViewModel : ViewModel
    {
        public double OrigWidth => 34;
        public double OrigHeight => 55;
        public string CountryCode { get; set; }

        private double _width;
        private double _height;

        private double _horizontalOffset;
        private double _verticalOffset;

        private bool _connected;

        public double HorizontalOffset
        {
            get => _horizontalOffset;
            set => Set(ref _horizontalOffset, value);
        }

        public double VerticalOffset
        {
            get => _verticalOffset;
            set => Set(ref _verticalOffset, value);
        }

        public double Width
        {
            get => _width;
            set => Set(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => Set(ref _height, value);
        }

        private PinStates _state;
        public PinStates State
        {
            get => _state;
            set
            {
                Set(ref _state, value);
                Connected = value == PinStates.Connected;
            }
        }

        public bool Connected
        {
            get => _connected;
            set => Set(ref _connected, value);
        }

        public bool Highlighted { get; set; }

        private bool _showTooltip;
        public bool ShowTooltip
        {
            get => _showTooltip;
            set
            {
                if (_showTooltip == value) return;
                var change = value ? BeforeShowTooltip() : BeforeHideTooltip();
                if (change)
                {
                    Set(ref _showTooltip, value);
                }
            }
        }

        public IName ConnectionName { get; set; }

        protected AbstractPinViewModel(string countryCode)
        {
            var coordinates = new CountryLocation(countryCode).Coordinates();
            HorizontalOffset = coordinates.X;
            VerticalOffset = coordinates.Y;
            CountryCode = countryCode;
            Width = OrigWidth;
            Height = OrigHeight;

            ShowTooltipCommand = new RelayCommand(ShowTooltipAction);
            HideTooltipCommand = new RelayCommand(HideTooltipAction);
            ConnectCommand = new RelayCommand(ConnectAction);
        }

        protected abstract bool BeforeShowTooltip();
        protected abstract bool BeforeHideTooltip();
        protected abstract void ConnectAction();

        protected virtual void ShowTooltipAction()
        {
            ShowTooltip = true;
        }

        protected virtual void HideTooltipAction()
        {
            ShowTooltip = false;
        }

        public virtual void OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
        }

        public ICommand ShowTooltipCommand { get; set; }
        public ICommand HideTooltipCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand TouchConnectCommand { get; set; }
    }

    public enum PinStates
    {
        Disconnected,
        Connected
    }
}
