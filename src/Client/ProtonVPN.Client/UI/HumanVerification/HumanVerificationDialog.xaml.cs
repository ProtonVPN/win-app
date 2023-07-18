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

using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using ProtonVPN.Api.Contracts.HumanVerification;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.UI.HumanVerification;

public sealed partial class HumanVerificationDialog
{
    private readonly ILogger _logger;
    private readonly IHumanVerificationConfig _humanVerificationConfig;

    public HumanVerificationDialog()
    {
        _logger = App.GetService<ILogger>();
        _humanVerificationConfig = App.GetService<IHumanVerificationConfig>();

        ViewModel = App.GetService<HumanVerificationViewModel>();

        InitializeComponent();
        InitializeWebView();
    }

    public HumanVerificationViewModel ViewModel { get; }

    // TODO: move to configuration
    public string UserDataFolder { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Proton", "Proton VPN", "WebView2");

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
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("Failed to initialize CoreWebView2 server certificate handler.", ex);
        }
    }

    private void WebView2_WebMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        try
        {
            CaptchaMessage? message = JsonConvert.DeserializeObject<CaptchaMessage>(args.WebMessageAsJson);
            switch (message?.Type)
            {
                case CaptchaMessageTypes.Height:
                    WebView2.Height = message.Height;
                    break;
                case CaptchaMessageTypes.TokenResponse:
                    ViewModel.TriggerVerificationTokenMessageCommand.Execute(message.Token);
                    break;
            }
        }
        catch (JsonException ex)
        {
            _logger.Error<AppLog>("Failed to deserialize webview message.", ex);
        }
    }

    private void OnServerCertificateErrorDetected(CoreWebView2 sender,
        CoreWebView2ServerCertificateErrorDetectedEventArgs e)
    {
        e.Action = CoreWebView2ServerCertificateErrorAction.Cancel;
        _logger.Error<AppLog>($"Failed to validate server certificate in a WebView. Reason: ${e.ErrorStatus}");
    }

    private void LogWebViewInitializationException(Exception e)
    {
        _logger.Error<AppLog>("Failed to initialize CoreWebView2.", e);
    }
}