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

using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.UI.Login.Overlays;

public partial class SsoLoginOverlayViewModel :  OverlayViewModelBase<IMainWindowOverlayActivator>
{
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly ISettings _settings;
    private readonly IConfiguration _configuration;
    private readonly Regex _uriRegex = new(".+\\/sso\\/login#token=(?<token>.+)&uid=(?<uid>.+)");

    [ObservableProperty]
    private bool _isLoadingPage;

    private string? _ssoResponseToken;

    public WebView2 SsoWebView { get; }

    public SsoLoginOverlayViewModel(
        IMainWindowOverlayActivator overlayActivator,
        IUserAuthenticator userAuthenticator,
        ISettings settings,
        IConfiguration configuration,
        IViewModelHelper viewModelHelper)
        : base(overlayActivator, viewModelHelper)
    {
        _userAuthenticator = userAuthenticator;
        _settings = settings;
        _configuration = configuration;

        SsoWebView = new WebView2 { VerticalAlignment = VerticalAlignment.Stretch };

        SsoWebView.NavigationStarting += OnNavigationStarting;
        SsoWebView.NavigationCompleted += OnNavigationCompleted;
    }

    private void OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
        // Intercept redirection to the account login page and extract the response token from the Uri
        Match match = _uriRegex.Match(e.Uri);
        if (match.Success && match.Groups["uid"]?.Value == _settings.UnauthUniqueSessionId)
        {
            // Cancel navigation and extract response token
            e.Cancel = true;

            _ssoResponseToken = match.Groups["token"]?.Value;

            OverlayActivator.CloseCurrentOverlay();
        }
    }

    private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        IsLoadingPage = false;
    }

    public async Task<AuthResult> AuthenticateAsync(string ssoChallengeToken)
    {
        if (string.IsNullOrEmpty(ssoChallengeToken))
        {
            return AuthResult.Fail(AuthError.SsoAuthFailed);
        }

        _ssoResponseToken = null;
        IsLoadingPage = true;
        await InitializeWebViewAsync(ssoChallengeToken);
        await OverlayActivator.ShowOverlayAsync(this);
        return await _userAuthenticator.CompleteSsoAuthAsync(_ssoResponseToken ?? string.Empty);
    }

    private async Task InitializeWebViewAsync(string ssoChallengeToken)
    {
        try
        {
            if (SsoWebView.CoreWebView2 == null)
            {
                CoreWebView2Environment environment = await CoreWebView2Environment.CreateWithOptionsAsync(null, _configuration.WebViewFolder, null);

                // WinUI3 does not support creating CoreWebView2 with custom environment. Set environment variable instead.
                Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", environment.UserDataFolder);

                await SsoWebView.EnsureCoreWebView2Async();

                SsoWebView.CoreWebView2!.Settings.IsWebMessageEnabled = true;
            }

            // Delete cookies to prevent auto authentication after a first successful login.
            SsoWebView.CoreWebView2.CookieManager.DeleteAllCookies();

            Uri requestUri = new(new Uri(_configuration.Urls.ApiUrl), $"auth/sso/{ssoChallengeToken}");

            CoreWebView2WebResourceRequest request = SsoWebView.CoreWebView2.Environment.CreateWebResourceRequest(
                requestUri.AbsoluteUri,
                "GET",
                null,
                $"x-pm-uid: {_settings.UnauthUniqueSessionId}\r\n" +
                $"Authorization: Bearer {_settings.UnauthAccessToken}");

            SsoWebView.CoreWebView2.NavigateWithWebResourceRequest(request);
        }
        catch (Exception e)
        {
            Logger.Error<AppLog>($"Error occured when trying to navigate to the SSO login page.", e);
        }
    }
}