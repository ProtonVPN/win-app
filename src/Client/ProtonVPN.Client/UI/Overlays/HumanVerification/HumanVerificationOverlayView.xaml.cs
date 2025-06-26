/*
 * Copyright (c) 2025 Proton AG
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

using System.Security.Cryptography.X509Certificates;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.HumanVerification;
using ProtonVPN.Api.Handlers.TlsPinning;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Client.Core.Bases;

namespace ProtonVPN.Client.UI.Overlays.HumanVerification;

public sealed partial class HumanVerificationOverlayView : IContextAware
{
    private const string CSP_HEADER = "Content-Security-Policy";
    private const int WEBVIEW_ADDED_HEIGHT = 124;

    private readonly ILogger _logger;
    private readonly IHumanVerificationConfig _humanVerificationConfig;
    private readonly IConfiguration _config;
    private readonly ICertificateValidator _certificateValidator;
    private readonly IApiHostProvider _apiHostProvider;
    private readonly HttpClient _httpClient;
    public HumanVerificationOverlayViewModel ViewModel { get; }

    public string UserDataFolder { get; }

    public HumanVerificationOverlayView()
    {
        _logger = App.GetService<ILogger>();
        _humanVerificationConfig = App.GetService<IHumanVerificationConfig>();
        _config = App.GetService<IConfiguration>();
        _certificateValidator = App.GetService<ICertificateValidator>();
        _apiHostProvider = App.GetService<IApiHostProvider>();
        _httpClient = App.GetService<IHumanVerificationHttpClientFactory>().GetHttpClient();

        ViewModel = App.GetService<HumanVerificationOverlayViewModel>();
        UserDataFolder = App.GetService<IConfiguration>().WebViewFolder;

        InitializeComponent();
        InitializeWebView();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private static X509Certificate2 ConvertToX509Certificate2(CoreWebView2Certificate certificate)
    {
        X509Certificate2 x509Certificate = new(Convert.FromBase64String(certificate.ToPemEncoding().Replace("-----BEGIN CERTIFICATE-----", string.Empty).Replace("-----END CERTIFICATE-----", string.Empty)))
        {
            FriendlyName = certificate.DisplayName
        };

        return x509Certificate;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Activate();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        CleanupWebView();
        ViewModel.Deactivate();
    }

    private void CleanupWebView()
    {
        try
        {
            if (WebView2 is null)
            {
                return;
            }

            WebView2.CoreWebView2Initialized -= OnCoreWebView2InitializedAsync;
            WebView2.WebMessageReceived -= WebView2_WebMessageReceived;

            if (WebView2?.CoreWebView2 != null)
            {
                WebView2.CoreWebView2.ServerCertificateErrorDetected -= OnServerCertificateErrorDetected;
                WebView2.CoreWebView2.WebResourceRequested -= CoreWebView2_WebResourceRequested;
            }
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("Failed to cleanup WebView2 event handlers.", ex);
        }
    }

    private void InitializeWebView()
    {
        try
        {
            if (_humanVerificationConfig.IsSupported())
            {
                Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", UserDataFolder);
                WebView2.CoreWebView2Initialized += OnCoreWebView2InitializedAsync;
            }
        }
        catch (Exception e)
        {
            LogWebViewInitializationException(e);
        }
    }

    private async void OnCoreWebView2InitializedAsync(WebView2 webView2, CoreWebView2InitializedEventArgs args)
    {
        try
        {
            await WebView2.CoreWebView2.ClearServerCertificateErrorActionsAsync();
            WebView2.CoreWebView2.ServerCertificateErrorDetected += OnServerCertificateErrorDetected;
            WebView2.WebMessageReceived += WebView2_WebMessageReceived;

            WebView2.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            WebView2.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("Failed to initialize CoreWebView2 server certificate handler.", ex);
        }
    }

    private void CoreWebView2_WebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs args)
    {
        HttpRequestMessage request = new(new HttpMethod(args.Request.Method), args.Request.Uri);
        HttpContent? content = null;

        foreach (KeyValuePair<string, string> pair in args.Request.Headers)
        {
            if (IsContentHeader(pair.Key))
            {
                content ??= new ByteArrayContent(Array.Empty<byte>());
                content.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }
            else
            {
                request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }
        }

        if (content != null)
        { 
            request.Content = content; 
        }

        if (_apiHostProvider.IsProxyActive())
        {
            request.Headers.Add("X-PM-DoH-Host", _config.DoHVerifyApiHost);
        }

        try
        {
            HttpResponseMessage response = _httpClient.Send(request);
            args.Response = ModifyResponseMessage(response);
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
                response.Content.ReadAsStream().AsRandomAccessStream(),
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

    private void WebView2_WebMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        try
        {
            CaptchaMessage? message = JsonConvert.DeserializeObject<CaptchaMessage>(args.WebMessageAsJson);
            switch (message?.Type)
            {
                case CaptchaMessageTypes.Height:
                    WebView2.Height = message.Height + WEBVIEW_ADDED_HEIGHT;
                    break;

                case CaptchaMessageTypes.TokenResponse:
                    ViewModel.TriggerVerificationTokenMessage(message.Token);
                    break;
            }
        }
        catch (JsonException ex)
        {
            _logger.Error<AppLog>("Failed to deserialize webview message.", ex);
        }
    }

    private void LogWebViewInitializationException(Exception e)
    {
        _logger.Error<AppLog>("Failed to initialize CoreWebView2.", e);
    }

    private void OnServerCertificateErrorDetected(object sender, CoreWebView2ServerCertificateErrorDetectedEventArgs e)
    {
        try
        {
            if (e.ErrorStatus == CoreWebView2WebErrorStatus.CertificateIsInvalid && IsCertificateValid(e))
            {
                e.Action = CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
            }
            else
            {
                e.Action = CoreWebView2ServerCertificateErrorAction.Cancel;
                _logger.Error<AppLog>($"Failed to validate server certificate in a WebView. Reason: ${e.ErrorStatus}");
            }
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("Failed to validate server certificate in a WebView.", ex);
        }
    }

    private bool IsCertificateValid(CoreWebView2ServerCertificateErrorDetectedEventArgs e)
    {
        X509Certificate2 certificate = ConvertToX509Certificate2(e.ServerCertificate);
        Uri requestUri = new(e.RequestUri);
        CertificateValidationParams validationParams = new()
        {
            Certificate = certificate,
            Chain = e.ServerCertificate.PemEncodedIssuerCertificateChain,
            HasSslError = _config.IsCertificateValidationEnabled,
            Host = requestUri.Host,
            RequestUri = requestUri,
        };

        return _certificateValidator.IsValid(validationParams);
    }

    private static bool IsContentHeader(string header)
    {
        return header.StartsWith("Content-", StringComparison.OrdinalIgnoreCase);
    }
}