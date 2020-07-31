/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System.IO;
using Newtonsoft.Json;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api.Contracts;
using System.Net;
using System.Net.Http;
using System.Text;
using ProtonVPN.Common.Logging;

namespace ProtonVPN.Core.Api
{
    public class BaseApiClient
    {
        private readonly JsonSerializer _jsonSerializer = new JsonSerializer();
        private readonly IApiAppVersion _appVersion;
        protected readonly ITokenStorage TokenStorage;
        private readonly string _apiVersion;
        private readonly string _locale;
        private readonly ILogger _logger;

        public BaseApiClient(
            ILogger logger,
            IApiAppVersion appVersion,
            ITokenStorage tokenStorage,
            string apiVersion,
            string locale)
        {
            _logger = logger;
            _apiVersion = apiVersion;
            TokenStorage = tokenStorage;
            _appVersion = appVersion;
            _locale = locale;
        }

        protected StringContent GetJsonContent(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        protected ApiResponseResult<T> ApiResponseResult<T>(string body, HttpStatusCode code) where T : BaseResponse
        {
            try
            {
                var response = JsonConvert.DeserializeObject<T>(body);
                if (response == null)
                {
                    throw new HttpRequestException(string.Empty);
                }

                return response.Code == ResponseCodes.OkResponse
                    ? Api.ApiResponseResult<T>.Ok(response)
                    : Api.ApiResponseResult<T>.Fail(response, code, response.Error);
            }
            catch (JsonException)
            {
                throw new HttpRequestException(code.Description());
            }
        }

        protected HttpRequestMessage GetRequest(HttpMethod method, string requestUri)
        {
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Add("x-pm-apiversion", _apiVersion);
            request.Headers.Add("x-pm-appversion", _appVersion.Value());
            request.Headers.Add("x-pm-locale", _locale);
            request.Headers.Add("User-Agent", _appVersion.UserAgent());

            return request;
        }

        protected HttpRequestMessage GetAuthorizedRequest(HttpMethod method, string requestUri)
        {
            var request = GetRequest(method, requestUri);
            request.Headers.Add("x-pm-uid", TokenStorage.Uid);
            request.Headers.Add("Authorization", $"Bearer {TokenStorage.AccessToken}");

            return request;
        }

        protected ApiResponseResult<T> GetResponseStreamResult<T>(Stream stream, HttpStatusCode code) where T : BaseResponse
        {
            using var streamReader = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(streamReader);

            var response = _jsonSerializer.Deserialize<T>(jsonTextReader);
            if (response == null)
            {
                throw new HttpRequestException(string.Empty);
            }

            if (response.Code != ResponseCodes.OkResponse)
            {
                return Api.ApiResponseResult<T>.Fail(code, response.Error);
            }

            return Api.ApiResponseResult<T>.Ok(response);
        }

        protected ApiResponseResult<T> Logged<T>(ApiResponseResult<T> result, string message = null) where T : BaseResponse
        {
            if (result.Failure)
                _logger.Error($"API: {(!string.IsNullOrEmpty(message) ? message : "Request")} failed: {result.Error}");

            return result;
        }
    }
}
