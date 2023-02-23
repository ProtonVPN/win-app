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
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.HumanVerification.Contracts;

namespace ProtonVPN.HumanVerification.Gui
{
    public class WebViewViewModel : ViewModelBase, IWebViewViewModel
    {
        private readonly ILogger _logger;
        private readonly ICaptchaUrlProvider _captchaUrlProvider;

        private int _height;
        private string _requestToken = string.Empty;

        public CoreWebView2CreationProperties WebView2CreationProperties { get; }
        public ICommand OnMessageReceivedCommand { get; set; }

        public int Height
        {
            get => _height;
            set => Set(ref _height, value);
        }

        public string Url => _captchaUrlProvider.GetCaptchaUrl(_requestToken);

        public event EventHandler<string> OnHumanVerificationTokenReceived;

        public WebViewViewModel(IConfiguration config, ILogger logger, ICaptchaUrlProvider captchaUrlProvider)
        {
            _logger = logger;
            _captchaUrlProvider = captchaUrlProvider;

            WebView2CreationProperties = new CoreWebView2CreationProperties { UserDataFolder = config.LocalAppDataFolder };
            OnMessageReceivedCommand = new RelayCommand<CoreWebView2WebMessageReceivedEventArgs>(OnWebMessageReceived);
        }

        public void SetRequestToken(string token)
        {
            _requestToken = token;
            RaisePropertyChanged(nameof(Url));
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
                        OnHumanVerificationTokenReceived?.Invoke(this, message.Token);
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