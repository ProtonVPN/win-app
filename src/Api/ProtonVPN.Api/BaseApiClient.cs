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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Api
{
    public class BaseApiClient : IClientBase
    {
        protected ILogger Logger { get; }
        protected IAppSettings AppSettings { get; }

        private readonly JsonSerializer _jsonSerializer = new();
        private readonly IApiAppVersion _appVersion;
        private readonly string _apiVersion;
        private readonly IAppLanguageCache _appLanguageCache;

        public event EventHandler<ActionableFailureApiResultEventArgs> OnActionableFailureResult;

        public BaseApiClient(
            ILogger logger,
            IApiAppVersion appVersion,
            IAppSettings appSettings,
            IAppLanguageCache appLanguageCache,
            IConfiguration config)
        {
            Logger = logger;
            AppSettings = appSettings;
            _appVersion = appVersion;
            _apiVersion = config.ApiVersion;
            _appLanguageCache = appLanguageCache;
        }

        protected StringContent GetJsonContent(object data)
        {
            string json = JsonConvert.SerializeObject(data);
            return new(json, Encoding.UTF8, "application/json");
        }

        protected async Task<ApiResponseResult<T>> GetApiResponseResultAsync<T>(HttpResponseMessage response)
            where T : BaseResponse
        {
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                T json = JsonConvert.DeserializeObject<T>(body) ?? throw new HttpRequestException(string.Empty);
                ApiResponseResult<T> result = CreateApiResponseResult(json, response);
                HandleResult(result, response);
                return result;
            }
            catch (JsonException)
            {
                throw new HttpRequestException(GetStatusCodeDescription(response.StatusCode));
            }
        }

        public string GetStatusCodeDescription(HttpStatusCode code)
        {
            string description = ReasonPhrases.GetReasonPhrase((int)code);
            return string.IsNullOrEmpty(description) ? $"HTTP error code: {code}." : description;
        }

        private ApiResponseResult<T> CreateApiResponseResult<T>(T response, HttpResponseMessage responseMessage)
            where T : BaseResponse
        {
            switch (response.Code)
            {
                case ResponseCodes.OkResponse:
                    return ApiResponseResult<T>.Ok(responseMessage, response);
                default:
                    string method = responseMessage.RequestMessage?.Method.ToString();
                    string message = $"{method} {responseMessage.RequestMessage?.RequestUri} responded with {response.Code} code.";
                    Logger.Info<ApiResponseLog>(message);
                    return ApiResponseResult<T>.Fail(response, responseMessage, response.Error);
            }
        }

        private void HandleResult<T>(ApiResponseResult<T> result, HttpResponseMessage responseMessage)
            where T : BaseResponse
        {
            if (result.Failure && !result.Actions.IsNullOrEmpty())
            {
                HandleActionableFailureResult(result, responseMessage);
            }
        }

        private void HandleActionableFailureResult<T>(ApiResponseResult<T> result, HttpResponseMessage responseMessage)
            where T : BaseResponse
        {
            ApiResponseResult<BaseResponse> baseResponseResult =
                CreateApiResponseResult<BaseResponse>(result.Value, responseMessage);
            ActionableFailureApiResultEventArgs eventArgs = new(baseResponseResult);
            OnActionableFailureResult?.Invoke(this, eventArgs);
        }

        protected HttpRequestMessage GetRequest(HttpMethod method, string requestUri)
        {
            HttpRequestMessage request = new(method, requestUri);
            request.Headers.Add("x-pm-apiversion", _apiVersion);
            request.Headers.Add("x-pm-appversion", _appVersion.Value());
            request.Headers.Add("x-pm-locale", _appLanguageCache.GetCurrentSelectedLanguageIetfTag());
            request.Headers.Add("User-Agent", _appVersion.UserAgent());

            return request;
        }

        protected HttpRequestMessage GetAuthorizedRequest(HttpMethod method, string requestUri)
        {
            return GetAuthorizedRequest(method, requestUri, AppSettings.AccessToken, AppSettings.Uid);
        }

        protected HttpRequestMessage GetAuthorizedRequest(HttpMethod method, string requestUri, string accessToken, string uid)
        {
            HttpRequestMessage request = GetRequest(method, requestUri);
            request.Headers.Add("x-pm-uid", uid);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            return request;
        }

        protected HttpRequestMessage GetAuthorizedRequest(HttpMethod method, string requestUri, string ip)
        {
            HttpRequestMessage request = GetAuthorizedRequest(method, requestUri);
            if (!ip.IsNullOrEmpty())
            {
                request.Headers.Add("x-pm-netzone", ip);
            }

            return request;
        }

        protected ApiResponseResult<T> Logged<T>(ApiResponseResult<T> result, string message = null) where T : BaseResponse
        {
            if (result.Failure)
            {
                Logger.Error<ApiErrorLog>($"API: {(message.IsNullOrEmpty() ? "Request" : message)} failed: {result.Error}");
            }

            return result;
        }
    }
}