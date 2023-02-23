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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.OperatingSystemLogs;
using ProtonVPN.Common.PortForwarding;
using ProtonVPN.Core.PortForwarding;
using ProtonVPN.Resource.Colors;

namespace ProtonVPN.PortForwarding.ActivePorts
{
    public class PortForwardingActivePortViewModel : Screen, IPortForwardingStateAware
    {
        private const int MAX_CLIPBOARD_TRIES = 4;
        private const int BASE_CLIPBOARD_RETRY_TIME_IN_MILLISECONDS = 100;
        private const bool LEAVE_CLIPBOARD_VALUE_AFTER_APP_EXIT = true;

        public ICommand PortForwardingValueCopyCommand { get; set; }
        public ICommand PortForwardingValueDefaultColorCommand { get; set; }
        public ICommand PortForwardingValueHighlightColorCommand { get; set; }
        
        private readonly ILogger _logger;

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

        private string _portForwardingValueColor;
        public string PortForwardingValueColor
        {
            get => _portForwardingValueColor;
            set => Set(ref _portForwardingValueColor, value);
        }

        private readonly Lazy<string> _highlightColor;
        private readonly Lazy<string> _defaultColor;

        public PortForwardingActivePortViewModel(IColorPalette colorPalette, ILogger logger)
        {
            _logger = logger;
            _highlightColor = new(() => colorPalette.GetStringByResourceName("InteractionNormAccentHoverBrushColor"));
            _defaultColor = new(() => colorPalette.GetStringByResourceName("TextNormBrushColor"));

            PortForwardingValueColor = _defaultColor.Value;
            PortForwardingValueCopyCommand = new RelayCommand(PortForwardingValueCopyActionAsync);
            PortForwardingValueDefaultColorCommand = new RelayCommand(PortForwardingValueDefaultColorAction);
            PortForwardingValueHighlightColorCommand = new RelayCommand(PortForwardingValueHighlightColorAction);
        }
        
        private async void PortForwardingValueCopyActionAsync()
        {
            int retryTimeInMilliseconds = BASE_CLIPBOARD_RETRY_TIME_IN_MILLISECONDS;
            for (int i = 1; i <= MAX_CLIPBOARD_TRIES; i++)
            {
                try
                {
                    Clipboard.SetDataObject(PortForwardingValue, LEAVE_CLIPBOARD_VALUE_AFTER_APP_EXIT);
                    break;
                }
                catch (Exception exception)
                {
                    string logMessage = $"Error when copying port number to clipboard. Try {i} of {MAX_CLIPBOARD_TRIES}.";
                    if (i == MAX_CLIPBOARD_TRIES)
                    {
                        _logger.Error<OperatingSystemLog>(logMessage, exception);
                        break;
                    }
                    _logger.Warn<OperatingSystemLog>(logMessage + $" Waiting {retryTimeInMilliseconds}ms.", exception);
                    await Task.Delay(TimeSpan.FromMilliseconds(retryTimeInMilliseconds));
                    retryTimeInMilliseconds *= 2;
                }
            }
        }

        private void PortForwardingValueDefaultColorAction()
        {
            PortForwardingValueColor = _defaultColor.Value;
        }

        private void PortForwardingValueHighlightColorAction()
        {
            PortForwardingValueColor = _highlightColor.Value;
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