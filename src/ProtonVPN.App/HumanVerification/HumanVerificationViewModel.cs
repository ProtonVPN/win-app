/*
 * Copyright (c) 2021 Proton Technologies AG
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
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Modals;

namespace ProtonVPN.HumanVerification
{
    public class HumanVerificationViewModel : BaseModalViewModel
    {
        private readonly ILogger _logger;
        private readonly Common.Configuration.Config _config;
        private string _requestToken = string.Empty;
        private int _height;

        public HumanVerificationViewModel(ILogger logger, Common.Configuration.Config config)
        {
            _logger = logger;
            _config = config;
            OnMessageReceivedCommand = new RelayCommand<CoreWebView2WebMessageReceivedEventArgs>(OnWebMessageReceived);
        }

        public ICommand OnMessageReceivedCommand { get; set; }

        public string Url => string.Format(_config.Urls.CaptchaUrl, _requestToken);

        public CoreWebView2CreationProperties WebView2CreationProperties => new()
        {
            UserDataFolder = _config.LocalAppDataFolder
        };

        public string ResponseToken { get; set; }

        public int Height
        {
            get => _height;
            set => Set(ref _height, value);
        }

        public override void BeforeOpenModal(dynamic token)
        {
            _requestToken = token;
            NotifyOfPropertyChange(nameof(Url));
        }

        private void OnWebMessageReceived(CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                CaptchaMessage message = JsonConvert.DeserializeObject<CaptchaMessage>(e.WebMessageAsJson);
                switch (message.Type)
                {
                    case CaptchaMessageTypes.Height:
                        Height = message.Height;
                        break;
                    case CaptchaMessageTypes.TokenResponse:
                        ResponseToken = message.Token;
                        TryClose(true);
                        break;
                }
            }
            catch (JsonException ex)
            {
                _logger.Error<AppLog>("Failed to deserialize webview message.", ex);
            }
        }
    }
}