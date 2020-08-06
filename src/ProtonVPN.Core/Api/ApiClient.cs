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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Api.Data;
using UserLocation = ProtonVPN.Core.Api.Contracts.UserLocation;

namespace ProtonVPN.Core.Api
{
    public class ApiClient : BaseApiClient, IApiClient
    {
        private readonly HttpClient _client;
        private readonly HttpClient _noCacheClient;
        private readonly ILogger _logger;

        private readonly JsonSerializer _jsonSerializer = new JsonSerializer();

        public ApiClient(
            HttpClient client,
            HttpClient noCacheClient,
            ILogger logger,
            ITokenStorage tokenStorage,
            IApiAppVersion appVersion,
            string apiVersion,
            string locale) : base(appVersion, tokenStorage, apiVersion, locale)
        {
            _logger = logger;
            _client = client;
            _noCacheClient = noCacheClient;
        }

        public async Task<ApiResponseResult<AuthResponse>> GetAuthResponse(AuthRequestData data)
        {
            var request = GetRequest(HttpMethod.Post, "auth");
            try
            {
                request.Content = GetJsonContent(data);

                using (var response = await _client.SendAsync(request).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return Logged(ApiResponseResult<AuthResponse>(body, response.StatusCode));
                }
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<AuthInfo>> GetAuthInfoResponse(AuthInfoRequestData data)
        {
            var request = GetRequest(HttpMethod.Post, "auth/info");
            try
            {
                request.Content = GetJsonContent(data);

                using (var response = await _client.SendAsync(request).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var validatedResponse = ApiResponseResult<AuthInfo>(body, response.StatusCode);
                    if (validatedResponse.Success && string.IsNullOrEmpty(validatedResponse.Value.Salt))
                    {
                        return Logged(Api.ApiResponseResult<AuthInfo>.Fail(response.StatusCode,
                            "Incorrect login credentials. Please try again"));
                    }
                    return Logged(validatedResponse);
                }
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<VpnInfoResponse>> GetVpnInfoResponse()
        {
            var request = GetAuthorizedRequest(HttpMethod.Get, "vpn");
            try
            {
                using (var response = await _client.SendAsync(request).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return ApiResponseResult<VpnInfoResponse>(body, response.StatusCode);
                }
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<BaseResponse>> GetLogoutResponse()
        {
            var request = GetAuthorizedRequest(HttpMethod.Delete, "auth");

            try
            {
                using (var response = await _client.SendAsync(request).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return Logged(ApiResponseResult<BaseResponse>(body, response.StatusCode), "Logout");
                }
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<EventResponse>> GetEventResponse(string lastId)
        {
            var id = string.IsNullOrEmpty(lastId) ? "latest" : lastId;
            var request = GetAuthorizedRequest(HttpMethod.Get, "events/" + id);

            try
            {
                using (var response = await _client.SendAsync(request).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return ApiResponseResult<EventResponse>(body, response.StatusCode);
                }
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<ServerList>> GetServersAsync(string ip)
        {
            try
            {
                var uri = "vpn/logicals";
                if (!string.IsNullOrEmpty(ip))
                {
                    uri += $"?IP={ip}";
                }

                var request = GetAuthorizedRequest(HttpMethod.Get, uri);
                using var response = await _client.SendAsync(request).ConfigureAwait(false);
                var stream = await response.Content.ReadAsStreamAsync();
                return Logged(GetResponseStreamResult<ServerList>(stream, response.StatusCode), "Get servers");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<UserLocation>> GetLocationDataAsync()
        {
            try
            {
                var request = GetRequest(HttpMethod.Get, "vpn/location");
                using var response = await _noCacheClient.SendAsync(request).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<UserLocation>(body, response.StatusCode), "Get location data");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<BaseResponse>> ReportBugAsync(IEnumerable<KeyValuePair<string, string>> fields, IEnumerable<File> files)
        {
            var content = new MultipartFormDataContent();

            foreach (var pair in fields)
            {
                content.Add(new StringContent(pair.Value), $"\"{pair.Key}\"");
            }

            var fileCount = 0;
            foreach (var file in files)
            {
                content.Add(new ByteArrayContent(file.Content),
                    $"\"File{fileCount}\"",
                    $"\"{file.Name}\"");
                fileCount++;
            }

            try
            {
                var request = GetAuthorizedRequest(HttpMethod.Post, "reports/bug");
                request.Content = content;
                using var response = await _client.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<BaseResponse>(body, response.StatusCode), "Report bug");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<SessionsResponse>> GetSessions()
        {
            try
            {
                var request = GetAuthorizedRequest(HttpMethod.Get, "vpn/sessions");
                using var response = await _client.SendAsync(request).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<SessionsResponse>(body, response.StatusCode), "Get sessions");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<ProfilesResponse>> GetProfiles()
        {
            try
            {
                var request = GetAuthorizedRequest(HttpMethod.Get, "vpn/profiles");
                using var response = await _client.SendAsync(request).ConfigureAwait(false);
                var stream = await response.Content.ReadAsStreamAsync();
                return Logged(GetResponseStreamResult<ProfilesResponse>(stream, response.StatusCode), "Get profiles");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<ProfileResponse>> CreateProfile(BaseProfile profile)
        {
            try
            {
                var request = GetAuthorizedRequest(HttpMethod.Post, "vpn/profiles");
                request.Content = GetJsonContent(profile);
                using var response = await _client.SendAsync(request).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<ProfileResponse>(body, response.StatusCode), "Create profile");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<ProfileResponse>> UpdateProfile(string id, BaseProfile profile)
        {
            try
            {
                var request = GetAuthorizedRequest(HttpMethod.Put, $"vpn/profiles/{id}");
                request.Content = GetJsonContent(profile);
                using var response = await _client.SendAsync(request).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<ProfileResponse>(body, response.StatusCode), "Update profile");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<ProfileResponse>> DeleteProfile(string id)
        {
            try
            {
                var request = GetAuthorizedRequest(HttpMethod.Delete, $"vpn/profiles/{id}");
                using var response = await _client.SendAsync(request).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<ProfileResponse>(body, response.StatusCode), "Delete profile");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<VpnConfig>> GetVpnConfig()
        {
            try
            {
                var request = GetAuthorizedRequest(HttpMethod.Get, "vpn/clientconfig");
                using var response = await _client.SendAsync(request).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<VpnConfig>(body, response.StatusCode), "Get VPN config");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        private ApiResponseResult<T> GetResponseStreamResult<T>(Stream stream, HttpStatusCode code) where T : BaseResponse
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
