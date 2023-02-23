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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Announcements;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Api.Contracts.Certificates;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Api.Contracts.Events;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Api.Contracts.Partners;
using ProtonVPN.Api.Contracts.Profiles;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Api.Contracts.Streaming;
using ProtonVPN.Api.Contracts.VpnConfig;
using ProtonVPN.Api.Contracts.VpnSessions;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ApiLogs;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Api
{
    public class ApiClient : BaseApiClient, IApiClient
    {
        private const int SERVERS_TIMEOUT_IN_SECONDS = 30;
        private const int SERVERS_RETRY_COUNT = 3;

        private readonly HttpClient _client;
        private readonly HttpClient _noCacheClient;

        public ApiClient(
            IApiHttpClientFactory httpClientFactory,
            ILogger logger,
            IAppSettings appSettings,
            IApiAppVersion appVersion,
            IAppLanguageCache appLanguageCache,
            IConfiguration config) : base(logger, appVersion, appSettings, appLanguageCache, config)
        {
            _client = httpClientFactory.GetApiHttpClientWithCache();
            _noCacheClient = httpClientFactory.GetApiHttpClientWithoutCache();
        }

        public async Task<ApiResponseResult<AuthResponse>> GetAuthResponse(AuthRequest authRequest)
        {
            HttpRequestMessage request = GetRequest(HttpMethod.Post, "auth");
            request.Content = GetJsonContent(authRequest);
            return await SendRequest<AuthResponse>(request, "Get auth");
        }

        public async Task<ApiResponseResult<AuthInfoResponse>> GetAuthInfoResponse(AuthInfoRequest authInfoRequest)
        {
            HttpRequestMessage request = GetRequest(HttpMethod.Post, "auth/info");
            request.Content = GetJsonContent(authInfoRequest);
            return await SendRequest<AuthInfoResponse>(request, "Get auth info");
        }

        public async Task<ApiResponseResult<BaseResponse>> GetTwoFactorAuthResponse(TwoFactorRequest twoFactorRequest, string accessToken, string uid)
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "auth/2fa", accessToken, uid);
            request.Content = GetJsonContent(twoFactorRequest);
            return await SendRequest<BaseResponse>(request, "Get two factor auth info");
        }

        public async Task<ApiResponseResult<VpnInfoWrapperResponse>> GetVpnInfoResponse()
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/v2");
            return await SendRequest<VpnInfoWrapperResponse>(request, "Get VPN info");
        }

        public async Task<ApiResponseResult<BaseResponse>> GetLogoutResponse()
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Delete, "auth");
            return await SendRequest<BaseResponse>(request, "Logout");
        }

        public async Task<ApiResponseResult<EventResponse>> GetEventResponse(string lastId)
        {
            string id = string.IsNullOrEmpty(lastId) ? "latest" : lastId;
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "events/" + id);
            return await SendRequest<EventResponse>(request, "Get events");
        }

        public async Task<ApiResponseResult<ServersResponse>> GetServersAsync(string ip)
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get,
                "vpn/logicals?SignServer=Server.EntryIP,Server.Label&WithPartnerLogicals=1", ip);
            request.SetRetryCount(SERVERS_RETRY_COUNT);
            request.SetCustomTimeout(TimeSpan.FromSeconds(SERVERS_TIMEOUT_IN_SECONDS));
            return await SendRequest<ServersResponse>(request, "Get servers");
        }

        public async Task<ApiResponseResult<ServersResponse>> GetServerLoadsAsync(string ip)
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/loads", ip);
            return await SendRequest<ServersResponse>(request, "Get server loads");
        }

        public async Task<ApiResponseResult<ReportAnIssueFormResponse>> GetReportAnIssueFormData()
        {
            HttpRequestMessage request = GetRequest(HttpMethod.Get, "vpn/v1/featureconfig/dynamic-bug-reports");
            return await SendRequest<ReportAnIssueFormResponse>(request, "Get report an issue form data");
        }

        public async Task<ApiResponseResult<UserLocationResponse>> GetLocationDataAsync()
        {
            HttpRequestMessage request = GetRequest(HttpMethod.Get, "vpn/location");
            return await SendRequestWithNoCache<UserLocationResponse>(request, "Get location data");
        }

        public async Task<ApiResponseResult<BaseResponse>> ReportBugAsync(
            IEnumerable<KeyValuePair<string, string>> fields, IEnumerable<File> files)
        {
            MultipartFormDataContent content = new();

            foreach (KeyValuePair<string, string> pair in fields)
            {
                content.Add(new StringContent(pair.Value ?? "undefined"), $"\"{pair.Key}\"");
            }

            int fileCount = 0;
            foreach (File file in files)
            {
                content.Add(new ByteArrayContent(file.Content),
                    $"\"File{fileCount}\"",
                    $"\"{file.Name}\"");
                fileCount++;
            }

            HttpRequestMessage request = GetRequest(HttpMethod.Post, "reports/bug");
            request.Content = content;
            return await SendRequest<BaseResponse>(request, "Report bug");
        }

        public async Task<ApiResponseResult<SessionsResponse>> GetSessions()
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/sessions");
            return await SendRequest<SessionsResponse>(request, "Get sessions");
        }

        public async Task<ApiResponseResult<ProfilesResponse>> GetProfiles()
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/profiles");
            return await SendRequest<ProfilesResponse>(request, "Get profiles");
        }

        public async Task<ApiResponseResult<ProfileWrapperResponse>> CreateProfile(BaseProfileResponse profile)
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "vpn/profiles");
            request.Content = GetJsonContent(profile);
            return await SendRequest<ProfileWrapperResponse>(request, "Create profile");
        }

        public async Task<ApiResponseResult<ProfileWrapperResponse>> UpdateProfile(string id,
            BaseProfileResponse profile)
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Put, $"vpn/profiles/{id}");
            request.Content = GetJsonContent(profile);
            return await SendRequest<ProfileWrapperResponse>(request, "Update profile");
        }

        public async Task<ApiResponseResult<ProfileWrapperResponse>> DeleteProfile(string id)
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Delete, $"vpn/profiles/{id}");
            return await SendRequest<ProfileWrapperResponse>(request, "Delete profile");
        }

        public async Task<ApiResponseResult<VpnConfigResponse>> GetVpnConfig()
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/v2/clientconfig");
            return await SendRequest<VpnConfigResponse>(request, "Get VPN config");
        }

        public async Task<ApiResponseResult<PhysicalServerWrapperResponse>> GetServerAsync(string serverId)
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, $"vpn/servers/{serverId}");
            return await SendRequest<PhysicalServerWrapperResponse>(request, "Get server status");
        }

        public async Task<ApiResponseResult<AnnouncementsResponse>> GetAnnouncementsAsync(
            AnnouncementsRequest announcementsRequest)
        {
            string url = "core/v4/notifications?" +
                         $"FullScreenImageSupport={announcementsRequest.FullScreenImageSupport}&" +
                         $"FullScreenImageWidth={announcementsRequest.FullScreenImageWidth}&" +
                         $"FullScreenImageHeight={announcementsRequest.FullScreenImageHeight}";
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, url);
            return await SendRequest<AnnouncementsResponse>(request, "Get announcements");
        }

        public async Task<ApiResponseResult<StreamingServicesResponse>> GetStreamingServicesAsync()
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/streamingservices");
            return await SendRequest<StreamingServicesResponse>(request, "Get streaming services");
        }

        public async Task<ApiResponseResult<PartnersResponse>> GetPartnersAsync()
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/v1/partners");
            return await SendRequest<PartnersResponse>(request, "Get partners");
        }

        public async Task<ApiResponseResult<BaseResponse>> CheckAuthenticationServerStatusAsync()
        {
            HttpRequestMessage request = GetRequest(HttpMethod.Get, "domains/available?Type=login");
            return await SendRequest<BaseResponse>(request, "Check authentication server status");
        }

        public async Task<ApiResponseResult<CertificateResponse>> RequestAuthCertificateAsync(
            CertificateRequest certificateRequest)
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "vpn/v1/certificate");
            request.Content = GetJsonContent(certificateRequest);
            return await SendRequest<CertificateResponse>(request, "Create auth certificate");
        }

        public async Task<ApiResponseResult<BaseResponse>> ApplyPromoCodeAsync(PromoCodeRequest promoCodeRequest)
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "payments/v4/promocode");
            request.Content = GetJsonContent(promoCodeRequest);
            return await SendRequest<BaseResponse>(request, "Apply promo code");
        }

        public async Task<ApiResponseResult<ForkedAuthSessionResponse>> ForkAuthSessionAsync(AuthForkSessionRequest authForkSessionRequest)
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "auth/v4/sessions/forks");
            request.SetCustomTimeout(TimeSpan.FromSeconds(3));
            request.Content = GetJsonContent(authForkSessionRequest);
            return await SendRequest<ForkedAuthSessionResponse>(request, "Fork auth session");
        }

        private async Task<ApiResponseResult<T>> SendRequest<T>(HttpRequestMessage request, string logDescription)
            where T : BaseResponse
        {
            return await SendRequest<T>(_client, request, logDescription);
        }

        private async Task<ApiResponseResult<T>> SendRequestWithNoCache<T>(HttpRequestMessage request,
            string logDescription) where T : BaseResponse
        {
            return await SendRequest<T>(_noCacheClient, request, logDescription);
        }

        private async Task<ApiResponseResult<T>> SendRequest<T>(HttpClient httpClient, HttpRequestMessage request,
            string logDescription) where T : BaseResponse
        {
            try
            {
                using (HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    return Logged(await GetApiResponseResult<T>(response), logDescription);
                }
            }
            catch (Exception e)
            {
                if (!e.IsApiCommunicationException())
                {
                    Logger.Error<ApiErrorLog>("An exception occurred in an API request " +
                        "that is not related with its communication.", e);
                }
                throw new HttpRequestException(e.Message, e);
            }
        }
    }
}