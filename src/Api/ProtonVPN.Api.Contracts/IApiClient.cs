﻿/*
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

using System.Collections.Generic;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts.Announcements;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Api.Contracts.Certificates;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Api.Contracts.Events;
using ProtonVPN.Api.Contracts.Features;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Api.Contracts.Partners;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Api.Contracts.Streaming;
using ProtonVPN.Api.Contracts.Users;
using ProtonVPN.Api.Contracts.VpnConfig;
using ProtonVPN.Api.Contracts.VpnSessions;

namespace ProtonVPN.Api.Contracts;

public interface IApiClient : IClientBase
{
    Task<ApiResponseResult<UnauthSessionResponse>> PostUnauthSessionAsync();
    Task<ApiResponseResult<AuthResponse>> GetAuthResponse(AuthRequest authRequest);
    Task<ApiResponseResult<AuthInfoResponse>> GetAuthInfoResponse(AuthInfoRequest authInfoRequest);
    Task<ApiResponseResult<BaseResponse>> GetTwoFactorAuthResponse(TwoFactorRequest twoFactorRequest, string accessToken, string uid);
    Task<ApiResponseResult<PhysicalServerWrapperResponse>> GetServerAsync(string serverId);
    Task<ApiResponseResult<VpnInfoWrapperResponse>> GetVpnInfoResponse();
    Task<ApiResponseResult<BaseResponse>> GetLogoutResponse();
    Task<ApiResponseResult<EventResponse>> GetEventResponse(string lastId = default);
    Task<ApiResponseResult<ServersResponse>> GetServersAsync(string ip);
    Task<ApiResponseResult<ReportAnIssueFormResponse>> GetReportAnIssueFormData();
    Task<ApiResponseResult<ServersResponse>> GetServerLoadsAsync(string ip);
    Task<ApiResponseResult<DeviceLocationResponse>> GetLocationDataAsync();
    Task<ApiResponseResult<BaseResponse>> ReportBugAsync(IEnumerable<KeyValuePair<string, string>> fields, IEnumerable<File> files);
    Task<ApiResponseResult<SessionsResponse>> GetSessions();
    Task<ApiResponseResult<VpnConfigResponse>> GetVpnConfig();
    Task<ApiResponseResult<AnnouncementsResponse>> GetAnnouncementsAsync(AnnouncementsRequest request);
    Task<ApiResponseResult<StreamingServicesResponse>> GetStreamingServicesAsync();
    Task<ApiResponseResult<PartnersResponse>> GetPartnersAsync();
    Task<ApiResponseResult<BaseResponse>> CheckAuthenticationServerStatusAsync();
    Task<ApiResponseResult<CertificateResponse>> RequestConnectionCertificateAsync(CertificateRequest request);
    Task<ApiResponseResult<BaseResponse>> ApplyPromoCodeAsync(PromoCodeRequest promoCodeRequest);
    Task<ApiResponseResult<ForkedAuthSessionResponse>> ForkAuthSessionAsync(AuthForkSessionRequest request);
    Task<ApiResponseResult<UsersResponse>> GetUserAsync();
    Task<ApiResponseResult<FeatureFlagsResponse>> GetFeatureFlagsAsync();
}