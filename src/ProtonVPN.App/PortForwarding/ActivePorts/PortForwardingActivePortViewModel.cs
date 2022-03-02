/*
 * Copyright (c) 2022 Proton Technologies AG
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

using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.PortForwarding;
using ProtonVPN.Core.PortForwarding;

namespace ProtonVPN.PortForwarding.ActivePorts
{
    public class PortForwardingActivePortViewModel : Screen, IPortForwardingStateAware
    {
        public ICommand PortForwardingValueCopyCommand { get; set; }
        public ICommand PortForwardingValueDefaultColorCommand { get; set; }
        public ICommand PortForwardingValueHighlightColorCommand { get; set; }

        private string _portForwardingValue = string.Empty;
        public string PortForwardingValue
        {
            get => _portForwardingValue;
            set
            {
                _portForwardingValue = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _hasPortForwardingValue;
        public bool HasPortForwardingValue
        {
            get => _hasPortForwardingValue;
            set
            {
                _hasPortForwardingValue = value;
                NotifyOfPropertyChange();
            }
        }

        private string _portForwardingValueColor = PortForwardingColorPalette.DEFAULT;
        public string PortForwardingValueColor
        {
            get => _portForwardingValueColor;
            set => Set(ref _portForwardingValueColor, value);
        }

        public PortForwardingActivePortViewModel()
        {
            PortForwardingValueCopyCommand = new RelayCommand(PortForwardingValueCopyAction);
            PortForwardingValueDefaultColorCommand = new RelayCommand(PortForwardingValueDefaultColorAction);
            PortForwardingValueHighlightColorCommand = new RelayCommand(PortForwardingValueHighlightColorAction);
        }

        private void PortForwardingValueCopyAction()
        {
            Clipboard.SetText(PortForwardingValue);
        }

        private void PortForwardingValueDefaultColorAction()
        {
            PortForwardingValueColor = PortForwardingColorPalette.DEFAULT;
        }

        private void PortForwardingValueHighlightColorAction()
        {
            PortForwardingValueColor = PortForwardingColorPalette.HIGHLIGHT;
        }

        public void OnPortForwardingStateChanged(PortForwardingState state)
        {
            if (state?.MappedPort?.MappedPort?.ExternalPort is null)
            {
                PortForwardingValue = string.Empty;
                HasPortForwardingValue = false;
            }
            else
            {
                PortForwardingValue = state.MappedPort.MappedPort.ExternalPort.ToString();
                HasPortForwardingValue = true;
            }
        }
    }
}