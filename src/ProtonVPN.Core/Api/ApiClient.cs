/*
 * Copyright (c) 2022 Proton Technologies AG
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
using ProtonVPN.Core.Api.Certificates;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Api.Contracts.ReportAnIssue;
using ProtonVPN.Core.Api.Data;
using ProtonVPN.Core.Settings;
using UserLocation = ProtonVPN.Core.Api.Contracts.UserLocation;

namespace ProtonVPN.Core.Api
{
    public class ApiClient : BaseApiClient, IApiClient
    {
        private readonly HttpClient _client;
        private readonly HttpClient _noCacheClient;

        public ApiClient(
            HttpClient client,
            HttpClient noCacheClient,
            ILogger logger,
            ITokenStorage tokenStorage,
            IApiAppVersion appVersion,
            IAppLanguageCache appLanguageCache,
            string apiVersion) : base(logger, appVersion, tokenStorage, appLanguageCache, apiVersion)
        {
            _client = client;
            _noCacheClient = noCacheClient;
        }

        public async Task<ApiResponseResult<BaseResponse>> GetPingResponseAsync()
        {
            HttpRequestMessage request = GetRequest(HttpMethod.Get, "tests/ping");
            try
            {
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<BaseResponse>(body, response.StatusCode));
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<AuthResponse>> GetAuthResponse(AuthRequestData data)
        {
            HttpRequestMessage request = GetRequest(HttpMethod.Post, "auth");
            try
            {
                request.Content = GetJsonContent(data);

                using (HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false))
                {
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
            HttpRequestMessage request = GetRequest(HttpMethod.Post, "auth/info");
            try
            {
                request.Content = GetJsonContent(data);

                using (HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false))
                {
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ApiResponseResult<AuthInfo> validatedResponse = ApiResponseResult<AuthInfo>(body, response.StatusCode);
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

        public async Task<ApiResponseResult<BaseResponse>> GetTwoFactorAuthResponse(TwoFactorRequestData data, string accessToken, string uid)
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "auth/2fa", accessToken, uid);
            
            try
            {
                request.Content = GetJsonContent(data);
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<BaseResponse>(body, response.StatusCode));
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<VpnInfoResponse>> GetVpnInfoResponse()
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/v2");
            try
            {
                using (HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false))
                {
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Delete, "auth");

            try
            {
                using (HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false))
                {
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
            string id = string.IsNullOrEmpty(lastId) ? "latest" : lastId;
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "events/" + id);

            try
            {
                using (HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false))
                {
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/logicals", ip);
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                System.IO.Stream stream = await response.Content.ReadAsStreamAsync();
                return Logged(GetResponseStreamResult<ServerList>(stream, response.StatusCode), "Get servers");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<ServerList>> GetServerLoadsAsync(string ip)
        {
            try
            {
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/loads", ip);
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                System.IO.Stream stream = await response.Content.ReadAsStreamAsync();
                return Logged(GetResponseStreamResult<ServerList>(stream, response.StatusCode), "Get server loads");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<ReportAnIssueFormData>> GetReportAnIssueFormData()
        {
            try
            {
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/v1/featureconfig/dynamic-bug-reports");
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                System.IO.Stream stream = await response.Content.ReadAsStreamAsync();
                return Logged(GetResponseStreamResult<ReportAnIssueFormData>(stream, response.StatusCode), "Get report an issue form data");
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
                HttpRequestMessage request = GetRequest(HttpMethod.Get, "vpn/location");
                using HttpResponseMessage response = await _noCacheClient.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<UserLocation>(body, response.StatusCode), "Get location data");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<BaseResponse>> ReportBugAsync(IEnumerable<KeyValuePair<string, string>> fields, IEnumerable<File> files)
        {
            MultipartFormDataContent content = new();

            foreach (KeyValuePair<string, string> pair in fields)
            {
                content.Add(new StringContent(pair.Value), $"\"{pair.Key}\"");
            }

            int fileCount = 0;
            foreach (File file in files)
            {
                content.Add(new ByteArrayContent(file.Content),
                    $"\"File{fileCount}\"",
                    $"\"{file.Name}\"");
                fileCount++;
            }

            try
            {
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "reports/bug");
                request.Content = content;
                using HttpResponseMessage response = await _client.SendAsync(request);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/sessions");
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/profiles");
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                System.IO.Stream stream = await response.Content.ReadAsStreamAsync();
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
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "vpn/profiles");
                request.Content = GetJsonContent(profile);
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Put, $"vpn/profiles/{id}");
                request.Content = GetJsonContent(profile);
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Delete, $"vpn/profiles/{id}");
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/v2/clientconfig");
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<VpnConfig>(body, response.StatusCode), "Get VPN config");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<PhysicalServerResponse>> GetServerAsync(string serverId)
        {
            try
            {
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, $"vpn/servers/{serverId}");
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<PhysicalServerResponse>(body, response.StatusCode), "Get server status");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<AnnouncementsResponse>> GetAnnouncementsAsync()
        {
            try
            {
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "core/v4/notifications");
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return Logged(ApiResponseResult<AnnouncementsResponse>(body, response.StatusCode), "Get announcements");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<StreamingServicesResponse>> GetStreamingServicesAsync()
        {
            try
            {
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/streamingservices");
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<StreamingServicesResponse>(body, response.StatusCode), "Get streaming services");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<BaseResponse>> CheckAuthenticationServerStatusAsync()
        {
            try
            {
                HttpRequestMessage request = GetRequest(HttpMethod.Get, "domains/available?Type=login");
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<BaseResponse>(body, response.StatusCode), "Check authentication server status");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<CertificateResponseData>> RequestAuthCertificateAsync(CertificateRequestData requestData)
        {
            try
            {
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "vpn/v1/certificate");
                request.Content = GetJsonContent(requestData);
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Logged(ApiResponseResult<CertificateResponseData>(body, response.StatusCode), "Create auth certificate");
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }
    }
}