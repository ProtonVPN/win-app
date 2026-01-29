/*
 * Copyright (c) 2026 Proton AG
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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;
using TimeZoneConverter;

namespace ProtonVPN.Api;

public class BaseApiClient : IClientBase
{
    protected ILogger Logger { get; }
    protected ISettings Settings { get; }
    protected IConfiguration Config { get; }

    private readonly JsonSerializer _jsonSerializer = new();
    private readonly IApiAppVersion _appVersion;
    private readonly string _apiVersion;

    public event EventHandler<ActionableFailureApiResultEventArgs> OnActionableFailureResult;

    public BaseApiClient(
        ILogger logger,
        IApiAppVersion appVersion,
        ISettings settings,
        IConfiguration config)
    {
        Logger = logger;
        Settings = settings;
        Config = config;

        _appVersion = appVersion;
        _apiVersion = config.ApiVersion;
    }

    protected StringContent GetJsonContent(object data)
    {
        string json = JsonConvert.SerializeObject(data);
        return new(json, Encoding.UTF8, "application/json");
    }

    protected async Task<ApiResponseResult<T>> GetApiResponseResultAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        if (typeof(T) == typeof(byte[]))
        {
            if (response.StatusCode is HttpStatusCode.NotModified)
            {
                return (ApiResponseResult<T>)(object)ApiResponseResult<byte[]>.NotModified(response);
            }

            if (response.IsSuccessStatusCode)
            {
                byte[] body = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                return (ApiResponseResult<T>)(object)ApiResponseResult<byte[]>.Ok(response, body);
            }
            else
            {
                return (ApiResponseResult<T>)(object)ApiResponseResult<byte[]>.Fail(response, GetStatusCodeDescription(response.StatusCode));
            }
        }

        if (typeof(T).IsAssignableTo(typeof(BaseResponse)))
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                T json = response.StatusCode is HttpStatusCode.NotModified
                    ? default(T)
                    : JsonConvert.DeserializeObject<T>(body) ?? throw new HttpRequestException(string.Empty);

                ApiResponseResult<T> result = CreateApiResponseResult(json, response);
                HandleResult(result, response);
                return result;
            }
            catch (JsonException)
            {
                throw new HttpRequestException(GetStatusCodeDescription(response.StatusCode));
            }
        }

        throw new NotSupportedException($"Type {typeof(T).Name} is not supported. Only BaseResponse types and byte[] are supported.");
    }

    public string GetStatusCodeDescription(HttpStatusCode code)
    {
        string description = ReasonPhrases.GetReasonPhrase((int)code);
        return string.IsNullOrEmpty(description) ? $"HTTP error code: {code}." : description;
    }

    private ApiResponseResult<T> CreateApiResponseResult<T>(T response, HttpResponseMessage responseMessage)
    {
        if (responseMessage.StatusCode is HttpStatusCode.NotModified)
        {
            return ApiResponseResult<T>.NotModified(responseMessage);
        }

        if (response is BaseResponse baseResponse)
        {
            return baseResponse.Code switch
            {
                ResponseCodes.OK_RESPONSE => ApiResponseResult<T>.Ok(responseMessage, response),
                _ => ApiResponseResult<T>.Fail(response, responseMessage, baseResponse.Error),
            };
        }

        return ApiResponseResult<T>.Ok(responseMessage, response);
    }

    private void HandleResult<T>(ApiResponseResult<T> result, HttpResponseMessage responseMessage)
    {
        if (result.Failure && !result.Actions.IsNullOrEmpty())
        {
            HandleActionableFailureResult(result, responseMessage);
        }
    }

    private void HandleActionableFailureResult<T>(ApiResponseResult<T> result, HttpResponseMessage responseMessage)
    {
        if (result.Value is BaseResponse baseResponse)
        {
            ApiResponseResult<BaseResponse> baseResponseResult = CreateApiResponseResult(baseResponse, responseMessage);
            ActionableFailureApiResultEventArgs eventArgs = new(baseResponseResult);
            OnActionableFailureResult?.Invoke(this, eventArgs);
        }
    }

    protected HttpRequestMessage GetRequest(HttpMethod method, string requestUri)
    {
        string accessToken = string.IsNullOrEmpty(Settings.AccessToken)
            ? Settings.UnauthAccessToken
            : Settings.AccessToken;
        string uniqueSessionId = string.IsNullOrEmpty(Settings.UniqueSessionId)
            ? Settings.UnauthUniqueSessionId
            : Settings.UniqueSessionId;

        return !string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(uniqueSessionId)
            ? GetAuthorizedRequest(method, requestUri, accessToken, uniqueSessionId)
            : GetUnauthorizedRequest(method, requestUri);
    }

    protected HttpRequestMessage GetAuthorizedRequest(HttpMethod method, string requestUri)
    {
        return GetAuthorizedRequest(method, requestUri, Settings.AccessToken, Settings.UniqueSessionId);
    }

    protected HttpRequestMessage GetAuthorizedRequest(HttpMethod method, string requestUri, string accessToken,
        string uniqueSessionId)
    {
        HttpRequestMessage request = GetUnauthorizedRequest(method, requestUri);
        request.Headers.Add("x-pm-uid", uniqueSessionId);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        return request;
    }

    protected HttpRequestMessage GetUnauthorizedRequest(HttpMethod method, string requestUri)
    {
        HttpRequestMessage request = new(method, requestUri);
        request.Headers.Add("x-pm-apiversion", _apiVersion);
        request.Headers.Add("x-pm-appversion", _appVersion.AppVersion);
        request.Headers.Add("x-pm-locale", Settings.Language);
        request.Headers.Add("User-Agent", _appVersion.UserAgent);

        try
        {
            request.Headers.Add("x-pm-timezone", TZConvert.WindowsToIana(TimeZoneInfo.Local.Id));
        }
        catch (Exception e)
        {
            Logger.Error<ApiLog>("Failed to set x-pm-timezone header", e);
        }

        return request;
    }

    protected HttpRequestMessage GetAuthorizedRequestWithLocation(HttpMethod method, string requestUri,
        DeviceLocation? deviceLocation)
    {
        HttpRequestMessage request = GetAuthorizedRequest(method, requestUri);

        if (!string.IsNullOrEmpty(deviceLocation?.CountryCode))
        {
            request.Headers.Add("x-pm-country", deviceLocation.Value.CountryCode);
        }
        if (!string.IsNullOrEmpty(deviceLocation?.IpAddress))
        {
            request.Headers.Add("x-pm-netzone", deviceLocation.Value.IpAddress);
        }

        return request;
    }

    protected ApiResponseResult<T> Logged<T>(ApiResponseResult<T> result, string message = null)
    {
        if (result.Failure)
        {
            Logger.Error<ApiErrorLog>($"API: {(string.IsNullOrEmpty(message) ? "Request" : message)} failed: {result.Error}");
        }

        return result;
    }
}