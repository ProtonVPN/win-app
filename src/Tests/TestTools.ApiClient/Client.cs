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
using ProtonVPN.Api;
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
using ProtonVPN.Core.Settings;

namespace TestTools.ApiClient
{
    public class Client : BaseApiClient, IApiClient
    {
        private readonly HttpClient _client;

        public Client(
            IConfiguration config,
            ILogger logger,
            HttpClient client,
            IAppSettings appSettings,
            IAppLanguageCache appLanguageCache) 
            : base(logger, new ApiAppVersion(), appSettings, appLanguageCache, config)
        {
            _client = client;
        }

        public async Task<ApiResponseResult<AuthResponse>> GetAuthResponse(AuthRequest authRequest)
        {
            HttpRequestMessage request = GetRequest(HttpMethod.Post, "auth");
            try
            {
                request.Content = GetJsonContent(authRequest);

                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                return await GetApiResponseResult<AuthResponse>(response);
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<AuthInfoResponse>> GetAuthInfoResponse(AuthInfoRequest authInfoRequest)
        {
            HttpRequestMessage request = GetRequest(HttpMethod.Post, "auth/info");
            try
            {
                request.Content = GetJsonContent(authInfoRequest);

                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                ApiResponseResult<AuthInfoResponse> validatedResponse = await GetApiResponseResult<AuthInfoResponse>(response);
                if (validatedResponse.Success && string.IsNullOrEmpty(validatedResponse.Value.Salt))
                {
                    return ProtonVPN.Api.Contracts.ApiResponseResult<AuthInfoResponse>.Fail(response,
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
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/profiles");
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                return await GetResponseStreamResult<ProfilesResponse>(response);
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public async Task<ApiResponseResult<ProfileWrapperResponse>> DeleteProfile(string id)
        {
            try
            {
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Delete, $"vpn/profiles/{id}");
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                return await GetApiResponseResult<ProfileWrapperResponse>(response);
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public Task<ApiResponseResult<VpnInfoWrapperResponse>> GetVpnInfoResponse() => throw new NotImplementedException();

        public Task<ApiResponseResult<BaseResponse>> GetTwoFactorAuthResponse(TwoFactorRequest twoFactorRequest, string accessToken, string uid) => throw new NotImplementedException();

        public Task<ApiResponseResult<ReportAnIssueFormResponse>> GetReportAnIssueFormData() => throw new NotImplementedException();

        public Task<ApiResponseResult<BaseResponse>> GetLogoutResponse() => throw new NotImplementedException();

        public Task<ApiResponseResult<EventResponse>> GetEventResponse(string lastId = default) => throw new NotImplementedException();

        public Task<ApiResponseResult<ServersResponse>> GetServersAsync(string ip) => throw new NotImplementedException();

        public Task<ApiResponseResult<ServersResponse>> GetServerLoadsAsync(string ip) => throw new NotImplementedException();

        public async Task<ApiResponseResult<UserLocationResponse>> GetLocationDataAsync()
        {
            try
            {
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/location");
                using HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);
                return await GetResponseStreamResult<UserLocationResponse>(response);
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message, e);
            }
        }

        public Task<ApiResponseResult<BaseResponse>> ReportBugAsync(IEnumerable<KeyValuePair<string, string>> fields, IEnumerable<File> files) => throw new NotImplementedException();

        public Task<ApiResponseResult<SessionsResponse>> GetSessions() => throw new NotImplementedException();

        public Task<ApiResponseResult<ProfileWrapperResponse>> CreateProfile(BaseProfileResponse profile) => throw new NotImplementedException();

        public Task<ApiResponseResult<ProfileWrapperResponse>> UpdateProfile(string id, BaseProfileResponse profile) => throw new NotImplementedException();

        public Task<ApiResponseResult<VpnConfigResponse>> GetVpnConfig() => throw new NotImplementedException();
        
        public Task<ApiResponseResult<AnnouncementsResponse>> GetAnnouncementsAsync(AnnouncementsRequest request) => throw new NotImplementedException();

        public Task<ApiResponseResult<PhysicalServerWrapperResponse>> GetServerAsync(string serverId) => throw new NotImplementedException();

        public Task<ApiResponseResult<AnnouncementsResponse>> GetAnnouncementsAsync() => throw new NotImplementedException();

        public Task<ApiResponseResult<StreamingServicesResponse>> GetStreamingServicesAsync() => throw new NotImplementedException();
        public Task<ApiResponseResult<PartnersResponse>> GetPartnersAsync() => throw new NotImplementedException();

        public Task<ApiResponseResult<BaseResponse>> CheckAuthenticationServerStatusAsync() => throw new NotImplementedException();

        public Task<ApiResponseResult<CertificateResponse>> RequestAuthCertificateAsync(CertificateRequest request) => throw new NotImplementedException();

        public Task<ApiResponseResult<BaseResponse>> ApplyPromoCodeAsync(PromoCodeRequest promoCodeRequest) => throw new NotImplementedException();

        public Task<ApiResponseResult<ForkedAuthSessionResponse>> ForkAuthSessionAsync(AuthForkSessionRequest request) => throw new NotImplementedException();
    }
}