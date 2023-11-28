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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Core.Auth;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Modals;

namespace ProtonVPN.Login.ViewModels;

public class SsoLoginViewModel : BaseModalViewModel
{
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private bool _isLoadingPage;

    private AuthSsoSessionInfo _currentSessionInfo;

    private readonly Regex _uriRegex = new(".+\\/sso\\/login#token=(?<token>.+)&uid=(?<uid>.+)");

    public bool IsLoadingPage
    {
        get => _isLoadingPage;
        set => Set(ref _isLoadingPage, value);
    }

    public string ResponseToken { get; private set; }

    public WebView2 SsoWebView { get; }

    public SsoLoginViewModel(IConfiguration config, ILogger logger)
    {
        _config = config;
        _logger = logger;

        SsoWebView = new WebView2();

        SsoWebView.NavigationStarting += OnNavigationStarting;
        SsoWebView.NavigationCompleted += OnNavigationCompleted;
    }

    public override void BeforeOpenModal(dynamic options)
    {
        _currentSessionInfo = null;
        ResponseToken = string.Empty;

        if (options is not AuthSsoSessionInfo sessionInfo)
        {
            CloseAction();
            return;
        }

        IsLoadingPage = true;

        _currentSessionInfo = sessionInfo;

        InitializeWebViewAsync();
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            if (SsoWebView.CoreWebView2 == null)
            {
                CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, _config.LocalAppDataFolder);
                await SsoWebView.EnsureCoreWebView2Async(environment);

                SsoWebView.CoreWebView2.Settings.IsWebMessageEnabled = true;
            }

            // Delete cookies to prevent auto authentication after a first successful login. (commented until further notice from account team)
            // SsoWebView.CoreWebView2.CookieManager.DeleteAllCookies();

            Uri requestUri = new Uri(new Uri(_config.Urls.ApiUrl), $"auth/sso/{_currentSessionInfo.SsoChallengeToken}");

            CoreWebView2WebResourceRequest request = SsoWebView.CoreWebView2.Environment.CreateWebResourceRequest(
                requestUri.AbsoluteUri,
                "GET",
                null,
                $"x-pm-uid: {_currentSessionInfo.UnauthSessionUid}\r\n" +
                $"Authorization: Bearer {_currentSessionInfo.UnauthSessionAccessToken}");

            SsoWebView.CoreWebView2.NavigateWithWebResourceRequest(request);
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>($"Error occured when trying to navigate to the SSO login page.", e);
        }
    }

    private void OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
        // Intercept redirection to the account login page and extract the response token from the Uri
        Match match = _uriRegex.Match(e.Uri);
        if (match.Success && match.Groups["uid"]?.Value == _currentSessionInfo.UnauthSessionUid)
        {
            // Cancel navigation and extract response token
            e.Cancel = true;

            ResponseToken = match.Groups["token"]?.Value;
            CloseAction();
        }
    }

    private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        IsLoadingPage = false;
    }
}