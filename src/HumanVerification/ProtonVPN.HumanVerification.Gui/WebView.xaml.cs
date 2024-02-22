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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Web.WebView2.Core;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Handlers.TlsPinning;
using ProtonVPN.Common.Configuration;
using ProtonVPN.HumanVerification.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.HumanVerification.Gui
{
    public partial class WebView
    {
        private const string CSP_HEADER = "Content-Security-Policy";

        private readonly IConfiguration _config;
        private readonly IApiHostProvider _apiHostProvider;
        private readonly ICertificateValidator _certificateValidator;
        private readonly IHumanVerificationConfig _humanVerificationConfig;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public WebView(
            IConfiguration config,
            IApiHostProvider apiHostProvider,
            ICertificateValidator certificateValidator,
            IHumanVerificationConfig humanVerificationConfig,
            ILogger logger,
            TlsPinnedCertificateHandler tlsPinnedCertificateHandler)
        {
            _config = config;
            _apiHostProvider = apiHostProvider;
            _certificateValidator = certificateValidator;
            _humanVerificationConfig = humanVerificationConfig;
            _logger = logger;
            _httpClient = new HttpClient(tlsPinnedCertificateHandler);

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

                    WebView2.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                    WebView2.CoreWebView2.WebResourceRequested += OnWebResourceRequested;
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

        private void OnWebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            HttpRequestMessage request = new(new HttpMethod(e.Request.Method), e.Request.Uri);
            foreach (KeyValuePair<string, string> pair in e.Request.Headers)
            {
                request.Headers.Add(pair.Key, pair.Value);
            }

            if (_apiHostProvider.IsProxyActive())
            {
                request.Headers.Add("X-PM-DoH-Host", _config.DoHVerifyApiHost);
            }

            try
            {
                HttpResponseMessage response = _httpClient.Send(request);
                e.Response = ModifyResponseMessage(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.Error<AppLog>("Failed to fetch webview resource.", ex);
            }
        }

        private CoreWebView2WebResourceResponse ModifyResponseMessage(HttpResponseMessage response)
        {
            CoreWebView2WebResourceResponse modifiedResponse =
                WebView2.CoreWebView2.Environment.CreateWebResourceResponse(
                    response.Content.ReadAsStream(),
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    string.Empty);

            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = response.Headers
                .Where(h => h.Key != CSP_HEADER)
                .Concat(response.Content.Headers);

            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                modifiedResponse.Headers.AppendHeader(header.Key, string.Join(",", header.Value));
            }

            return modifiedResponse;
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