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
using ProtonVPN.Account;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Config.Url;

namespace ProtonVPN.Windows.Popups.Offers
{
    public class OfferPopupViewModel : BasePopupViewModel
    {
        private readonly ILogger _logger;
        private readonly IOsProcesses _processes;
        private readonly IWebAuthenticator _webAuthenticator;

        private Panel _panel;
        public Panel Panel
        {
            get => _panel;
            set
            {
                Set(ref _panel, value);
                OnPanelChange(value);
            }
        }

        private bool _isToDisplayPill;
        public bool IsToDisplayPill
        {
            get => _isToDisplayPill;
            set => Set(ref _isToDisplayPill, value);
        }

        private bool _isToDisplayButton;
        public bool IsToDisplayButton
        {
            get => _isToDisplayButton;
            set => Set(ref _isToDisplayButton, value);
        }

        private bool _isToDisplayFullScreenImage;
        public bool IsToDisplayFullScreenImage
        {
            get => _isToDisplayFullScreenImage;
            set => Set(ref _isToDisplayFullScreenImage, value);
        }

        private string _fullScreenImageSource;
        public string FullScreenImageSource
        {
            get => _fullScreenImageSource;
            set => Set(ref _fullScreenImageSource, value);
        }

        private ActiveUrl _buttonUrl;

        public ICommand ButtonCommand { get; set; }
        public ICommand FullScreenImageButtonCommand { get; set; }

        public OfferPopupViewModel(
            AppWindow appWindow,
            ILogger logger,
            IOsProcesses processes,
            IWebAuthenticator webAuthenticator) : base(appWindow)
        {
            _logger = logger;
            _processes = processes;
            _webAuthenticator = webAuthenticator;

            ButtonCommand = new RelayCommand(ButtonAction);
            FullScreenImageButtonCommand = new RelayCommand(OpenFullScreeImageButtonLink);
        }

        private void OnPanelChange(Panel value)
        {
            if (!string.IsNullOrEmpty(value?.Button?.Url))
            {
                _buttonUrl = new ActiveUrl(_processes, value.Button.Url)
                    .WithQueryParams(new() { { "utm_source", "windowsvpn" } });
            }

            SetFullScreenImage(value);
            IsToDisplayPill = value != null && value.Pill != null && !value.Pill.IsNullOrEmpty();
            IsToDisplayButton = value != null && value.Button != null && !value.Button.Text.IsNullOrEmpty();
        }

        private void SetFullScreenImage(Panel panel)
        {
            if (panel?.FullScreenImage?.Source != null)
            {
                IsToDisplayFullScreenImage = true;
                FullScreenImageSource = panel.FullScreenImage?.Source;
            }
            else
            {
                IsToDisplayFullScreenImage = false;
                FullScreenImageSource = null;
            }
        }

        private async void OpenFullScreeImageButtonLink()
        {
            if (Panel.Button.Action == "OpenURL")
            {
                string url = Panel.Button.Behaviors.Contains("AutoLogin")
                    ? await _webAuthenticator.GetLoginUrlAsync(Panel.Button.Url)
                    : Panel.Button.Url;
                _processes.Open(url);
            }
            else
            {
                _logger.Error<AppLog>($"Unsupported button action {Panel.Button.Action}");
            }

            TryClose();
        }

        protected virtual void ButtonAction()
        {
            if (_buttonUrl != null)
            {
                _buttonUrl.Open();
                TryClose();
            }
        }
    }
}