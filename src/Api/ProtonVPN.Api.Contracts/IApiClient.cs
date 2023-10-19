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

using System.Collections.Generic;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts.Announcements;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Api.Contracts.Certificates;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Api.Contracts.Settings;
using ProtonVPN.Api.Contracts.Streaming;
using ProtonVPN.Api.Contracts.Users;
using ProtonVPN.Api.Contracts.VpnSessions;
using ProtonVPN.Common.StatisticalEvents;

namespace ProtonVPN.Api.Contracts
{
    public interface IApiClient : IClientBase
    {
        Task<ApiResponseResult<AuthResponse>> GetAuthResponse(AuthRequest authRequest);
        Task<ApiResponseResult<AuthInfoResponse>> GetAuthInfoResponse(AuthInfoRequest authInfoRequest);
        Task<ApiResponseResult<BaseResponse>> GetTwoFactorAuthResponse(TwoFactorRequest twoFactorRequest, string accessToken, string uid);
        Task<ApiResponseResult<PhysicalServerWrapperResponse>> GetServerAsync(string serverId);
        Task<ApiResponseResult<VpnInfoWrapperResponse>> GetVpnInfoResponse();
        Task<ApiResponseResult<BaseResponse>> GetLogoutResponse();
        Task<ApiResponseResult<ServersResponse>> GetServersAsync(string ip);
        Task<ApiResponseResult<ReportAnIssueFormResponse>> GetReportAnIssueFormData();
        Task<ApiResponseResult<ServersResponse>> GetServerLoadsAsync(string ip);
        Task<ApiResponseResult<UserLocationResponse>> GetLocationDataAsync();
        Task<ApiResponseResult<BaseResponse>> ReportBugAsync(IEnumerable<KeyValuePair<string, string>> fields, IEnumerable<File> files);
        Task<ApiResponseResult<SessionsResponse>> GetSessions();
        Task<ApiResponseResult<VpnConfig.VpnConfigResponse>> GetVpnConfig();
        Task<ApiResponseResult<AnnouncementsResponse>> GetAnnouncementsAsync(AnnouncementsRequest request);
        Task<ApiResponseResult<StreamingServicesResponse>> GetStreamingServicesAsync();
        Task<ApiResponseResult<BaseResponse>> CheckAuthenticationServerStatusAsync();
        Task<ApiResponseResult<CertificateResponse>> RequestAuthCertificateAsync(CertificateRequest request);
        Task<ApiResponseResult<BaseResponse>> ApplyPromoCodeAsync(PromoCodeRequest promoCodeRequest);
        Task<ApiResponseResult<ForkedAuthSessionResponse>> ForkAuthSessionAsync(AuthForkSessionRequest request);
        Task<ApiResponseResult<SettingsResponse>> GetSettingsAsync();
        Task<ApiResponseResult<BaseResponse>> PostStatisticalEventsAsync(StatisticalEventsBatch statisticalEvents);
        Task<ApiResponseResult<UsersResponse>> GetUserAsync();
    }
}