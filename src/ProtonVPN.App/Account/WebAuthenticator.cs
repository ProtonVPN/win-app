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
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;

namespace ProtonVPN.Account
{
    public class WebAuthenticator : IWebAuthenticator
    {
        public const string CUSTOM_PROTOCOL_PREFIX = "protonvpn://";

        private readonly IApiClient _apiClient;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public WebAuthenticator(IApiClient apiClient, IConfiguration config, ILogger logger)
        {
            _apiClient = apiClient;
            _config = config;
            _logger = logger;
        }

        public async Task<string> GetLoginUrlAsync(LoginUrlParams urlParams)
        {
            string selector = await GetSelector();
            return selector.IsNullOrEmpty() ? _config.Urls.AccountUrl : GetAutoLoginUrl(urlParams, selector);
        }

        public async Task<string> GetLoginUrlAsync(string url)
        {
            string selector = await GetSelector();
            return selector.IsNullOrEmpty() ? _config.Urls.AccountUrl : url + $"#selector={selector}";
        }

        private string GetAutoLoginUrl(LoginUrlParams urlParams, string selector)
        {
            return _config.AutoLoginBaseUrl + "?" +
                   $"action={urlParams.Action}&" +
                   $"fullscreen={urlParams.Fullscreen}&" +
                   $"redirect={CUSTOM_PROTOCOL_PREFIX + urlParams.Redirect}&" +
                   $"start={urlParams.Start}&" +
                   $"type={urlParams.Type}" +
                   "#selector=" + selector;
        }

        private async Task<string> GetSelector()
        {
            try
            {
                AuthForkSessionRequest request = new() { ChildClientId = "web-account-lite", Independent = 0, };
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

            return null;
        }
    }
}