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

using System.Collections.Generic;
using System.Threading.Tasks;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Api.Data;
using UserLocation = ProtonVPN.Core.Api.Contracts.UserLocation;

namespace ProtonVPN.Core.Api
{
    public interface IApiClient
    {
        Task<ApiResponseResult<AuthResponse>> GetAuthResponse(AuthRequestData data);
        Task<ApiResponseResult<AuthInfo>> GetAuthInfoResponse(AuthInfoRequestData data);
        Task<ApiResponseResult<VpnInfoResponse>> GetVpnInfoResponse();
        Task<ApiResponseResult<BaseResponse>> GetLogoutResponse();
        Task<ApiResponseResult<EventResponse>> GetEventResponse(string lastId = default);
        Task<ApiResponseResult<ServerList>> GetServersAsync(string ip);
        Task<ApiResponseResult<ServerList>> GetServerLoadsAsync(string ip);
        Task<ApiResponseResult<UserLocation>> GetLocationDataAsync();
        Task<ApiResponseResult<BaseResponse>> ReportBugAsync(IEnumerable<KeyValuePair<string, string>> fields, IEnumerable<File> files);
        Task<ApiResponseResult<SessionsResponse>> GetSessions();
        Task<ApiResponseResult<ProfilesResponse>> GetProfiles();
        Task<ApiResponseResult<ProfileResponse>> CreateProfile(BaseProfile profile);
        Task<ApiResponseResult<ProfileResponse>> UpdateProfile(string id, BaseProfile profile);
        Task<ApiResponseResult<ProfileResponse>> DeleteProfile(string id);
        Task<ApiResponseResult<VpnConfig>> GetVpnConfig();
    }
}
