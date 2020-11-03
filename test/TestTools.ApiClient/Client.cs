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
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Api.Data;
using File = ProtonVPN.Core.Api.File;

namespace TestTools.ApiClient
{
    public class Client : BaseApiClient, IApiClient
    {
        private readonly HttpClient _client;

        public Client(
            ILogger logger,
            HttpClient client,
            ITokenStorage tokenStorage) : base(logger, new ApiAppVersion(), tokenStorage, "3", "en")
        {
            _client = client;
        }

        public async Task<ApiResponseResult<AuthResponse>> GetAuthResponse(AuthRequestData data)
        {
            var request = GetRequest(HttpMethod.Post, "auth");
            try
            {
                request.Content = GetJsonContent(data);

                using var response = await _client.SendAsync(request).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ApiResponseResult<AuthResponse>(body, response.StatusCode);
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

                using var response = await _client.SendAsync(request).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var validatedResponse = ApiResponseResult<AuthInfo>(body, response.StatusCode);
                if (validatedResponse.Success && string.IsNullOrEmpty(validatedResponse.Value.Salt))
                {
                    return ProtonVPN.Core.Api.ApiResponseResult<AuthInfo>.Fail(response.StatusCode,
                        "Incorrect login credentials. Please try again");
                }

                return validatedResponse;
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
                return GetResponseStreamResult<ProfilesResponse>(stream, response.StatusCode);
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
                return ApiResponseResult<ProfileResponse>(body, response.StatusCode);
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public Task<ApiResponseResult<VpnInfoResponse>> GetVpnInfoResponse() => throw new NotImplementedException();

        public Task<ApiResponseResult<BaseResponse>> GetLogoutResponse() => throw new NotImplementedException();

        public Task<ApiResponseResult<EventResponse>> GetEventResponse(string lastId = default) => throw new NotImplementedException();

        public Task<ApiResponseResult<ServerList>> GetServersAsync(string ip) => throw new NotImplementedException();

        public Task<ApiResponseResult<ServerList>> GetServerLoadsAsync(string ip) => throw new NotImplementedException();

        public async Task<ApiResponseResult<UserLocation>> GetLocationDataAsync()
        {
            try
            {
                var request = GetAuthorizedRequest(HttpMethod.Get, "vpn/location");
                using var response = await _client.SendAsync(request).ConfigureAwait(false);
                var stream = await response.Content.ReadAsStreamAsync();
                return GetResponseStreamResult<UserLocation>(stream, response.StatusCode);
            }
            catch(Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public Task<ApiResponseResult<BaseResponse>> ReportBugAsync(IEnumerable<KeyValuePair<string, string>> fields, IEnumerable<File> files) => throw new NotImplementedException();

        public Task<ApiResponseResult<SessionsResponse>> GetSessions() => throw new NotImplementedException();

        public Task<ApiResponseResult<ProfileResponse>> CreateProfile(BaseProfile profile) => throw new NotImplementedException();

        public Task<ApiResponseResult<ProfileResponse>> UpdateProfile(string id, BaseProfile profile) => throw new NotImplementedException();

        public Task<ApiResponseResult<VpnConfig>> GetVpnConfig() => throw new NotImplementedException();

        public Task<ApiResponseResult<PhysicalServerResponse>> GetServerAsync(string serverId) => throw new NotImplementedException();

        public Task<ApiResponseResult<AnnouncementsResponse>> GetAnnouncementsAsync() => throw new NotImplementedException();
    }
}
