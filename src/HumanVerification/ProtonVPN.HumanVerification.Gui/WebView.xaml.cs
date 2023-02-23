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
using System.Security.Cryptography.X509Certificates;
using Microsoft.Web.WebView2.Core;
using ProtonVPN.Api.Handlers.TlsPinning;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.HumanVerification.Contracts;

namespace ProtonVPN.HumanVerification.Gui
{
    public partial class WebView
    {
        private readonly ICertificateValidator _certificateValidator;
        private readonly IHumanVerificationConfig _humanVerificationConfig;
        private readonly ILogger _logger;

        public WebView(ICertificateValidator certificateValidator,
            IHumanVerificationConfig humanVerificationConfig,
            ILogger logger)
        {
            _certificateValidator = certificateValidator;
            _humanVerificationConfig = humanVerificationConfig;
            _logger = logger;
            InitializeComponent();
            InitializeWebView();
        }

        private void InitializeWebView()
        {
            try
            {
                if (_humanVerificationConfig.IsSupported())
                {
                    WebView2.CoreWebView2InitializationCompleted += OnCoreWebView2InitializationCompleted;
                }
            }
            catch (Exception e)
            {
                LogWebViewInitializationException(e);
            }
        }

        private async void OnCoreWebView2InitializationCompleted(object sender,
            CoreWebView2InitializationCompletedEventArgs e)
        {
            try
            {
                if (e is not null && e.IsSuccess)
                {
                    await WebView2.CoreWebView2.ClearServerCertificateErrorActionsAsync();
                    WebView2.CoreWebView2.ServerCertificateErrorDetected += OnServerCertificateErrorDetected;
                }
                else
                {
                    LogWebViewInitializationException(e?.InitializationException);
                }
            }
            catch (Exception ex)
            {
                _logger.Error<AppLog>("Failed to initialize CoreWebView2 server certificate handler.", ex);
            }
        }

        private void LogWebViewInitializationException(Exception e)
        {
            _logger.Error<AppLog>("Failed to initialize CoreWebView2.", e);
        }

        private void OnServerCertificateErrorDetected(object sender,
            CoreWebView2ServerCertificateErrorDetectedEventArgs e)
        {
            try
            {
                e.Action = IsCertificateValid(e)
                    ? CoreWebView2ServerCertificateErrorAction.AlwaysAllow
                    : CoreWebView2ServerCertificateErrorAction.Cancel;
            }
            catch (Exception ex)
            {
                _logger.Error<AppLog>("Failed to validate server certificate in a WebView.", ex);
            }
        }

        private bool IsCertificateValid(CoreWebView2ServerCertificateErrorDetectedEventArgs e)
        {
            X509Certificate2 certificate = e.ServerCertificate.ToX509Certificate2();
            Uri requestUri = new(e.RequestUri);
            CertificateValidationParams validationParams = new()
            {
                Certificate = certificate,
                Chain = e.ServerCertificate.PemEncodedIssuerCertificateChain,
                HasSslError = true,
                Host = requestUri.Host,
                RequestUri = requestUri,
            };

            return _certificateValidator.IsValid(validationParams);
        }
    }
}