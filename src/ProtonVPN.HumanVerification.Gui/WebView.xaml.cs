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

using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Web.WebView2.Core;
using ProtonVPN.Api.Handlers.TlsPinning;

namespace ProtonVPN.HumanVerification.Gui
{
    public partial class WebView
    {
        private readonly ICertificateValidator _certificateValidator;

        public WebView(ICertificateValidator certificateValidator)
        {
            _certificateValidator = certificateValidator;
            InitializeComponent();
            WebView2.CoreWebView2InitializationCompleted += OnCoreWebView2InitializationCompleted;
        }

        private async void OnCoreWebView2InitializationCompleted(object sender,
            CoreWebView2InitializationCompletedEventArgs e)
        {
            await WebView2.CoreWebView2.ClearServerCertificateErrorActionsAsync();
            WebView2.CoreWebView2.ServerCertificateErrorDetected += OnServerCertificateErrorDetected;
        }

        private void OnServerCertificateErrorDetected(object sender,
            CoreWebView2ServerCertificateErrorDetectedEventArgs e)
        {
            e.Action = IsCertificateValid(e)
                ? CoreWebView2ServerCertificateErrorAction.AlwaysAllow
                : CoreWebView2ServerCertificateErrorAction.Cancel;
        }

        private bool IsCertificateValid(CoreWebView2ServerCertificateErrorDetectedEventArgs e)
        {
            X509Certificate2 certificate = e.ServerCertificate.ToX509Certificate2();
            Uri requestUri = new Uri(e.RequestUri);
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