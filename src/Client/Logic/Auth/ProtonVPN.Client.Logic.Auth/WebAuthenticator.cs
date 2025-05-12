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

using System.Collections.Specialized;
using System.Web;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.Logic.Auth;

public class WebAuthenticator : IWebAuthenticator
{
    private const string UPGRADE_INTENT = "upgrade";
    private const string MANAGE_SUBSCRIPTION_INTENT = "manage-subscription";
    private const string SUBSCRIBE_ACCOUNT_ACTION = "subscribe-account";
    private const string REFRESH_ACCOUNT_REDIRECT = "refresh-account";

    private readonly IApiClient _apiClient;
    private readonly IConfiguration _config;
    private readonly ILogger _logger;

    protected string ActivationProtocol { get; }

    public WebAuthenticator(IApiClient apiClient, IConfiguration config, ILogger logger)
    {
        _apiClient = apiClient;
        _config = config;
        _logger = logger;

        ActivationProtocol = $"{_config.ProtocolActivationScheme}://";
    }

    public Task<string> GetMyAccountUrlAsync()
    {
        return Task.FromResult(_config.Urls.AccountUrl);
    }

    public async Task<string> GetUpgradeAccountUrlAsync(ModalSource modalSource, string? notificationReference = null)
    {
        string redirectUrl = GetRedirectUrl(modalSource, notificationReference);

        AuthUrlParameters parameters = new()
        {
            Action = SUBSCRIBE_ACCOUNT_ACTION,
            Fullscreen = "off",
            Redirect = redirectUrl,
            Start = "compare",
            Type = UPGRADE_INTENT,
        };
        string url = await GetAuthUrlAsync(parameters);

        return url;
    }

    public async Task<string> GetAuthUrlAsync(AuthUrlParameters parameters)
    {
        string selector = await GetSelectorAsync();
        return selector.IsNullOrEmpty()
            ? _config.Urls.AccountUrl
            : GetAutoLoginUrl(parameters, selector);
    }

    public async Task<string> GetAuthUrlAsync(string url, ModalSource modalSource, string notificationReference)
    {
        Uri uri = new(url);
        NameValueCollection uriQuery = HttpUtility.ParseQueryString(uri.Query);
        if (uriQuery.AllKeys.Contains("redirect") && uriQuery["redirect"] is not null)
        {
            string queryPrefix = uriQuery["redirect"]?.Contains('?') ?? false ? "&" : "?";
            uriQuery["redirect"] += $"{queryPrefix}modal-source={modalSource}&notification-reference={notificationReference}";
        }

        url = $"{uri.GetLeftPart(UriPartial.Path)}?{uriQuery}";

        string selector = await GetSelectorAsync();
        return selector.IsNullOrEmpty()
            ? _config.Urls.AccountUrl
            : url + $"#selector={selector}";
    }

    private string GetRedirectUrl(ModalSource modalSource, string? notificationReference = null)
    {
        string url = $"{GetRedirectUrl()}?modal-source={modalSource}";
        if (!string.IsNullOrWhiteSpace(notificationReference))
        {
            url += $"&notification-reference={notificationReference}";
        }

        return url;
    }

    private string GetRedirectUrl()
    {
        return REFRESH_ACCOUNT_REDIRECT;
    }

    private string GetAutoLoginUrl(AuthUrlParameters parameters, string selector)
    {
        return _config.Urls.AutoLoginBaseUrl + "?" +
               $"action={parameters.Action}&" +
               $"fullscreen={parameters.Fullscreen}&" +
               $"redirect={ActivationProtocol + parameters.Redirect}&" +
               $"start={parameters.Start}&" +
               $"type={parameters.Type}&" +
               "app=vpn" +
               "#selector=" + selector;
    }

    private async Task<string> GetSelectorAsync()
    {
        try
        {
            AuthForkSessionRequest request = new()
            {
                ChildClientId = "web-account-lite",
                Independent = 0,
            };
            ApiResponseResult<ForkedAuthSessionResponse> result = await _apiClient.ForkAuthSessionAsync(request);
            if (result.Success)
            {
                return result.Value.Selector;
            }
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Failed to update auto login selector.", e);
        }

        return string.Empty;
    }
}