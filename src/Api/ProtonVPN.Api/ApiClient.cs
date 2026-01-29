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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Announcements;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Api.Contracts.Certificates;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Api.Contracts.Events;
using ProtonVPN.Api.Contracts.Features;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Api.Contracts.NpsSurvey;
using ProtonVPN.Api.Contracts.Partners;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Api.Contracts.Streaming;
using ProtonVPN.Api.Contracts.Users;
using ProtonVPN.Api.Contracts.VpnConfig;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Common.Core.StatisticalEvents;
using ProtonVPN.Common.Legacy.OS.Net.Http;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;

namespace ProtonVPN.Api;

public class ApiClient : BaseApiClient, IApiClient
{
    private const int SERVERS_TIMEOUT_IN_SECONDS = 30;
    private const int SERVERS_RETRY_COUNT = 3;
    private const int CERTIFICATE_RETRY_COUNT = 5;

    private const string LOGICALS_SIGN_SERVER_PARAM_VALUE = "Server.EntryIP,Server.Label";
    private const string LOGICALS_PROTOCOL_ENTRIES_PARAM_VALUE = "WireGuardUDP,WireGuardTCP,WireGuardTLS,OpenVPNUDP,OpenVPNTCP";

    private readonly HttpClient _client;
    private readonly HttpClient _noCacheClient;

    public ApiClient(
        IApiHttpClientFactory httpClientFactory,
        ILogger logger,
        IApiAppVersion appVersion,
        ISettings settings,
        IConfiguration config) : base(logger, appVersion, settings, config)
    {
        _client = httpClientFactory.GetApiHttpClientWithCache();
        _noCacheClient = httpClientFactory.GetApiHttpClientWithoutCache();
    }

    public async Task<ApiResponseResult<UnauthSessionResponse>> PostUnauthSessionAsync(CancellationToken cancellationToken = default)
    {
        HttpRequestMessage request = GetUnauthorizedRequest(HttpMethod.Post, "auth/v4/sessions");

        return await SendRequestAsync<UnauthSessionResponse>(request, cancellationToken, "Post unauth sessions");
    }

    public async Task<ApiResponseResult<AuthResponse>> GetAuthResponse(AuthRequest authRequest, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = GetRequest(HttpMethod.Post, "auth");
        request.Content = GetJsonContent(authRequest);
        return await SendRequestAsync<AuthResponse>(request, cancellationToken, "Get auth");
    }

    public async Task<ApiResponseResult<AuthInfoResponse>> GetAuthInfoResponse(AuthInfoRequest authInfoRequest, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = GetRequest(HttpMethod.Post, "auth/info");
        request.Content = GetJsonContent(authInfoRequest);
        return await SendRequestAsync<AuthInfoResponse>(request, cancellationToken, "Get auth info");
    }

    public async Task<ApiResponseResult<BaseResponse>> GetTwoFactorAuthResponse(
        TwoFactorRequest twoFactorRequest,
        string accessToken,
        string uid,
        CancellationToken cancellationToken)
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "auth/v4/2fa", accessToken, uid);
        request.Content = GetJsonContent(twoFactorRequest);
        return await SendRequestAsync<BaseResponse>(request, cancellationToken, "Get two factor auth info");
    }

    public async Task<ApiResponseResult<VpnInfoWrapperResponse>> GetVpnInfoResponse(CancellationToken cancellationToken)
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/v2");
        return await SendRequestAsync<VpnInfoWrapperResponse>(request, cancellationToken, "Get VPN info");
    }

    public async Task<ApiResponseResult<BaseResponse>> GetLogoutResponse()
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Delete, "auth");
        return await SendRequestAsync<BaseResponse>(request, CancellationToken.None, "Logout");
    }

    public async Task<ApiResponseResult<EventResponse>> GetEventResponse(string lastId)
    {
        string id = string.IsNullOrEmpty(lastId) ? "latest" : lastId;
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "events/" + id);
        return await SendRequestAsync<EventResponse>(request, CancellationToken.None, "Get events");
    }

    public async Task<ApiResponseResult<ServersResponse>> GetServersAsync(
        DeviceLocation? deviceLocation,
        bool isServerListTruncationEnabled,
        bool useLegacyEndpoint,
        IEnumerable<string> favoriteServerIds = null,
        CancellationToken cancellationToken = default)
    {
        string endpointUrl = useLegacyEndpoint
            ? "vpn/v1/logicals"
            : "vpn/v2/logicals";

        StringBuilder endpoint = new(endpointUrl +
            $"?SignServer={LOGICALS_SIGN_SERVER_PARAM_VALUE}" +
            "&SecureCoreFilter=all" +
            "&WithState=true" +
            $"&WithEntriesForProtocols={LOGICALS_PROTOCOL_ENTRIES_PARAM_VALUE}");

        if (isServerListTruncationEnabled && favoriteServerIds is not null && favoriteServerIds.Any())
        {
            foreach (string requestServerId in favoriteServerIds)
            {
                endpoint.Append($"&IncludeID[]={requestServerId}");
            }
        }

        HttpRequestMessage request = GetAuthorizedRequestWithLocation(HttpMethod.Get, endpoint.ToString(), deviceLocation);
        request.SetRetryCount(SERVERS_RETRY_COUNT);
        request.SetCustomTimeout(TimeSpan.FromSeconds(SERVERS_TIMEOUT_IN_SECONDS));
        request.Headers.IfModifiedSince = Settings.LogicalsLastModifiedDate;

        if (isServerListTruncationEnabled)
        {
            request.Headers.Add("x-pm-response-truncation-permitted", "true");
        }

        return await SendRequestAsync<ServersResponse>(request, cancellationToken, "Get servers");
    }

    public async Task<ApiResponseResult<byte[]>> GetServerLoadsAndStatusBinaryStringAsync(string statusId, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, $"vpn/v2/status/{statusId}/binary");

        return await SendRequestAsync<byte[]>(request, cancellationToken, "Get server loads and status binary string");
    }

    public async Task<ApiResponseResult<ServerCountResponse>> GetServersCountAsync()
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/servers-count");
        return await SendRequestAsync<ServerCountResponse>(request, CancellationToken.None, "Get servers and countries count");
    }

    public async Task<ApiResponseResult<ServersResponse>> GetServerLoadsAsync(DeviceLocation? deviceLocation, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = GetAuthorizedRequestWithLocation(HttpMethod.Get, "vpn/loads", deviceLocation);
        return await SendRequestAsync<ServersResponse>(request, cancellationToken, "Get server loads");
    }

    public async Task<ApiResponseResult<ReportAnIssueFormResponse>> GetReportAnIssueFormData()
    {
        HttpRequestMessage request = GetRequest(HttpMethod.Get, "vpn/v1/featureconfig/dynamic-bug-reports");
        return await SendRequestAsync<ReportAnIssueFormResponse>(request, CancellationToken.None, "Get report an issue form data");
    }

    public async Task<ApiResponseResult<DeviceLocationResponse>> GetLocationDataAsync()
    {
        HttpRequestMessage request = GetRequest(HttpMethod.Get, "vpn/location");
        return await SendRequestWithNoCacheAsync<DeviceLocationResponse>(request, CancellationToken.None, "Get location data");
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
        return await SendRequestAsync<BaseResponse>(request, CancellationToken.None, "Report bug");
    }

    public async Task<ApiResponseResult<VpnConfigResponse>> GetVpnConfigAsync(DeviceLocation? deviceLocation, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = GetAuthorizedRequestWithLocation(HttpMethod.Get, "vpn/v2/clientconfig", deviceLocation);
        return await SendRequestAsync<VpnConfigResponse>(request, cancellationToken, "Get VPN config");
    }

    public async Task<ApiResponseResult<PhysicalServerWrapperResponse>> GetServerAsync(string serverId)
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, $"vpn/servers/{serverId}");
        return await SendRequestAsync<PhysicalServerWrapperResponse>(request, CancellationToken.None, "Get server status");
    }

    public async Task<ApiResponseResult<AnnouncementsResponse>> GetAnnouncementsAsync(
        AnnouncementsRequest announcementsRequest)
    {
        string url = "core/v4/notifications?" +
                     $"FullScreenImageSupport={announcementsRequest.FullScreenImageSupport}&" +
                     $"FullScreenImageWidth={announcementsRequest.FullScreenImageWidth}&" +
                     $"FullScreenImageHeight={announcementsRequest.FullScreenImageHeight}";
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, url);
        return await SendRequestAsync<AnnouncementsResponse>(request, CancellationToken.None, "Get announcements");
    }

    public async Task<ApiResponseResult<StreamingServicesResponse>> GetStreamingServicesAsync()
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/streamingservices");
        return await SendRequestAsync<StreamingServicesResponse>(request, CancellationToken.None, "Get streaming services");
    }

    public async Task<ApiResponseResult<PartnersResponse>> GetPartnersAsync()
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/v1/partners");
        return await SendRequestAsync<PartnersResponse>(request, CancellationToken.None, "Get partners");
    }

    public async Task<ApiResponseResult<CertificateResponse>> RequestConnectionCertificateAsync(
        CertificateRequest certificateRequest,
        CancellationToken cancellationToken)
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "vpn/v1/certificate");
        request.Content = GetJsonContent(certificateRequest);
        request.SetRetryCount(CERTIFICATE_RETRY_COUNT);
        return await SendRequestAsync<CertificateResponse>(request, cancellationToken, "Create connection certificate");
    }

    public async Task<ApiResponseResult<BaseResponse>> ApplyPromoCodeAsync(PromoCodeRequest promoCodeRequest)
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "payments/v4/promocode");
        request.Content = GetJsonContent(promoCodeRequest);
        return await SendRequestAsync<BaseResponse>(request, CancellationToken.None, "Apply promo code");
    }

    public async Task<ApiResponseResult<ForkedAuthSessionResponse>> ForkAuthSessionAsync(AuthForkSessionRequest authForkSessionRequest)
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "auth/v4/sessions/forks");
        request.SetCustomTimeout(TimeSpan.FromSeconds(3));
        request.Content = GetJsonContent(authForkSessionRequest);
        return await SendRequestAsync<ForkedAuthSessionResponse>(request, CancellationToken.None, "Fork auth session");
    }

    public async Task<ApiResponseResult<BaseResponse>> PostUnauthenticatedStatisticalEventsAsync(StatisticalEventsBatch statisticalEvents)
    {
        HttpRequestMessage request = GetRequest(HttpMethod.Post, "data/v1/stats/multiple");
        request.Content = GetJsonContent(statisticalEvents);
        return await SendRequestAsync<BaseResponse>(request, CancellationToken.None, "Post unauthenticated statistical events batch");
    }

    public async Task<ApiResponseResult<BaseResponse>> PostAuthenticatedStatisticalEventsAsync(StatisticalEventsBatch statisticalEvents)
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "data/v1/stats/multiple");
        request.Content = GetJsonContent(statisticalEvents);
        return await SendRequestAsync<BaseResponse>(request, CancellationToken.None, "Post authenticated statistical events batch");
    }

    public async Task<ApiResponseResult<UsersResponse>> GetUserAsync(CancellationToken cancellationToken = default)
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "core/v4/users");
        return await SendRequestAsync<UsersResponse>(request, cancellationToken, "Get user");
    }

    public async Task<ApiResponseResult<Ipv6FragmentsResponse>> GetIpv6FragmentsAsync(CancellationToken cancellationToken = default)
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Get, "vpn/v1/ipv6-fragments");
        return await SendRequestAsync<Ipv6FragmentsResponse>(request, cancellationToken, "Get IPv6 fragments");
    }

    public async Task<ApiResponseResult<FeatureFlagsResponse>> GetFeatureFlagsAsync(CancellationToken cancellationToken = default)
    {
        HttpRequestMessage request = GetRequest(HttpMethod.Get, "feature/v2/frontend");
        return await SendRequestAsync<FeatureFlagsResponse>(request, cancellationToken, "Get feature flags");
    }

    public async Task<ApiResponseResult<BaseResponse>> SubmitNpsSurveyAsync(NpsSurveyRequest npsSurveyRequest)
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "vpn/v1/nps/submit");
        request.Content = GetJsonContent(npsSurveyRequest);
        return await SendRequestAsync<BaseResponse>(request, CancellationToken.None, "Submit NPS survey");
    }

    public async Task<ApiResponseResult<BaseResponse>> DismissNpsSurveyAsync()
    {
        HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "vpn/v1/nps/dismiss");
        return await SendRequestAsync<BaseResponse>(request, CancellationToken.None, "Dismiss NPS survey");
    }

    public async Task<ApiResponseResult<LookupServerResponse>> GetServerByNameAsync(string serverName, DeviceLocation? deviceLocation)
    {
        serverName = Uri.EscapeDataString(serverName);

        StringBuilder endpoint = new($"vpn/v1/logicals/lookup/{serverName}" +
            $"?SignServer={LOGICALS_SIGN_SERVER_PARAM_VALUE}" +
            $"&WithEntriesForProtocols={LOGICALS_PROTOCOL_ENTRIES_PARAM_VALUE}");

        HttpRequestMessage request = GetAuthorizedRequestWithLocation(HttpMethod.Get, endpoint.ToString(), deviceLocation);
        request.SetRetryCount(0);
        return await SendRequestAsync<LookupServerResponse>(request, CancellationToken.None, $"Get server by name ({serverName})");
    }

    private async Task<ApiResponseResult<T>> SendRequestAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken,
        string logDescription)
    {
        return await SendRequestAsync<T>(_client, request, cancellationToken, logDescription);
    }

    private async Task<ApiResponseResult<T>> SendRequestWithNoCacheAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken,
        string logDescription)
    {
        return await SendRequestAsync<T>(_noCacheClient, request, cancellationToken, logDescription);
    }

    private async Task<ApiResponseResult<T>> SendRequestAsync<T>(
        HttpClient httpClient,
        HttpRequestMessage request,
        CancellationToken cancellationToken,
        string logDescription)
    {
        try
        {
            using (HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
            {
                return Logged(await GetApiResponseResultAsync<T>(response, cancellationToken), logDescription);
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